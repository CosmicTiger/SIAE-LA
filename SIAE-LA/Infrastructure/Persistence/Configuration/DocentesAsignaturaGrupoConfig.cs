using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class DocenteAsignaturaGrupoConfig : IEntityTypeConfiguration<DocenteAsignaturaGrupo>
{
    public void Configure(EntityTypeBuilder<DocenteAsignaturaGrupo> b)
    {
        b.HasIndex(x => new { x.DocenteId, x.AsignaturaId, x.GrupoId, x.PeriodoId }).IsUnique();

        b.HasOne(x => x.Docente)
         .WithMany(d => d.Asignaciones)
         .HasForeignKey(x => x.DocenteId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Asignatura)
         .WithMany(a => a.Asignaciones)
         .HasForeignKey(x => x.AsignaturaId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Grupo)
         .WithMany(g => g.Asignaciones)
         .HasForeignKey(x => x.GrupoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Periodo)
         .WithMany()
         .HasForeignKey(x => x.PeriodoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
