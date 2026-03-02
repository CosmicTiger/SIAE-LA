using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class CursoConfig : IEntityTypeConfiguration<Curso>
{
    public void Configure(EntityTypeBuilder<Curso> b)
    {
        b.ToTable("curso");
        b.Property(x => x.Id).HasColumnName("curso_id");
        b.Property(x => x.Codigo).HasColumnName("codigo");
        b.Property(x => x.Descripcion).HasColumnName("descripcion");
        b.Property(x => x.Activo).HasColumnName("activo");
        b.HasIndex(x => x.Codigo).HasDatabaseName("ix_curso_codigo");
        b.HasIndex(x => x.Codigo).IsUnique(false);
    }
}
