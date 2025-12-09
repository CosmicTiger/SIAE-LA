using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class ApoderadoConfig : IEntityTypeConfiguration<Apoderado>
{
    public void Configure(EntityTypeBuilder<Apoderado> b)
    {
        b.ToTable("apoderado");
        b.Property(x => x.Id).HasColumnName("apoderado_id");
        b.Property(x => x.PersonaId).HasColumnName("persona_id");
        b.Property(x => x.EstadoCivil).HasColumnName("estado_civil");
        b.Property(x => x.TipoParentesco).HasColumnName("tipo_parentesco");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
    }
}
