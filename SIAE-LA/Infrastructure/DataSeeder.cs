// Infrastructure/DataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;

namespace SIAE_LA.Infrastructure;

public class DataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DataSeeder(
        ApplicationDbContext db,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        await SeedRolesAsync();
        await SeedAdminUserAsync("admin@demo.local", "Change_this_123!");

        await SeedCatalogsAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "Direccion", "Subdireccion", "JefeArea", "Docente" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedAdminUserAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null) return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "Administrador General"
        };

        var result = await _userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new InvalidOperationException($"No se pudo crear el usuario admin: {errors}");
        }
    }

    private async Task SeedCatalogsAsync()
    {
        if (!await _db.Niveles.AnyAsync())
        {
            var primaria = new Nivel { Nombre = "Primaria" };
            var secundaria = new Nivel { Nombre = "Secundaria" };
            _db.Niveles.AddRange(primaria, secundaria);
            await _db.SaveChangesAsync();

            _db.Grupos.AddRange(
                new Grupo { Nombre = "1-A", NivelId = primaria.Id },
                new Grupo { Nombre = "1-B", NivelId = primaria.Id },
                new Grupo { Nombre = "2-A", NivelId = primaria.Id },
                new Grupo { Nombre = "7-A", NivelId = secundaria.Id }
            );
        }

        if (!await _db.Asignaturas.AnyAsync())
        {
            _db.Asignaturas.AddRange(
                new Asignatura { Nombre = "Matemática", Codigo = "MAT-01", Creditos = 4 },
                new Asignatura { Nombre = "Lengua y Literatura", Codigo = "LEN-01", Creditos = 4 },
                new Asignatura { Nombre = "Ciencias Naturales", Codigo = "CIE-01", Creditos = 3 }
            );
        }

        if (!await _db.Periodos.AnyAsync())
        {
            var anio = DateTime.UtcNow.Year;
            _db.Periodos.AddRange(
                new Periodo { Anio = anio, NombreCorte = "I Corte" },
                new Periodo { Anio = anio, NombreCorte = "II Corte" },
                new Periodo { Anio = anio, NombreCorte = "III Corte" }
            );
        }

        await _db.SaveChangesAsync();
    }
}
