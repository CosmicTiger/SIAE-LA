using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class DocenteNivelDetalleCursoConfig : IEntityTypeConfiguration<DocenteNivelDetalleCurso>
{
    public void Configure(EntityTypeBuilder<DocenteNivelDetalleCurso> b)
    {
        b.ToTable("docente_nivel_detalle_curso");
        b.Property(x => x.Id).HasColumnName("docente_nivel_detalle_curso_id");
        b.Property(x => x.NivelDetalleCursoId).HasColumnName("nivel_detalle_curso_id");
        b.Property(x => x.DocenteId).HasColumnName("docente_id");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
        b.HasIndex(x => new { x.NivelDetalleCursoId, x.DocenteId }).IsUnique();

        b.HasOne(x => x.NivelDetalleCurso)
         .WithMany(ndc => ndc.Docentes)
         .HasForeignKey(x => x.NivelDetalleCursoId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Docente)
         .WithMany(d => d.Asignaciones)
         .HasForeignKey(x => x.DocenteId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
