using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class PeriodoConfig : IEntityTypeConfiguration<Periodo>
{
    public void Configure(EntityTypeBuilder<Periodo> b)
    {
        b.ToTable("periodo");
        b.Property(x => x.Id).HasColumnName("periodo_id");
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.Property(x => x.AnioLectivoId).HasColumnName("anio_lectivo_id");
        b.HasOne(x => x.AnioLectivo).WithMany(a => a.Periodos).HasForeignKey(x => x.AnioLectivoId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.Orden).HasColumnName("orden");
        b.HasIndex(x => new { x.AnioLectivoId, x.Orden }).HasDatabaseName("ix_periodo_anio_orden");
    }
}
