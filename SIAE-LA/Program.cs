using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text;

using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no está configurada")));

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

// CORS para Angular
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(o => o.AddPolicy("spa", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

//builder.Services.AddCors(opt =>
//{
//    opt.AddPolicy("spa", p => p
//        .WithOrigins("http://localhost:4200")
//        .AllowAnyHeader()
//        .AllowAnyMethod()
//        .AllowCredentials());
//});

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

app.MapControllers();
app.Run();
