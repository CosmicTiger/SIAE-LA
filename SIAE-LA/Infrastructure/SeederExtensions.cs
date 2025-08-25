// Infrastructure/SeederExtensions.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;

namespace SIAE_LA.Infrastructure;

public static class SeederExtensions
{
    public static async Task UseDataSeeder(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleMgr = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = services.GetRequiredService<UserManager<ApplicationUser>>();

        var seeder = new DataSeeder(db, roleMgr, userMgr);
        await seeder.SeedAsync();
    }
}
