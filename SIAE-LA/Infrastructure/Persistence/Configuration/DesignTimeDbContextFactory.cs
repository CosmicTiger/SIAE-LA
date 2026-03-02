using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.Http;

namespace SIAE_LA.Infrastructure.Persistence.Configuration
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var provider = cfg["DatabaseProvider"] ?? "SqlServer"; // ← default a SQL Server
            var cs = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada");
            //?? "Server=localhost,1433;Database=SIAE_LA_Db;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;";

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                builder.UseSqlServer(cs);
            } else
            {
                builder.UseNpgsql(cs);
            }

            // Provide a default IHttpContextAccessor for design-time DbContext operations
            return new ApplicationDbContext(builder.Options, new HttpContextAccessor());
        }
    }
}
