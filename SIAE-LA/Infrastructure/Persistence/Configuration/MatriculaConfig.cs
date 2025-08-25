using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class MatriculaConfig : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> b)
    {
        b.HasIndex(x => new { x.EstudianteId, x.GrupoId, x.PeriodoId }).IsUnique();

        b.HasOne(x => x.Estudiante)
         .WithMany(e => e.Matriculas)
         .HasForeignKey(x => x.EstudianteId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Grupo)
         .WithMany(g => g.Matriculas)
         .HasForeignKey(x => x.GrupoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Periodo)
         .WithMany()
         .HasForeignKey(x => x.PeriodoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
