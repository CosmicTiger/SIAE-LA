using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class GradoSeccionConfig : IEntityTypeConfiguration<GradoSeccion>
{
    public void Configure(EntityTypeBuilder<GradoSeccion> b)
    {
        b.ToTable("grado_seccion");
        b.Property(x => x.Id).HasColumnName("grado_seccion_id");
        b.Property(x => x.DescripcionGrado).HasColumnName("descripcion_grado").HasMaxLength(50).IsRequired();
        b.Property(x => x.DescripcionSeccion).HasColumnName("descripcion_seccion").HasMaxLength(50).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
    }
}
