using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class MatriculaConfig : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> b)
    {
        b.ToTable("matricula");

        b.Property(x => x.Id).HasColumnName("matricula_id");
        b.Property(x => x.ValorCodigo).HasColumnName("valor_codigo");
        b.Property(x => x.Codigo).HasColumnName("codigo");
        b.Property(x => x.Situacion).HasColumnName("situacion");
        b.Property(x => x.AlumnoId).HasColumnName("alumno_id");
        b.Property(x => x.NivelDetalleId).HasColumnName("nivel_detalle_id");
        b.Property(x => x.ApoderadoId).HasColumnName("apoderado_id");
        b.Property(x => x.InstitucionProcedencia).HasColumnName("institucion_procedencia");
        b.Property(x => x.EsRepitente).HasColumnName("es_repitente");
        b.Property(x => x.PeriodoId).HasColumnName("periodo_id");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.Property(x => x.FechaRegistro).HasColumnName("fecha_registro");

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
