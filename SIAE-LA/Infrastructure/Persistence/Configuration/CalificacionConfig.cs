using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class CalificacionConfig : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> b)
    {
        b.Property(x => x.Nota).HasPrecision(5, 2);
        b.ToTable(t => t.HasCheckConstraint("CK_Calificacion_Nota", "[Nota] >= 0 AND [Nota] <= 100"));

        b.HasOne(x => x.Estudiante)
         .WithMany(e => e.Calificaciones)
         .HasForeignKey(x => x.EstudianteId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.DocenteAsignaturaGrupo)
         .WithMany(dag => dag.Calificaciones)
         .HasForeignKey(x => x.DocenteAsignaturaGrupoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
