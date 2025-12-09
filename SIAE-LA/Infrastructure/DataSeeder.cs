using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Utils;

namespace SIAE_LA.Infrastructure;

public class DataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DataSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        // 1) Migraciones
        await _db.Database.MigrateAsync();

        // 2) Roles + root admin
        await SeedRolesAsync();
        await SeedRootAdminAsync();

        // 3) Catálogos del ER
        await SeedCatalogsAsync();

        // 4) Personas / Usuarios / Vínculos (docentes, dirección, alumnos + tutores)
        await SeedPeopleAndUsersAsync();
    }

    private static DateTime UtcDate(int y, int m, int d)
    => new DateTime(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime AsUtcDate(DateTime dt)
        => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);


    // ─────────────────────────────────────────────────────────────────────────────
    // 1) Roles
    // ─────────────────────────────────────────────────────────────────────────────
    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "Direccion", "Subdireccion", "JefeArea", "Docente", "Estudiante", "Tutor" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
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
            return;
        }

        // Ya hay admin(es). Asegura que al menos uno esté aprobado.
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var firstNotApproved = admins.FirstOrDefault(a => !a.IsApproved);
        if (firstNotApproved is not null)
        {
            firstNotApproved.IsApproved = true;
            await _db.SaveChangesAsync();
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
            var primaria = new Nivel { DescripcionNivel = "Primaria", DescripcionTurno = "Mañana" };
            var secundaria = new Nivel { DescripcionNivel = "Secundaria", DescripcionTurno = "Tarde" };
            _db.Niveles.AddRange(primaria, secundaria);
            await _db.SaveChangesAsync();
        }

        // GRADO_SECCION
        if (!await _db.GradoSecciones.AnyAsync())
        {
            _db.GradoSecciones.AddRange(
                new GradoSeccion { DescripcionGrado = "1°", DescripcionSeccion = "A" },
                new GradoSeccion { DescripcionGrado = "1°", DescripcionSeccion = "B" },
                new GradoSeccion { DescripcionGrado = "2°", DescripcionSeccion = "A" },
                new GradoSeccion { DescripcionGrado = "7°", DescripcionSeccion = "A" }
            );
            await _db.SaveChangesAsync();
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
            await _db.SaveChangesAsync();
        }

        // CURSO
        if (!await _db.Cursos.AnyAsync())
        {
            _db.Cursos.AddRange(
                new Curso { Descripcion = "Matemática", Codigo = "MAT-01" },
                new Curso { Descripcion = "Lengua y Literatura", Codigo = "LEN-01" },
                new Curso { Descripcion = "Ciencias Naturales", Codigo = "CIE-01" }
            );
            await _db.SaveChangesAsync();
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

            await _db.SaveChangesAsync();
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

            _db.Personas.AddRange(p1, p2);
            await _db.SaveChangesAsync();

            _db.Docentes.AddRange(
                new Docente { PersonaId = p1.Id, GradoEstudio = "Licenciatura" },
                new Docente { PersonaId = p2.Id, GradoEstudio = "Maestría" }
            );
            await _db.SaveChangesAsync();
        }

        // DOCENTE_NIVELDETALLE_CURSO (asignación docente–curso–nivel)
        if (!await _db.DocentesNivelDetalleCurso.AnyAsync())
        {
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
            await _db.SaveChangesAsync();
        }

        // CURRICULA (una por asignación)
        if (!await _db.Curriculas.AnyAsync())
        {
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
            await _db.SaveChangesAsync();
        }

        // PERIODO
        if (!await _db.Periodos.AnyAsync())
        {
            var y = DateTime.UtcNow.Year;
            _db.Periodos.AddRange(
                new Periodo { Descripcion = $"{y} - I Corte", Activo = true },
                new Periodo { Descripcion = $"{y} - II Corte", Activo = true },
                new Periodo { Descripcion = $"{y} - III Corte", Activo = true }
            );
            await _db.SaveChangesAsync();
        }
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
        }

        var current = await _userManager.GetRolesAsync(user);
        var missing = roles.Except(current, StringComparer.OrdinalIgnoreCase).ToArray();
        if (missing.Length > 0)
            await _userManager.AddToRolesAsync(user, missing);

        return user;
    }

    private async Task SeedPeopleAndUsersAsync()
    {
        // 1) Docente de profesión con rol de sistema JefeArea (debe existir fila Docente)
        if (!await _userManager.Users.AnyAsync(u => u.Email == "ana.gomez@demo.local"))
        {
            var pAna = await _db.Personas.FirstOrDefaultAsync(p => p.Email == "ana.gomez@demo.local");
            if (pAna is null)
            {
                var fn = new DateTime(1990, 7, 16);
                var ced = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fn, 1027, 'B');
                TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("8888-0000", out var tel);
                pAna = new Persona { Nombres = "Ana", Apellidos = "Gómez", Email = "ana.gomez@demo.local", DocumentoIdentidad = ced, FechaNacimiento = fn, Sexo = "F", NumeroTelefono = tel, Ciudad = "Managua" };
                _db.Personas.Add(pAna); await _db.SaveChangesAsync();
            }

            if (!await _db.Docentes.AnyAsync(d => d.PersonaId == pAna.Id))
            {
                _db.Docentes.Add(new Docente { PersonaId = pAna.Id, GradoEstudio = "Licenciatura" });
                await _db.SaveChangesAsync();
            }

            await EnsureUserAsync("ana.gomez@demo.local", "Docente123!", "Ana Gómez", pAna.Id, approved: true, "JefeArea");
        }

        // 2) Docente de profesión con rol de sistema Direccion
        if (!await _userManager.Users.AnyAsync(u => u.Email == "luis.martinez@demo.local"))
        {
            var pLuis = await _db.Personas.FirstOrDefaultAsync(p => p.Email == "luis.martinez@demo.local");
            if (pLuis is null)
            {
                var fn = new DateTime(1987, 3, 10);
                var ced = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fn, 2045, 'C');
                TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("2222-0000", out var tel);
                pLuis = new Persona { Nombres = "Luis", Apellidos = "Martínez", Email = "luis.martinez@demo.local", DocumentoIdentidad = ced, FechaNacimiento = fn, Sexo = "M", NumeroTelefono = tel, Ciudad = "León" };
                _db.Personas.Add(pLuis); await _db.SaveChangesAsync();
            }

            if (!await _db.Docentes.AnyAsync(d => d.PersonaId == pLuis.Id))
            {
                _db.Docentes.Add(new Docente { PersonaId = pLuis.Id, GradoEstudio = "Maestría" });
                await _db.SaveChangesAsync();
            }

            await EnsureUserAsync("luis.martinez@demo.local", "Docente123!", "Luis Martínez", pLuis.Id, approved: true, "Direccion");
        }

        // 3) Dirección que NO es docente de profesión (no crea fila Docente)
        if (!await _userManager.Users.AnyAsync(u => u.Email == "carlos.director@demo.local"))
        {
            var pDir = await _db.Personas.FirstOrDefaultAsync(p => p.Email == "carlos.director@demo.local");
            if (pDir is null)
            {
                var fn = new DateTime(1980, 1, 20);
                var ced = CedulaNicaraguenseValidadorHelper.BuildCedula("001", fn, 3001, 'D');
                TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi("5789-1234", out var tel);
                pDir = new Persona { Nombres = "Carlos", Apellidos = "Director", Email = "carlos.director@demo.local", DocumentoIdentidad = ced, FechaNacimiento = fn, Sexo = "M", NumeroTelefono = tel, Ciudad = "Managua" };
                _db.Personas.Add(pDir); await _db.SaveChangesAsync();
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
            _db.Personas.Add(pTutor); await _db.SaveChangesAsync();

            var tutor = new Apoderado { PersonaId = pTutor.Id, TipoParentesco = "Madre", Activo = true };
            _db.Apoderados.Add(tutor); await _db.SaveChangesAsync();

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
            _db.Personas.Add(pAlumno); await _db.SaveChangesAsync();

            var alumno = new Alumno { PersonaId = pAlumno.Id, Activo = true };
            _db.Alumnos.Add(alumno); await _db.SaveChangesAsync();

            await EnsureUserAsync("diego.alumno@demo.local", "Alumno123!", "Diego Juárez", pAlumno.Id, approved: true, "Estudiante");

            var nd = await _db.NivelesDetalle.AsNoTracking().FirstAsync();
            var per = await _db.Periodos.AsNoTracking().FirstAsync();

            _db.Matriculas.Add(new Matricula
            {
                AlumnoId = alumno.Id,
                NivelDetalleId = nd.Id,
                PeriodoId = per.Id,
                ApoderadoId = tutor.Id,
                Activo = true
            });
            await _db.SaveChangesAsync();
        }
    }
}
