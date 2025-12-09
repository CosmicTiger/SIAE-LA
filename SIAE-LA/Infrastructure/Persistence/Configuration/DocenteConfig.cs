using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class DocenteConfig : IEntityTypeConfiguration<Docente>
{
    public void Configure(EntityTypeBuilder<Docente> b)
    {
        b.ToTable("docente");
        
        b.Property(x => x.Id).HasColumnName("docente_id");
        b.Property(x => x.PersonaId).HasColumnName("persona_id");
        b.Property(x => x.GradoEstudio).HasColumnName("grado_estudio").HasMaxLength(100);
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
    }
}
