using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Estudiante> Estudiantes { get; set; } = default!;
    public DbSet<Docente> Docentes { get; set; } = default!;
    public DbSet<Asignatura> Asignaturas { get; set; } = default!;
    public DbSet<Nivel> Niveles { get; set; } = default!;
    public DbSet<Grupo> Grupos { get; set; } = default!;
    public DbSet<Periodo> Periodos { get; set; } = default!;
    public DbSet<Matricula> Matriculas { get; set; } = default!;
    public DbSet<DocenteAsignaturaGrupo> DocenteAsignaturaGrupos { get; set; } = default!;
    public DbSet<Calificacion> Calificaciones { get; set; } = default!;
    public DbSet<EventoCalendario> EventosCalendario { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
