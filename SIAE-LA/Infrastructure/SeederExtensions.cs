using Microsoft.AspNetCore.Identity;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure;
using SIAE_LA.Infrastructure.Persistence;

public static class SeederExtensions
{
    public static async Task UseDataSeeder(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

        var logger = services.GetRequiredService<ILogger<DataSeeder>>(); // Logger for seeder
        var config = services.GetRequiredService<IConfiguration>(); // Configuration for seeder
        var seeder = new DataSeeder(db, roleMgr, userMgr, logger, config); // Create seeder instance
        await seeder.SeedAsync();
    }
}
