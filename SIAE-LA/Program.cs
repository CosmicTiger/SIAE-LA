using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
//var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
//    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada");

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//{
//    if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
//        options.UseNpgsql(connectionString);
//    else if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
//        options.UseSqlServer(connectionString);
//    else
//        throw new InvalidOperationException($"DatabaseProvider '{dbProvider}' no soportado.");
//});
// small helper to mask sensitive parts of connection strings for logs
static string MaskConnectionString(string cs)
{
    if (string.IsNullOrWhiteSpace(cs)) return cs ?? string.Empty;

    try
    {
        // Mask URL form: scheme://user:pass@host...
        var urlPattern = System.Text.RegularExpressions.Regex.Replace(
            cs,
            @"(://[^:@\/\s]+:)([^@\/\s]+)(@)",
            "$1*****$3",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (urlPattern != cs) return urlPattern;

        // Mask key=value pairs like Password=..., pwd=..., Pass=...
        var kvPattern = System.Text.RegularExpressions.Regex.Replace(
            cs,
            @"(?<=\b(password|pwd|pass)\s*=\s*)([^;]+)",
            "*****",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return kvPattern;
    }
    catch
    {
        return "*****";
    }
}

// Build a lightweight logger for startup diagnostics (console)
using var startupLoggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var startupLogger = startupLoggerFactory.CreateLogger("Startup");

// DbContext: choose provider based on config (default -> PostgreSQL)
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada");

// Log chosen variables (connection string masked)
startupLogger.LogInformation("Database provider configured: {Provider}", dbProvider);
startupLogger.LogInformation("DefaultConnection (masked): {ConnectionString}", MaskConnectionString(connectionString));

// Configure DbContext using provider selection. Defaults to PostgreSQL unless DatabaseProvider == "SqlServer"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        startupLogger.LogInformation("Configuring DbContext to use SQL Server.");
        options.UseSqlServer(connectionString);
    }
    else if (dbProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
             dbProvider.Equals("Npgsql", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(dbProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        startupLogger.LogInformation("Configuring DbContext to use PostgreSQL (Npgsql).");
        options.UseNpgsql(connectionString);
    }
    else
    {
        startupLogger.LogWarning("Unknown DatabaseProvider '{Provider}' — falling back to PostgreSQL (Npgsql).", dbProvider);
        options.UseNpgsql(connectionString);
    }

    options.EnableDetailedErrors();
});

// Identity (cookies siguen existiendo para SignInManager, pero no serán el default)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequireLowercase = true;
    o.Password.RequireUppercase = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Evitar redirecciones 302 a /Account/Login en APIs (devolver 401/403)
builder.Services.ConfigureApplicationCookie(o =>
{
    o.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
});

// JWT (lee y valida configuración)
string issuer = builder.Configuration["Jwt:Issuer"] ?? throw new("Jwt:Issuer missing");
string audience = builder.Configuration["Jwt:Audience"] ?? throw new("Jwt:Audience missing");
string key = builder.Configuration["Jwt:Key"] ?? throw new("Jwt:Key missing");
int minutes = int.TryParse(builder.Configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 60;

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false; // dev
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Servicios propios
builder.Services.AddScoped<ITokenService>(sp => new TokenService(issuer, audience, key, minutes));

// Controllers
builder.Services.AddControllers();

// Repository for reportes
builder.Services.AddScoped<SIAE_LA.Abstractions.IReportesRepository, SIAE_LA.Infrastructure.ReportesRepository>();

// CORS para Angular
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(o => o.AddPolicy("spa", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// Autorización
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("PuedeDireccionar", p => p.RequireRole("Direccion", "Subdireccion"));
});

// OpenAPI JSON (Swashbuckle) + Scalar UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SIAE-LA API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. Ej: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference("/docs", opts =>
        opts.WithTitle("SIAE-LA API")
            .WithDarkMode(true)
            .WithOpenApiRoutePattern("/openapi/{documentName}.json"));
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();

// Seed de datos iniciales (roles, usuario admin, datos de ejemplo)
await app.UseDataSeeder();

// MAP CONTROLLERS: exigir autorización por defecto para controladores API
// Esto hará que todos los endpoints de controllers requieran auten
app.MapControllers().RequireAuthorization();
app.Run();
