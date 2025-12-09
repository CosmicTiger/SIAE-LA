using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.ToTable("usuarios"); // conserva tabla por defecto: AspNetUsers
        b.HasOne(u => u.Persona)
         .WithOne()
         .HasForeignKey<ApplicationUser>(u => u.PersonaId)
         .OnDelete(DeleteBehavior.Restrict);

        b.Property(u => u.PersonaId).HasColumnName("persona_id");
        b.Property(u => u.FullName).HasColumnName("full_name");
        b.Property(u => u.UserName).HasColumnName("username");
        b.Property(u => u.Email).HasColumnName("email");
        b.Property(u => u.EmailConfirmed).HasColumnName("es_email_confirmado");
        b.Property(u => u.PhoneNumber).HasColumnName("telefono");
        b.Property(u => u.PhoneNumberConfirmed).HasColumnName("es_telefono_confirmado");
        b.Property(u => u.PasswordHash).HasColumnName("password_hash");
        b.Property(u => u.IsApproved).HasColumnName("esta_aprobado");
        b.Property(u => u.ApprovedAt).HasColumnName("fecha_aprobacion");
        b.Property(u => u.ApprovedByUserId).HasColumnName("aprobado_por");

        b.HasOne(u => u.Persona)
         .WithOne()
         .HasForeignKey<ApplicationUser>(u => u.PersonaId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
