using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class GrupoConfig : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> b)
    {
        b.HasOne(x => x.Nivel)
         .WithMany(n => n.Grupos)
         .HasForeignKey(x => x.NivelId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
