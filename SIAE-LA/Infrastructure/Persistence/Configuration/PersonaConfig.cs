using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// no value converter needed now: FechaNacimiento is stored as DateTime
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configuration;

public class PersonaConfig : IEntityTypeConfiguration<Persona>
{
        public void Configure(EntityTypeBuilder<Persona> b)
        {
        b.ToTable("persona");
        b.HasKey(x => x.Id);

        b.Property(x => x.Nombres).HasMaxLength(100).IsRequired().HasColumnName("nombres");
        b.Property(x => x.Apellidos).HasMaxLength(100).IsRequired().HasColumnName("apellidos");

            b.Property(x => x.FechaNacimiento)
                .HasColumnName("fecha_nacimiento")
                .IsRequired();
        b.Property(x => x.Sexo).HasMaxLength(1).IsRequired().HasColumnName("sexo");

        b.Property(x => x.DocumentoIdentidad).HasMaxLength(30).IsRequired().HasColumnName("documento_identidad");
        b.HasIndex(x => x.DocumentoIdentidad).IsUnique();

        // OJO: NO se definen aquí los CHECKs de formato ni teléfono.
        // Se añadirán en ApplicationDbContext según provider.
        b.Property(x => x.Ciudad).HasMaxLength(80).HasColumnName("ciudad");
        b.Property(x => x.Direccion).HasMaxLength(140).HasColumnName("direccion");
        b.Property(x => x.Email).HasMaxLength(120).HasColumnName("email");
        b.Property(x => x.NumeroTelefono).HasMaxLength(30).HasColumnName("numero_telefono");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
        b.Property(x => x.Codigo).HasMaxLength(30).HasColumnName("codigo");
        b.Property(x => x.ValorCodigo).HasMaxLength(30).HasColumnName("valor_codigo");
    }
}