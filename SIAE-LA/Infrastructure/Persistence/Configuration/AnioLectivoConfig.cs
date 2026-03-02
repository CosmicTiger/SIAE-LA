using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configuration;

public class AnioLectivoConfig : IEntityTypeConfiguration<AnioLectivo>
{
    public void Configure(EntityTypeBuilder<AnioLectivo> b)
    {
        b.ToTable("anio_lectivo");
        b.Property(x => x.Id).HasColumnName("anio_lectivo_id");
        b.Property(x => x.Anio).HasColumnName("anio").IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(100);
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaInicio).HasColumnName("fecha_inicio");
        b.Property(x => x.FechaFin).HasColumnName("fecha_fin");

        b.HasMany(x => x.Periodos).WithOne(p => p.AnioLectivo).HasForeignKey(p => p.AnioLectivoId).OnDelete(DeleteBehavior.Restrict);
    }
}
