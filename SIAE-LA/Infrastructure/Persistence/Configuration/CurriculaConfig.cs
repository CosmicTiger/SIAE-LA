using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class CurriculaConfig : IEntityTypeConfiguration<Curricula>
{
    public void Configure(EntityTypeBuilder<Curricula> b)
    {
        b.ToTable("curricula");
        b.Property(x => x.Id).HasColumnName("curricula_id");
        b.Property(x => x.DocenteNivelDetalleCursoId).HasColumnName("docente_nivel_detalle_curso_id");
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");
        b.HasOne(x => x.DocenteNivelDetalleCurso)
         .WithMany(dndc => dndc.Curriculas)
         .HasForeignKey(x => x.DocenteNivelDetalleCursoId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
