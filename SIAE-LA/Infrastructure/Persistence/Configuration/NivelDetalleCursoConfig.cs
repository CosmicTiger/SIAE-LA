using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class NivelDetalleCursoConfig : IEntityTypeConfiguration<NivelDetalleCurso>
{
    public void Configure(EntityTypeBuilder<NivelDetalleCurso> b)
    {
        b.ToTable("NIVEL_DETALLE_CURSO");
        b.HasIndex(x => new { x.NivelDetalleId, x.CursoId }).IsUnique();

        b.HasOne(x => x.NivelDetalle)
         .WithMany(nd => nd.Cursos)
         .HasForeignKey(x => x.NivelDetalleId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Curso)
         .WithMany(c => c.Niveles)
         .HasForeignKey(x => x.CursoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
