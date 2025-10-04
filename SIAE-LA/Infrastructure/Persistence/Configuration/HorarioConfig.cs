using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class HorarioConfig : IEntityTypeConfiguration<Horario>
{
    public void Configure(EntityTypeBuilder<Horario> b)
    {
        b.ToTable("HORARIO");

        b.HasOne(x => x.NivelDetalleCurso)
         .WithMany(ndc => ndc.Horarios)
         .HasForeignKey(x => x.NivelDetalleCursoId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
