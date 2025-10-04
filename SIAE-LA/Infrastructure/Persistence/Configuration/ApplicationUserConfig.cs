using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.ToTable("AspNetUsers"); // conserva tabla por defecto
        b.HasOne(u => u.Persona)
         .WithOne()
         .HasForeignKey<ApplicationUser>(u => u.PersonaId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
