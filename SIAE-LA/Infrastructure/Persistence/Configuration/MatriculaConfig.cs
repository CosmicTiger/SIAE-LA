using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class MatriculaConfig : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> b)
    {
        b.ToTable("MATRICULA");

        b.HasIndex(x => new { x.AlumnoId, x.NivelDetalleId, x.PeriodoId }).IsUnique();

        b.HasOne(x => x.Alumno)
         .WithMany(a => a.Matriculas)
         .HasForeignKey(x => x.AlumnoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.NivelDetalle)
         .WithMany(nd => nd.Matriculas)
         .HasForeignKey(x => x.NivelDetalleId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Apoderado)
         .WithMany(a => a.Matriculas)
         .HasForeignKey(x => x.ApoderadoId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.Periodo)
         .WithMany()
         .HasForeignKey(x => x.PeriodoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
