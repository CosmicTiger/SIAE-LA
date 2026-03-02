using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SIAE_LA.Domain.Entities;
using System.Security.Claims;

namespace SIAE_LA.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Persona> Personas { get; set; } = default!;
    public DbSet<Alumno> Alumnos { get; set; } = default!;
    public DbSet<Docente> Docentes { get; set; } = default!;
    public DbSet<Apoderado> Apoderados { get; set; } = default!;
    public DbSet<AlumnoApoderado> AlumnosApoderados { get; set; } = default!;
    public DbSet<GradoSeccion> GradoSecciones { get; set; } = default!;
    public DbSet<NivelDetalle> NivelesDetalle { get; set; } = default!;
    public DbSet<Curso> Cursos { get; set; } = default!;
    public DbSet<NivelDetalleCurso> NivelesDetalleCurso { get; set; } = default!;
    public DbSet<DocenteNivelDetalleCurso> DocentesNivelDetalleCurso { get; set; } = default!;
    public DbSet<Curricula> Curriculas { get; set; } = default!;
    public DbSet<Horario> Horarios { get; set; } = default!;
    public DbSet<Matricula> Matriculas { get; set; } = default!;
    public DbSet<Calificacion> Calificaciones { get; set; } = default!;
    public DbSet<AnioLectivo> AniosLectivos { get; set; } = default!;
    public DbSet<Nivel> Niveles { get; set; } = default!;
    public DbSet<Periodo> Periodos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ---- CHECKs dependientes del provider (PERSONA) ----
        if (Database.IsNpgsql())
        {
            modelBuilder.Entity<Persona>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_PERSONA_DOCID_FORMAT",
                    @"(
                        documento_identidad ~ '^\d{3}-\d{6}-\d{3,4}[A-Z]?$'
                        OR documento_identidad ~ '^TUTOR-\d{3}-\d{6}-\d{3,4}[A-Z]?$'
                      )"));

            modelBuilder.Entity<Persona>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_PERSONA_SEXO",
                    "sexo IN ('M','F','O')"));

            modelBuilder.Entity<Persona>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_PERSONA_TEL_NI",
                    @"(
                        numero_telefono IS NULL
                        OR numero_telefono ~ '^\+505\d{8}$'
                        OR numero_telefono ~ '^\d{8}$'
                      )"));
        }
        else if (Database.IsSqlServer())
        {
            modelBuilder.Entity<Persona>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_PERSONA_DOCID_FORMAT",
                    @"(
                        documento_identidad LIKE '[0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][A-Z]'
                        OR documento_identidad LIKE 'TUTOR-[0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][A-Z]'
                      )"));

            modelBuilder.Entity<Persona>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_PERSONA_SEXO",
                    "sexo IN ('M','F','O')"));

            modelBuilder.Entity<Persona>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_PERSONA_TEL_NI",
                    @"(
                        numero_telefono IS NULL
                        OR numero_telefono LIKE '+505________'
                        OR numero_telefono LIKE '________'
                      )"));
        }

        // -------------- GLOBAL SNAKE_CASE CONVENTION ----------------
        static string ToSnakeCase(string? name)
        {
            if (string.IsNullOrEmpty(name)) return name ?? string.Empty;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0) sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        // Apply snake_case to tables, columns, keys, indexes, foreign keys
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // table
            var currentTableName = entityType.GetTableName();
            var currentSchema = entityType.GetSchema();

            if (!string.IsNullOrEmpty(currentTableName))
            {
                entityType.SetTableName(ToSnakeCase(currentTableName));
                // update local var to new table name used by EF after SetTableName
                currentTableName = entityType.GetTableName();
            }

            // properties -> columns (only if we have a table name)
            if (!string.IsNullOrEmpty(currentTableName))
            {
                var storeIdentifier = StoreObjectIdentifier.Table(currentTableName!, currentSchema);
                foreach (var property in entityType.GetProperties())
                {
                    // property.GetColumnName can return null; skip if null
                    var columnName = property.GetColumnName(storeIdentifier);
                    if (!string.IsNullOrEmpty(columnName))
                    {
                        property.SetColumnName(ToSnakeCase(columnName));
                    }
                    else
                    {
                        // fallback: use property CLR name
                        property.SetColumnName(ToSnakeCase(property.Name));
                    }
                }
            }
            else
            {
                // fallback: set column names using CLR names when no table name
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.Name));
                }
            }

            // keys, FKs, indexes names (guarded when name is present)
            foreach (var key in entityType.GetKeys())
            {
                var name = key.GetName();
                if (!string.IsNullOrEmpty(name)) key.SetName(ToSnakeCase(name));
            }

            foreach (var fk in entityType.GetForeignKeys())
            {
                var name = fk.GetConstraintName();
                if (!string.IsNullOrEmpty(name)) fk.SetConstraintName(ToSnakeCase(name));
            }

            foreach (var index in entityType.GetIndexes())
            {
                var name = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(name)) index.SetDatabaseName(ToSnakeCase(name));
            }
        }

        // -------------- ADD SHADOW AUDIT PROPERTIES FOR ALL ENTITIES --------------
        foreach (var et in modelBuilder.Model.GetEntityTypes())
        {
            // Skip EF internal/owned types
            if (et.IsOwned()) continue;
            var clrType = et.ClrType;
            if (clrType == null) continue;

            // Configure FechaRegistro for the entity:
            // - If the CLR type already defines a FechaRegistro property, map it using its CLR type
            // - Otherwise add a nullable shadow property FechaRegistro (DateTime?)
            var propInfo = clrType.GetProperty("FechaRegistro");
            // Choose default SQL for FechaRegistro depending on provider
            string defaultFechaRegistroSql;
            if (Database.IsNpgsql()) defaultFechaRegistroSql = "now()";
            else if (Database.IsSqlServer()) defaultFechaRegistroSql = "GETUTCDATE()";
            else defaultFechaRegistroSql = "CURRENT_TIMESTAMP";

            if (propInfo != null)
            {
                var propType = propInfo.PropertyType;
                // Map the CLR property, mark as required and set a DB default value
                modelBuilder.Entity(clrType)
                    .Property(propType, "FechaRegistro")
                    .HasColumnName("fecha_registro")
                    .IsRequired()
                    .HasDefaultValueSql(defaultFechaRegistroSql);
            }
            else
            {
                // Add a non-nullable shadow property FechaRegistro (DateTime) for entities that don't define it
                modelBuilder.Entity(clrType)
                    .Property<DateTime>("FechaRegistro")
                    .HasColumnName("fecha_registro")
                    .IsRequired()
                    .HasDefaultValueSql(defaultFechaRegistroSql);
            }
            // Add shadow property creado_por (string up to Identity length)
            modelBuilder.Entity(clrType).Property<string?>("CreadoPor").HasMaxLength(450).HasColumnName("creado_por");
            // Add shadow property modificado_por
            modelBuilder.Entity(clrType).Property<string?>("ModificadoPor").HasMaxLength(450).HasColumnName("modificado_por");
            // Add shadow property fecha_modificacion
            modelBuilder.Entity(clrType).Property<DateTime?>("FechaModificacion").HasColumnName("fecha_modificacion");
        }

        // Map Identity user columns to snake_case consistently (non-destructive to current mapping)
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("usuarios");
            b.Property(u => u.UserName).HasColumnName("username");
            b.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name");
            b.Property(u => u.Email).HasColumnName("email");
            b.Property(u => u.NormalizedEmail).HasColumnName("normalized_email");
            b.Property(u => u.PasswordHash).HasColumnName("password_hash");
            b.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
            b.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            b.Property(u => u.PhoneNumber).HasColumnName("telefono");
            b.Property(u => u.PhoneNumberConfirmed).HasColumnName("es_telefono_confirmado");
            b.Property(u => u.EmailConfirmed).HasColumnName("es_email_confirmado");
            b.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
            b.Property(u => u.LockoutEnabled).HasColumnName("lockout_enabled");
            b.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            b.Property(u => u.AccessFailedCount).HasColumnName("access_failed_count");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        string? userId = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            // Set creado_por only on Added
            if (entry.State == EntityState.Added)
            {
                var creadoPropMeta = entry.Metadata.FindProperty("CreadoPor");
                if (creadoPropMeta != null)
                    entry.Property(creadoPropMeta.Name).CurrentValue = userId;

                // If entity has FechaRegistro property, ensure it's set (safe check to avoid boxing null)
                var fechaRegistroMeta = entry.Metadata.FindProperty("FechaRegistro");
                if (fechaRegistroMeta != null)
                {
                    var current = entry.Property(fechaRegistroMeta.Name).CurrentValue;
                    if (current == null || (current is DateTime dt && dt == default))
                        entry.Property(fechaRegistroMeta.Name).CurrentValue = now;
                }
            }

            // Always set modified fields for Added & Modified (so new rows will have modification stamps too)
            var modificadoPropMeta = entry.Metadata.FindProperty("ModificadoPor");
            if (modificadoPropMeta != null)
                entry.Property(modificadoPropMeta.Name).CurrentValue = userId;

            var fechaModMeta = entry.Metadata.FindProperty("FechaModificacion");
            if (fechaModMeta != null)
                entry.Property(fechaModMeta.Name).CurrentValue = now;

            // Ensure any DateTime properties are UTC. Npgsql requires DateTimeKind.Utc for timestamptz.
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

        return base.SaveChangesAsync(cancellationToken);
    }
}
