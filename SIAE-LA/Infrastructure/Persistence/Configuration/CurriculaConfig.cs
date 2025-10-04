using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class CurriculaConfig : IEntityTypeConfiguration<Curricula>
{
    public void Configure(EntityTypeBuilder<Curricula> b)
    {
        b.ToTable("CURRICULA");
        b.HasOne(x => x.DocenteNivelDetalleCurso)
         .WithMany(dndc => dndc.Curriculas)
         .HasForeignKey(x => x.DocenteNivelDetalleCursoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
