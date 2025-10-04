using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class NivelDetalleConfig : IEntityTypeConfiguration<NivelDetalle>
{
    public void Configure(EntityTypeBuilder<NivelDetalle> b)
    {
        b.ToTable("NIVEL_DETALLE");
        b.HasIndex(x => new { x.NivelId, x.GradoSeccionId }).IsUnique();

        b.HasOne(x => x.Nivel)
         .WithMany(n => n.Detalles)
         .HasForeignKey(x => x.NivelId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.GradoSeccion)
         .WithMany(gs => gs.NivelDetalles)
         .HasForeignKey(x => x.GradoSeccionId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
