#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Controllers;
using SIAE_LA.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace SIAE_LA.Tests;

public class AlumnosControllerTests
{
    private static ApplicationDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var ctx = new ApplicationDbContext(options);
        return ctx;
    }

    private static UserManager<ApplicationUser> BuildUserManager(ApplicationDbContext ctx)
    {
        var store = new UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string>(ctx);
        var identityOptions = Options.Create(new IdentityOptions());
        var pwdHasher = new PasswordHasher<ApplicationUser>();
        var userValidators = new IUserValidator<ApplicationUser>[] { new UserValidator<ApplicationUser>() };
        var pwdValidators = new IPasswordValidator<ApplicationUser>[] { new PasswordValidator<ApplicationUser>() };
        var normalizer = new UpperInvariantLookupNormalizer();
        var describer = new IdentityErrorDescriber();
        return new UserManager<ApplicationUser>(
            store,
            identityOptions,
            pwdHasher,
            userValidators,
            pwdValidators,
            normalizer,
            describer,
            /* IServiceProvider */ null,
            NullLogger<UserManager<ApplicationUser>>.Instance);
    }

    [Fact]
    public async Task AssignTutor_CreatesAssignment_ClosesPrevious()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var ctx = CreateInMemoryContext(dbName);
        // seed alumno, apoderados and an existing active assignment
        var personaAlumno = new Persona { Nombres = "Alumno", Apellidos = "Uno", DocumentoIdentidad = "001-010120-0001A", FechaNacimiento = DateTime.UtcNow.AddYears(-10), Sexo = "M" };
        ctx.Personas.Add(personaAlumno);
        await ctx.SaveChangesAsync();

        var alumno = new Alumno { PersonaId = personaAlumno.Id, Activo = true };
        ctx.Alumnos.Add(alumno);
        await ctx.SaveChangesAsync();

        // apoderado anterior
        var personaAp1 = new Persona { Nombres = "Tutor", Apellidos = "Viejo", DocumentoIdentidad = "002-010180-0002B", FechaNacimiento = DateTime.UtcNow.AddYears(-40), Sexo = "F" };
        ctx.Personas.Add(personaAp1);
        await ctx.SaveChangesAsync();
        var ap1 = new Apoderado { PersonaId = personaAp1.Id, Activo = true };
        ctx.Apoderados.Add(ap1);
        await ctx.SaveChangesAsync();

        // apoderado nuevo
        var personaAp2 = new Persona { Nombres = "Tutor", Apellidos = "Nuevo", DocumentoIdentidad = "003-020190-0003C", FechaNacimiento = DateTime.UtcNow.AddYears(-35), Sexo = "M" };
        ctx.Personas.Add(personaAp2);
        await ctx.SaveChangesAsync();
        var ap2 = new Apoderado { PersonaId = personaAp2.Id, Activo = true };
        ctx.Apoderados.Add(ap2);
        await ctx.SaveChangesAsync();

        // existing active assignment to ap1
        var active = new AlumnoApoderado
        {
            AlumnoId = alumno.Id,
            ApoderadoId = ap1.Id,
            FechaInicio = DateTime.UtcNow.AddMonths(-6),
            FechaFin = null,
            EsResponsableLegal = true
        };
        ctx.AlumnosApoderados.Add(active);
        await ctx.SaveChangesAsync();

        var um = BuildUserManager(ctx);
        var controller = new AlumnosController(ctx, um);

        var dto = new TutorAssignCreateDto { ApoderadoId = ap2.Id, EsResponsableLegal = true };

        // Act
        var actionResult = await controller.AssignTutor(alumno.Id, dto);

        // Assert shape
        Assert.IsType<ActionResult<ApiResponse<TutorAssignmentDto>>>(actionResult);

        // Verify previous assignment closed and new assignment exists active
        var prev = await ctx.AlumnosApoderados.FindAsync(active.Id);
        Assert.NotNull(prev);
        Assert.NotNull(prev!.FechaFin);

        var newAssign = ctx.AlumnosApoderados.SingleOrDefault(a => a.ApoderadoId == ap2.Id && a.AlumnoId == alumno.Id);
        Assert.NotNull(newAssign);
        Assert.Null(newAssign!.FechaFin);
        Assert.True(newAssign.EsResponsableLegal);
    }

    [Fact]
    public async Task EndTutorAssignment_SetsFechaFin()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var ctx = CreateInMemoryContext(dbName);

        var personaAlumno = new Persona { Nombres = "Alumno", Apellidos = "Dos", DocumentoIdentidad = "004-030120-0004D", FechaNacimiento = DateTime.UtcNow.AddYears(-11), Sexo = "F" };
        ctx.Personas.Add(personaAlumno);
        await ctx.SaveChangesAsync();

        var alumno = new Alumno { PersonaId = personaAlumno.Id, Activo = true };
        ctx.Alumnos.Add(alumno);
        await ctx.SaveChangesAsync();

        var personaAp = new Persona { Nombres = "Tutor", Apellidos = "Activo", DocumentoIdentidad = "005-040170-0005E", FechaNacimiento = DateTime.UtcNow.AddYears(-45), Sexo = "M" };
        ctx.Personas.Add(personaAp);
        await ctx.SaveChangesAsync();
        var ap = new Apoderado { PersonaId = personaAp.Id, Activo = true };
        ctx.Apoderados.Add(ap);
        await ctx.SaveChangesAsync();

        var assignment = new AlumnoApoderado
        {
            AlumnoId = alumno.Id,
            ApoderadoId = ap.Id,
            FechaInicio = DateTime.UtcNow.AddMonths(-3),
            FechaFin = null,
            EsResponsableLegal = false
        };
        ctx.AlumnosApoderados.Add(assignment);
        await ctx.SaveChangesAsync();

        var um = BuildUserManager(ctx);
        var controller = new AlumnosController(ctx, um);

        // Act
        var actionResult = await controller.EndTutorAssignment(alumno.Id, assignment.Id, null);

        // Assert response shape
        Assert.IsType<ActionResult<ApiResponse<string>>>(actionResult);

        var refreshed = await ctx.AlumnosApoderados.FindAsync(assignment.Id);
        Assert.NotNull(refreshed);
        Assert.NotNull(refreshed!.FechaFin);
        // FechaFin should be recent (within a minute)
        Assert.True((DateTime.UtcNow - refreshed.FechaFin.Value).TotalMinutes < 2);
    }
}