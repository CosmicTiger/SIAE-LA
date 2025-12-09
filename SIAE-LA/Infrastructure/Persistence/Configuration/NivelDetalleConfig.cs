using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class NivelDetalleConfig : IEntityTypeConfiguration<NivelDetalle>
{
    public void Configure(EntityTypeBuilder<NivelDetalle> b)
    {
        b.ToTable("nivel_detalle");
        b.Property(x => x.Id).HasColumnName("nivel_detalle_id");
        b.Property(x => x.NivelId).HasColumnName("nivel_id");
        b.Property(x => x.GradoSeccionId).HasColumnName("grado_seccion_id");
        b.Property(x => x.TotalVacantes).HasColumnName("total_vacantes");
        b.Property(x => x.VacantesOcupadas).HasColumnName("vacantes_ocupadas");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro").IsRequired();

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
