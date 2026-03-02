using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class CalificacionConfig : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> b)
    {
        b.ToTable("calificacion");
        b.Property(x => x.Id).HasColumnName("calificacion_id");
        b.Property(x => x.CurriculaId).HasColumnName("curricula_id");
        b.Property(x => x.AlumnoId).HasColumnName("alumno_id");
        b.Property(x => x.Nota).HasColumnName("nota").HasPrecision(5, 2);
        b.Property(x => x.PeriodoId).HasColumnName("periodo_id");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");

        // CHECK en snake_case (válido para PostgreSQL y también para SQL Server)
        b.ToTable(t => t.HasCheckConstraint("ck_calificacion_nota", "nota >= 0 AND nota <= 100"));

        b.HasIndex(x => new { x.CurriculaId, x.AlumnoId })
         .IsUnique()
         .HasDatabaseName("ux_calificacion_curricula_alumno");

        b.HasOne(x => x.Curricula)
         .WithMany(c => c.Calificaciones)
         .HasForeignKey(x => x.CurriculaId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Alumno)
         .WithMany(a => a.Calificaciones)
         .HasForeignKey(x => x.AlumnoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Periodo).WithMany().HasForeignKey(x => x.PeriodoId).OnDelete(DeleteBehavior.Restrict);
    }
}
