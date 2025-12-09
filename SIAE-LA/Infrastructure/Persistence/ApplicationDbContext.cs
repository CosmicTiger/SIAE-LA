using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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
    public DbSet<Nivel> Niveles { get; set; } = default!;
    public DbSet<Periodo> Periodos { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // ---- CHECKs dependientes del provider (PERSONA) ----
        if (Database.IsNpgsql())
        {
            // DocumentoIdentidad:
            //  - cédula: ddd-dddddd-ddd(d)[A-Z]?   (tu ejemplo TUTOR-001-050585-450 encaja con d{3}-d{6}-d{3,4})
            //  - tutor:  TUTOR-ddd-dddddd-ddd(d)[A-Z]?
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
            // T-SQL con clases de caracteres
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
    }
}
