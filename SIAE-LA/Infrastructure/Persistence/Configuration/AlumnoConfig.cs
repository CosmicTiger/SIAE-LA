using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class AlumnoConfig : IEntityTypeConfiguration<Alumno>
{
    public void Configure(EntityTypeBuilder<Alumno> b)
    {
        b.ToTable("alumno");
        b.Property(x => x.Id).HasColumnName("alumno_id");
        b.Property(x => x.PersonaId).HasColumnName("persona_id");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
    }
}
