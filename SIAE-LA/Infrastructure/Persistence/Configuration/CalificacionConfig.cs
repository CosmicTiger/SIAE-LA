using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class CalificacionConfig : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> b)
    {
        b.ToTable("CALIFICACION");
        b.Property(x => x.Nota).HasPrecision(5, 2);
        b.ToTable(t => t.HasCheckConstraint("CK_Calificacion_Nota", "[Nota]>=0 AND [Nota]<=100"));

        b.HasIndex(x => new { x.CurriculaId, x.AlumnoId }).IsUnique();

        b.HasOne(x => x.Curricula)
         .WithMany(c => c.Calificaciones)
         .HasForeignKey(x => x.CurriculaId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Alumno)
         .WithMany(a => a.Calificaciones)
         .HasForeignKey(x => x.AlumnoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
