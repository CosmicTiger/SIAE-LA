#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server and DefaultConnection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost,1433;Database=SIAE_LA_Db;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;"
    ));

// Add Identity with ApplicationUser and EntityFrameworkStores
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// AddControllersWithViews + global validation filter
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Authorization policies: "PuedeDireccionar" (Direccion, Subdireccion)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PuedeDireccionar", policy =>
        policy.RequireRole("Direccion", "Subdireccion"));
});

builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed data at startup
await app.UseDataSeeder();

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();
