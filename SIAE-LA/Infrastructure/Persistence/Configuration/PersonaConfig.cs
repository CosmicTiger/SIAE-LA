using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configuration;

public class PersonaConfig : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> b)
    {
        b.ToTable("PERSONA");
        b.HasKey(x => x.Id);

        b.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
        b.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();

        // NUEVO: requeridos
        b.Property(x => x.FechaNacimiento).IsRequired();
        b.Property(x => x.Sexo).HasMaxLength(1).IsRequired();   // 'M' o 'F'

        // DocumentoIdentidad requerido + único
        b.Property(x => x.DocumentoIdentidad).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.DocumentoIdentidad).IsUnique();

        // CHECK: formato cédula nica o TUTOR-<cédula>
        b.ToTable(t => t.HasCheckConstraint(
            "CK_PERSONA_DOCID_FORMAT",
            "(DocumentoIdentidad LIKE '[0-9][0-9][0-9]-[0-3][0-9][0-1][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][A-Z]' " +
            " OR DocumentoIdentidad LIKE 'TUTOR-[0-9][0-9][0-9]-[0-3][0-9][0-1][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][A-Z]')"));

        // CHECK: valores válidos de Sexo (ajusta si quieres permitir 'O')
        b.ToTable(t => t.HasCheckConstraint("CK_PERSONA_SEXO", "Sexo IN ('M','F')"));

        // (Opcional) teléfono normalizado E.164 nica: ########  (8 dígitos)
        b.ToTable(t => t.HasCheckConstraint(
            "CK_PERSONA_TEL_NI",
            "NumeroTelefono IS NULL OR NumeroTelefono LIKE '________'"));
    }
}
