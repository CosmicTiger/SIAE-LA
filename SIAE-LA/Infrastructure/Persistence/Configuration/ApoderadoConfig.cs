using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Infrastructure.Persistence.Configurations;

public class ApoderadoConfig : IEntityTypeConfiguration<Apoderado>
{
    public void Configure(EntityTypeBuilder<Apoderado> b)
    {
        b.ToTable("APODERADO");
    }
}
