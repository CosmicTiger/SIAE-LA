using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class HorarioConfig : IEntityTypeConfiguration<Horario>
{
    public void Configure(EntityTypeBuilder<Horario> b)
    {
        b.ToTable("horario");
        b.Property(x => x.Id).HasColumnName("horario_id");
        b.Property(x => x.NivelDetalleCursoId).HasColumnName("nivel_detalle_curso_id");
        b.Property(x => x.DiaSemana).HasColumnName("dia_semana").HasMaxLength(20).IsRequired();
        b.Property(x => x.HoraInicio).HasColumnName("hora_inicio").IsRequired();
        b.Property(x => x.HoraFin).HasColumnName("hora_fin").IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro").IsRequired();

        b.HasOne(x => x.NivelDetalleCurso)
         .WithMany(ndc => ndc.Horarios)
         .HasForeignKey(x => x.NivelDetalleCursoId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
