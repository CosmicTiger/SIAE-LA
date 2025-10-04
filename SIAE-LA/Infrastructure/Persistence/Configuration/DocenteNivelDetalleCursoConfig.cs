using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class DocenteNivelDetalleCursoConfig : IEntityTypeConfiguration<DocenteNivelDetalleCurso>
{
    public void Configure(EntityTypeBuilder<DocenteNivelDetalleCurso> b)
    {
        b.ToTable("DOCENTE_NIVELDETALLE_CURSO");
        b.HasIndex(x => new { x.NivelDetalleCursoId, x.DocenteId }).IsUnique();

        b.HasOne(x => x.NivelDetalleCurso)
         .WithMany(ndc => ndc.Docentes)
         .HasForeignKey(x => x.NivelDetalleCursoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Docente)
         .WithMany(d => d.Asignaciones)
         .HasForeignKey(x => x.DocenteId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
