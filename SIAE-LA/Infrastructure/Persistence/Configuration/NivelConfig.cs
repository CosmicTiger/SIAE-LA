using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class NivelConfig : IEntityTypeConfiguration<Nivel>
{
    public void Configure(EntityTypeBuilder<Nivel> b)
    {
        b.ToTable("nivel");
        b.Property(x => x.Id).HasColumnName("nivel_id");
        b.Property(x => x.DescripcionNivel).HasColumnName("descripcion_nivel").IsRequired().HasMaxLength(100);
        b.Property(x => x.DescripcionTurno).HasColumnName("descripcion_turno").HasMaxLength(100);
        b.Property(x => x.Horario).HasColumnName("horario").HasMaxLength(50);
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro").IsRequired();

    }
}
