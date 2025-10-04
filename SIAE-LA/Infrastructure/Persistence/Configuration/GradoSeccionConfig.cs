using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class GradoSeccionConfig : IEntityTypeConfiguration<GradoSeccion>
{
    public void Configure(EntityTypeBuilder<GradoSeccion> b)
    {
        b.ToTable("GRADO_SECCION");
    }
}
