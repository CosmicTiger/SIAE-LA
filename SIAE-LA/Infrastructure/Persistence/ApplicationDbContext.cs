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
    }
}
