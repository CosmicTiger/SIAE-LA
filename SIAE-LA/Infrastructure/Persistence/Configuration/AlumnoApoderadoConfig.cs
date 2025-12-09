#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configuration;

public class AlumnoApoderadoConfig : IEntityTypeConfiguration<AlumnoApoderado>
{
    public void Configure(EntityTypeBuilder<AlumnoApoderado> b)
    {
        b.ToTable("alumno_apoderado");
        b.Property(x => x.Id).HasColumnName("alumno_apoderado_id");
        b.Property(x => x.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        b.Property(x => x.FechaFin).HasColumnName("fecha_fin");
        b.Property(x => x.EsResponsableLegal).HasColumnName("es_responsable_legal").HasDefaultValue(false);

        b.HasOne(x => x.Alumno)
            .WithMany(a => a.Apoderados) // requiere navegación en Alumno
            .HasForeignKey(x => x.AlumnoId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Apoderado)
            .WithMany() // no navegar desde Apoderado por ahora
            .HasForeignKey(x => x.ApoderadoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.AlumnoId);
        b.HasIndex(x => x.ApoderadoId);
        // Útil para búsquedas de la asignación activa (FechaFin IS NULL)
        b.HasIndex(x => new { x.AlumnoId, x.FechaFin }).HasDatabaseName("IX_ALUMNO_APODERADO_ALUMNO_FECHAFIN");
    }
}