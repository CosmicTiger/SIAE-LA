using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Utils;

namespace SIAE_LA.Infrastructure;

public class DataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DataSeeder> _logger;
    private readonly IConfiguration _config;
    // Id of the user to record in audit fields when seeding (root/system user)
    private string? _seedUserId;

    public DataSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<DataSeeder> logger,
        IConfiguration config)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
        _config = config;
    }

    public async Task SeedAsync()
    {
        // 1) Migraciones
        _logger?.LogInformation("Starting database migrations...");
        await _db.Database.MigrateAsync();
        _logger?.LogInformation("Migrations applied.");
        // default seed user id when no real user exists yet
        _seedUserId = "system";

        // 2) Roles + root admin
        await SeedRolesAsync();
        await SeedRootAdminAsync();

        // 3) Catálogos del ER
        await SeedCatalogsAsync();

        // 4) Año lectivo demo + periodos (4 bimestres)
        // Default demo year — configurable via appsettings:Siaela:DemoYear or env SIAE_DEMO_YEAR
        var demoYear = _config?.GetValue<int?>("Siae:DemoYear")
            ?? (int.TryParse(Environment.GetEnvironmentVariable("SIAE_DEMO_YEAR"), out var y) ? y : (int?)null)
            ?? 2026;
        _logger?.LogInformation("Seeding demo academic year: {Year}", demoYear);
        await SeedAcademicYearDemoAsync(demoYear);

        // 5) Personas / Usuarios / Vínculos (docentes, dirección, alumnos + tutores)
        _logger?.LogInformation("Seeding people and users for year {Year}", demoYear);
        await SeedPeopleAndUsersAsync(demoYear);
        _logger?.LogInformation("Data seeding completed.");
    }

    // Save changes while assigning audit shadow properties for seeding operations
    private async Task<int> SaveChangesWithAuditAsync(CancellationToken ct = default)
    {
        var userId = _seedUserId;
        var now = DateTime.UtcNow;
        foreach (var entry in _db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            if (entry.State == EntityState.Added)
            {
                var creadoProp = entry.Metadata.FindProperty("CreadoPor");
                if (creadoProp != null)
                    entry.Property(creadoProp.Name).CurrentValue = userId;

                var fechaRegistro = entry.Metadata.FindProperty("FechaRegistro");
                if (fechaRegistro != null)
                {
                    var cur = entry.Property(fechaRegistro.Name).CurrentValue;
                    if (cur == null || (cur is DateTime dt && dt == default))
                        entry.Property(fechaRegistro.Name).CurrentValue = now;
                }
            }

            var modProp = entry.Metadata.FindProperty("ModificadoPor");
            if (modProp != null)
                entry.Property(modProp.Name).CurrentValue = userId;

            var fechaMod = entry.Metadata.FindProperty("FechaModificacion");
            if (fechaMod != null)
                entry.Property(fechaMod.Name).CurrentValue = now;
            
            // Ensure any DateTime properties have Kind==Utc. Npgsql requires UTC for timestamptz.
            foreach (var prop in entry.Properties)
            {
                var val = prop.CurrentValue;
                if (val is DateTime dt)
                {
                    if (dt.Kind == DateTimeKind.Unspecified)
                        prop.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    else if (dt.Kind == DateTimeKind.Local)
                        prop.CurrentValue = dt.ToUniversalTime();
                }
            }
        }

        return await _db.SaveChangesAsync(ct);
    }

    private static DateTime UtcDate(int y, int m, int d)
    => new DateTime(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime AsUtcDate(DateTime dt)
        => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);

    // Seed an academic year (AnioLectivo) and four bimesters (Periodos)
    private async Task SeedAcademicYearDemoAsync(int year, CancellationToken ct = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            _logger?.LogInformation("Seeding academic year {Year}", year);
            var existing = await _db.AniosLectivos.AsNoTracking().FirstOrDefaultAsync(a => a.Anio == year, ct);
            AnioLectivo anio;
            if (existing is null)
            {
                _logger?.LogInformation("Creating AnioLectivo for {Year}", year);
                anio = new AnioLectivo
                {
                    Anio = year,
                    Descripcion = $"Año lectivo {year}",
                    Activo = true,
                    FechaInicio = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                };
                _db.AniosLectivos.Add(anio);
                await SaveChangesWithAuditAsync(ct);
            }
            else
            {
                _logger?.LogInformation("AnioLectivo {Year} already exists (Id={Id})", year, existing.Id);
                anio = existing;
            }

            // Ensure 4 bimestres (Periodos) exist and are linked to this AnioLectivo
            var descriptions = new[] { "I Bimestre", "II Bimestre", "III Bimestre", "IV Bimestre" };
            for (int i = 0; i < descriptions.Length; i++)
            {
                var desc = $"{year} - {descriptions[i]}";
                _logger?.LogDebug("Ensuring periodo '{Desc}'", desc);
                var p = await _db.Periodos.FirstOrDefaultAsync(per => per.Descripcion == desc);
                if (p is null)
                {
                    _logger?.LogInformation("Creating periodo {Desc} for AnioLectivo {AnioId}", desc, anio.Id);
                    p = new Periodo { Descripcion = desc, Activo = true, AnioLectivoId = anio.Id, Orden = i + 1 };
                    _db.Periodos.Add(p);
                    await SaveChangesWithAuditAsync(ct);
                }
                else
                {
                    // If exists but linked to wrong AnioLectivo, fix it
                    if (p.AnioLectivoId != anio.Id)
                    {
                        _logger?.LogWarning("Periodo {Desc} exists but linked to AnioLectivo {Old} - reassigning to {New}", desc, p.AnioLectivoId, anio.Id);
                        p.AnioLectivoId = anio.Id;
                        p.Orden = i + 1;
                        _db.Periodos.Update(p);
                        await SaveChangesWithAuditAsync(ct);
                    }
                }
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }


    // ─────────────────────────────────────────────────────────────────────────────
    // 1) Roles
    // ─────────────────────────────────────────────────────────────────────────────
    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "Direccion", "Subdireccion", "JefeArea", "Docente", "Estudiante", "Tutor" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger?.LogInformation("Creating role {Role}", role);
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 2) Root Admin (aprobado) – solo si no existe ninguno
    //    Se leen credenciales desde variables de entorno si están presentes:
    //    ROOT_ADMIN_EMAIL, ROOT_ADMIN_PASSWORD
    // ─────────────────────────────────────────────────────────────────────────────
    private async Task SeedRootAdminAsync()
    {
        var anyAdmin = (await _userManager.GetUsersInRoleAsync("Admin")).Any();
        var envEmail = Environment.GetEnvironmentVariable("ROOT_ADMIN_EMAIL");
        var envPass = Environment.GetEnvironmentVariable("ROOT_ADMIN_PASSWORD");

        var email = string.IsNullOrWhiteSpace(envEmail) ? "root@siae.local" : envEmail!;
        var pass = string.IsNullOrWhiteSpace(envPass) ? "Change_this_123!" : envPass!;

        if (!anyAdmin)
        {
            _logger?.LogInformation("No admin found - creating root admin {Email}", email);
            var root = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Root Administrator",
                IsApproved = true // ← root entra aprobado
            };

            var create = await _userManager.CreateAsync(root, pass);
            if (!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"No se pudo crear el root admin: {errors}");
            }

            await _userManager.AddToRoleAsync(root, "Admin");
            _logger?.LogInformation("Root admin created: {Email}", email);
            // Use the created root user's Id for subsequent audit fields
            _seedUserId = root.Id;
            // Ensure audit shadow properties are set for the identity user row as well
            try
            {
                _db.Attach(root);
                var now = DateTime.UtcNow;
                var entry = _db.Entry(root);
                // Set shadow properties if present
                if (entry.Metadata.FindProperty("CreadoPor") != null) entry.Property("CreadoPor").CurrentValue = _seedUserId;
                if (entry.Metadata.FindProperty("FechaRegistro") != null) entry.Property("FechaRegistro").CurrentValue = now;
                if (entry.Metadata.FindProperty("ModificadoPor") != null) entry.Property("ModificadoPor").CurrentValue = _seedUserId;
                if (entry.Metadata.FindProperty("FechaModificacion") != null) entry.Property("FechaModificacion").CurrentValue = now;
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await SaveChangesWithAuditAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to persist audit fields for root admin user {Email}", email);
            }
            return;
        }

        // Ya hay admin(es). Asegura que al menos uno esté aprobado.
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var firstNotApproved = admins.FirstOrDefault(a => !a.IsApproved);
        if (firstNotApproved is not null)
        {
            _logger?.LogInformation("Approving existing admin user {UserId}", firstNotApproved.Id);
            firstNotApproved.IsApproved = true;
            // Persist the approval and ensure audit fields use the admin id
            // Temporarily set seed user to this admin so the approval change is auditable
            var previousSeed = _seedUserId;
            try
            {
                _seedUserId = firstNotApproved.Id;
                await SaveChangesWithAuditAsync();
            }
            finally
            {
                _seedUserId = previousSeed;
            }
            // Also use this approved admin id for subsequent seeding
            _seedUserId = firstNotApproved.Id;
        }
        else
        {
            // If there is no specifically approved admin, pick the first existing admin id for audit
            var chosen = admins.FirstOrDefault();
            if (chosen is not null)
                _seedUserId = chosen.Id;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 3) Catálogos según el ER
    // ─────────────────────────────────────────────────────────────────────────────
    private async Task SeedCatalogsAsync()
    {
        // NIVELES
        if (!await _db.Niveles.AnyAsync())
        {
            _logger?.LogInformation("Seeding niveles (Primaria, Secundaria)");
            var primaria = new Nivel { DescripcionNivel = "Primaria", DescripcionTurno = "Mañana" };
            var secundaria = new Nivel { DescripcionNivel = "Secundaria", DescripcionTurno = "Tarde" };
            _db.Niveles.AddRange(primaria, secundaria);
            await SaveChangesWithAuditAsync();
        }

        // GRADO_SECCION
        if (!await _db.GradoSecciones.AnyAsync())
        {
            _logger?.LogInformation("Seeding grado seccion demo");
            _db.GradoSecciones.AddRange(
                new GradoSeccion { DescripcionGrado = "1°", DescripcionSeccion = "A" },
                new GradoSeccion { DescripcionGrado = "1°", DescripcionSeccion = "B" },
                new GradoSeccion { DescripcionGrado = "2°", DescripcionSeccion = "A" },
                new GradoSeccion { DescripcionGrado = "7°", DescripcionSeccion = "A" }
            );
            await SaveChangesWithAuditAsync();
        }

        // NIVEL_DETALLE (Nivel + GradoSeccion)
        if (!await _db.NivelesDetalle.AnyAsync())
        {
            var primaria = await _db.Niveles.FirstAsync(n => n.DescripcionNivel == "Primaria");
            var secundaria = await _db.Niveles.FirstAsync(n => n.DescripcionNivel == "Secundaria");
            var g1A = await _db.GradoSecciones.FirstAsync(g => g.DescripcionGrado == "1°" && g.DescripcionSeccion == "A");
            var g1B = await _db.GradoSecciones.FirstAsync(g => g.DescripcionGrado == "1°" && g.DescripcionSeccion == "B");
            var g2A = await _db.GradoSecciones.FirstAsync(g => g.DescripcionGrado == "2°" && g.DescripcionSeccion == "A");
            var g7A = await _db.GradoSecciones.FirstAsync(g => g.DescripcionGrado == "7°" && g.DescripcionSeccion == "A");

            _db.NivelesDetalle.AddRange(
                new NivelDetalle { NivelId = primaria.Id, GradoSeccionId = g1A.Id, TotalVacantes = 40, VacantesOcupadas = 0 },
                new NivelDetalle { NivelId = primaria.Id, GradoSeccionId = g1B.Id, TotalVacantes = 40, VacantesOcupadas = 0 },
                new NivelDetalle { NivelId = primaria.Id, GradoSeccionId = g2A.Id, TotalVacantes = 40, VacantesOcupadas = 0 },
                new NivelDetalle { NivelId = secundaria.Id, GradoSeccionId = g7A.Id, TotalVacantes = 40, VacantesOcupadas = 0 }
            );
            await SaveChangesWithAuditAsync();
        }

        // CURSO
        if (!await _db.Cursos.AnyAsync())
        {
            _logger?.LogInformation("Seeding cursos demo");
            _db.Cursos.AddRange(
                new Curso { Descripcion = "Matemática", Codigo = "MAT-01" },
                new Curso { Descripcion = "Lengua y Literatura", Codigo = "LEN-01" },
                new Curso { Descripcion = "Ciencias Naturales", Codigo = "CIE-01" }
            );
            await SaveChangesWithAuditAsync();
        }

        // NIVEL_DETALLE_CURSO (asociar todos los cursos a todos los detalles)
        if (!await _db.NivelesDetalleCurso.AnyAsync())
        {
            var detalles = await _db.NivelesDetalle.AsNoTracking().ToListAsync();
            var cursos = await _db.Cursos.AsNoTracking().ToListAsync();

                    foreach (var nd in detalles)
                foreach (var c in cursos)
                    _db.NivelesDetalleCurso.Add(new NivelDetalleCurso
                    {
                        NivelDetalleId = nd.Id,
                        CursoId = c.Id,
                        Activo = true
                    });

            await SaveChangesWithAuditAsync();
        }

        // DOCENTE(s) demo (por estructura – no usuarios aún)
        if (!await _db.Docentes.AnyAsync())
        {
            // Personas completas (cédula, fecha, sexo, teléfono normalizado)
            var fnAna = UtcDate(1990, 7, 16);
            var fnLuis = UtcDate(1987, 3, 10);

            var cedAna = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fnAna, 1027, 'B');
            var cedLuis = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fnLuis, 2045, 'C');

            // Teléfonos válidos NI (8 dígitos, prefijo 2/5/7/8)
            TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("8888-0000", out var telAna);
            TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("2222-0000", out var telLuis);

            var p1 = new Persona
            {
                Nombres = "Ana",
                Apellidos = "Gómez",
                Email = "ana.gomez@demo.local",
                Ciudad = "Managua",
                DocumentoIdentidad = cedAna,
                FechaNacimiento = fnAna,
                Sexo = "F",
                NumeroTelefono = telAna
            };
            var p2 = new Persona
            {
                Nombres = "Luis",
                Apellidos = "Martínez",
                Email = "luis.martinez@demo.local",
                Ciudad = "León",
                DocumentoIdentidad = cedLuis,
                FechaNacimiento = fnLuis,
                Sexo = "M",
                NumeroTelefono = telLuis
            };

            _logger?.LogInformation("Seeding demo personas/docentes");
            _db.Personas.AddRange(p1, p2);
            await SaveChangesWithAuditAsync();

            _db.Docentes.AddRange(
                new Docente { PersonaId = p1.Id, GradoEstudio = "Licenciatura" },
                new Docente { PersonaId = p2.Id, GradoEstudio = "Maestría" }
            );
            await SaveChangesWithAuditAsync();
        }

        // DOCENTE_NIVELDETALLE_CURSO (asignación docente–curso–nivel)
        if (!await _db.DocentesNivelDetalleCurso.AnyAsync())
        {
            _logger?.LogInformation("Seeding docentes-nivel-detalle-curso assignments");
            var docente1 = await _db.Docentes.OrderBy(d => d.Id).FirstAsync();
            var docente2 = await _db.Docentes.OrderBy(d => d.Id).Skip(1).FirstAsync();
            var ndcList = await _db.NivelesDetalleCurso.AsNoTracking().ToListAsync();

            for (int i = 0; i < ndcList.Count; i++)
            {
                _db.DocentesNivelDetalleCurso.Add(new DocenteNivelDetalleCurso
                {
                    NivelDetalleCursoId = ndcList[i].Id,
                    DocenteId = (i % 2 == 0) ? docente1.Id : docente2.Id,
                    Activo = true
                });
            }
            await SaveChangesWithAuditAsync();
        }

        // CURRICULA (una por asignación)
        if (!await _db.Curriculas.AnyAsync())
        {
            _logger?.LogInformation("Seeding demo curriculas");
            var asignaciones = await _db.DocentesNivelDetalleCurso.AsNoTracking().ToListAsync();
            foreach (var a in asignaciones.Take(6))
            {
                _db.Curriculas.Add(new Curricula
                {
                    DocenteNivelDetalleCursoId = a.Id,
                    Descripcion = "Unidad 1: Introducción",
                    Activo = true
                });
            }
            await SaveChangesWithAuditAsync();
        }

        // PERIODO
        // periodos are seeded per AnioLectivo in SeedAcademicYearDemoAsync
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // 4) Personas + Usuarios + Reglas (docente de profesión, dirección/subdirección)
    // ─────────────────────────────────────────────────────────────────────────────

    // Crea/actualiza usuario con roles y marca aprobación
    private async Task<ApplicationUser> EnsureUserAsync(
        string email, string password, string fullName,
        int? personaId, bool approved, params string[] roles)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                PersonaId = personaId,
                IsApproved = approved
            };
            var res = await _userManager.CreateAsync(user, password);
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));

            // Persist audit shadow properties for the created identity user
            try
            {
                // use current seed user id when seeding (could be 'system' or previously chosen admin)
                var now = DateTime.UtcNow;
                _db.Attach(user);
                var entry = _db.Entry(user);
                if (entry.Metadata.FindProperty("CreadoPor") != null) entry.Property("CreadoPor").CurrentValue = _seedUserId;
                if (entry.Metadata.FindProperty("FechaRegistro") != null) entry.Property("FechaRegistro").CurrentValue = now;
                if (entry.Metadata.FindProperty("ModificadoPor") != null) entry.Property("ModificadoPor").CurrentValue = _seedUserId;
                if (entry.Metadata.FindProperty("FechaModificacion") != null) entry.Property("FechaModificacion").CurrentValue = now;
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await SaveChangesWithAuditAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to persist audit fields for user {Email}", email);
            }
        }

        var current = await _userManager.GetRolesAsync(user);
        var missing = roles.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();
        if (missing.Length > 0)
            await _userManager.AddToRolesAsync(user, missing);

        return user;
    }

    private async Task SeedPeopleAndUsersAsync(int year)
    {
        // 1) Docente de profesión con rol de sistema JefeArea (debe existir fila Docente)
        if (!await _userManager.Users.AnyAsync(u => u.Email == "ana.gomez@demo.local"))
        {
            var pAna = await _db.Personas.FirstOrDefaultAsync(p => p.Email == "ana.gomez@demo.local");
            if (pAna is null)
            {
                var fn = UtcDate(1990, 7, 16);
                var ced = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fn, 1027, 'B');
                TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("8888-0000", out var tel);
                pAna = new Persona { Nombres = "Ana", Apellidos = "Gómez", Email = "ana.gomez@demo.local", DocumentoIdentidad = ced, FechaNacimiento = fn, Sexo = "F", NumeroTelefono = tel, Ciudad = "Managua" };
                _db.Personas.Add(pAna); await SaveChangesWithAuditAsync();
            }

            if (!await _db.Docentes.AnyAsync(d => d.PersonaId == pAna.Id))
            {
                _db.Docentes.Add(new Docente { PersonaId = pAna.Id, GradoEstudio = "Licenciatura" });
                await SaveChangesWithAuditAsync();
            }

            await EnsureUserAsync("ana.gomez@demo.local", "Docente123!", "Ana Gómez", pAna.Id, approved: true, "JefeArea");
        }

        // 2) Docente de profesión con rol de sistema Direccion
        if (!await _userManager.Users.AnyAsync(u => u.Email == "luis.martinez@demo.local"))
        {
            var pLuis = await _db.Personas.FirstOrDefaultAsync(p => p.Email == "luis.martinez@demo.local");
            if (pLuis is null)
            {
                var fn = UtcDate(1987, 3, 10);
                var ced = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fn, 2045, 'C');
                TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("2222-0000", out var tel);
                pLuis = new Persona { Nombres = "Luis", Apellidos = "Martínez", Email = "luis.martinez@demo.local", DocumentoIdentidad = ced, FechaNacimiento = fn, Sexo = "M", NumeroTelefono = tel, Ciudad = "León" };
                _db.Personas.Add(pLuis); await SaveChangesWithAuditAsync();
            }

            if (!await _db.Docentes.AnyAsync(d => d.PersonaId == pLuis.Id))
            {
                _db.Docentes.Add(new Docente { PersonaId = pLuis.Id, GradoEstudio = "Maestría" });
                await SaveChangesWithAuditAsync();
            }

            await EnsureUserAsync("luis.martinez@demo.local", "Docente123!", "Luis Martínez", pLuis.Id, approved: true, "Direccion");
        }

        // 3) Dirección que NO es docente de profesión (no crea fila Docente)
        if (!await _userManager.Users.AnyAsync(u => u.Email == "carlos.director@demo.local"))
        {
            var pDir = await _db.Personas.FirstOrDefaultAsync(p => p.Email == "carlos.director@demo.local");
            if (pDir is null)
            {
                var fn = UtcDate(1980, 1, 20);
                var ced = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fn, 3001, 'D');
                TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("5789-1234", out var tel);
                pDir = new Persona { Nombres = "Carlos", Apellidos = "Director", Email = "carlos.director@demo.local", DocumentoIdentidad = ced, FechaNacimiento = fn, Sexo = "M", NumeroTelefono = tel, Ciudad = "Managua" };
                _db.Personas.Add(pDir); await SaveChangesWithAuditAsync();
            }

            await EnsureUserAsync("carlos.director@demo.local", "Direccion123!", "Carlos Director", pDir.Id, approved: true, "Direccion");
        }

        // 4) Alumno + Tutor (Apoderado) con matrícula
        if (!await _db.Alumnos.AnyAsync())
        {
            // Tutor (adulto con cédula NI válida)
            var fnTutor = UtcDate(1985, 5, 30);
            var cedTutor = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fnTutor, 4500, 'E');
            TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("8888-1111", out var telTutor);

            var pTutor = new Persona
            {
                Nombres = "María",
                Apellidos = "Juárez",
                Email = "maria.tutora@demo.local",
                DocumentoIdentidad = cedTutor,
                FechaNacimiento = fnTutor,
                Sexo = "F",
                NumeroTelefono = telTutor,
                Ciudad = "Masaya"
            };
            _db.Personas.Add(pTutor); await SaveChangesWithAuditAsync();
            _logger?.LogInformation("Created tutor persona {Email}", pTutor.Email);

            var tutor = new Apoderado { PersonaId = pTutor.Id, TipoParentesco = "Madre", Activo = true };
            _db.Apoderados.Add(tutor); await SaveChangesWithAuditAsync();
            _logger?.LogInformation("Created apoderado (tutor) Id={ApId} for persona {PersonaId}", tutor.Id, pTutor.Id);

            await EnsureUserAsync("maria.tutora@demo.local", "Tutor123!", "María Juárez", pTutor.Id, approved: true, "Tutor");

            // Alumno menor → DocumentoIdentidad = "TUTOR-<cedTutor>"
            var fnAlumno = AsUtcDate(DateTime.UtcNow.AddYears(-12)); // 12 años
            TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("7777-2222", out var telAlumno);

            var pAlumno = new Persona
            {
                Nombres = "Diego",
                Apellidos = "Juárez",
                Email = "diego.alumno@demo.local",
                DocumentoIdentidad = $"TUTOR-{cedTutor}", // ← regla de menor de edad
                FechaNacimiento = fnAlumno,
                Sexo = "M",
                NumeroTelefono = telAlumno,
                Ciudad = "Masaya"
            };
            _db.Personas.Add(pAlumno); await SaveChangesWithAuditAsync();
            _logger?.LogInformation("Created alumno persona {Email}", pAlumno.Email);

            var alumno = new Alumno { PersonaId = pAlumno.Id, Activo = true };
            _db.Alumnos.Add(alumno); await SaveChangesWithAuditAsync();
            _logger?.LogInformation("Created alumno Id={AlumnoId} for persona {PersonaId}", alumno.Id, pAlumno.Id);

            await EnsureUserAsync("diego.alumno@demo.local", "Alumno123!", "Diego Juárez", pAlumno.Id, approved: true, "Estudiante");
            _logger?.LogInformation("Created user for alumno {Email}", "diego.alumno@demo.local");

            // Assign matricula explicitly to AnioLectivo for the demo year and to Primaria 1° A
            var anio = await _db.AniosLectivos.AsNoTracking().FirstOrDefaultAsync(a => a.Anio == year);
            if (anio is null) throw new InvalidOperationException($"AnioLectivo {year} debe existir antes de crear matrículas demo");

            // Prefer Primaria > 1° A
            var nd = await _db.NivelesDetalle.AsNoTracking()
                .Include(n => n.Nivel)
                .Include(n => n.GradoSeccion)
                .FirstOrDefaultAsync(n => n.Nivel.DescripcionNivel == "Primaria" && n.GradoSeccion.DescripcionGrado.StartsWith("1°"));
            if (nd is null)
            {
                // fallback al primer NivelDetalle disponible
                nd = await _db.NivelesDetalle.AsNoTracking().FirstAsync();
            }

            _db.Matriculas.Add(new Matricula
            {
                AlumnoId = alumno.Id,
                NivelDetalleId = nd.Id,
                AnioLectivoId = anio.Id,
                ApoderadoId = tutor.Id,
                Activo = true
            });
            await SaveChangesWithAuditAsync();
        }
    }
}
