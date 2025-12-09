using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Services;
using SIAE_LA.DTOs;
using SIAE_LA.Utils;
using Microsoft.EntityFrameworkCore;

namespace SIAE_LA.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> um,
        SignInManager<ApplicationUser> sm,
        ITokenService ts)
    {
        _db = db;
        _userManager = um;
        _signInManager = sm;
        _tokenService = ts;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Register([FromBody] RegisterDto dto)
    {
        var roles = (dto.Roles?.Length > 0 ? dto.Roles : new[] { "Docente" })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        bool isOnlyAdmin = roles.Length == 1 && roles[0].Equals("Admin", StringComparison.OrdinalIgnoreCase);
        bool requierePersona = !isOnlyAdmin;

        // Reglas:
        bool solicitaDocenteRole = roles.Contains("Docente", StringComparer.OrdinalIgnoreCase);
        bool solicitaJefeArea = roles.Contains("JefeArea", StringComparer.OrdinalIgnoreCase);
        bool solicitaDireccion = roles.Contains("Direccion", StringComparer.OrdinalIgnoreCase);
        bool solicitaSubdir = roles.Contains("Subdireccion", StringComparer.OrdinalIgnoreCase);

        // Debe tener fila en Docente si:
        // - Tiene rol Docente, o
        // - Tiene rol JefeArea, o
        // - Es Direccion/Subdireccion **y** EsDocente == true
        bool requiereDocente = solicitaDocenteRole || solicitaJefeArea ||
                               ((solicitaDireccion || solicitaSubdir) && dto.EsDocente);

        if (requierePersona && dto.Persona is null)
            return BadRequest(new { message = "Se requiere Persona para roles distintos a Admin." });

        if (requiereDocente && dto.Docente is null)
            return BadRequest(new { message = "Faltan datos de Docente (por reglas del rol)." });

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            return Conflict(new { message = "El email ya está registrado" });

        using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            int? personaId = null;
            string fullName = "Usuario";

            if (requierePersona)
            {
                // ↓↓↓ NEW: Validaciones/normalizaciones previas a crear Persona ↓↓↓
                if (string.IsNullOrWhiteSpace(dto.Persona!.Sexo))
                    return BadRequest(new { message = "Sexo es obligatorio." });

                if (dto.Persona!.FechaNacimiento is null)
                    return BadRequest(new { message = "FechaNacimiento es obligatoria." });

                if (string.IsNullOrWhiteSpace(dto.Persona!.DocumentoIdentidad))
                    return BadRequest(new { message = "DocumentoIdentidad es obligatorio." });

                var doc = dto.Persona!.DocumentoIdentidad.Trim();

                // En registro general NO aceptamos TUTOR-... (eso solo en AlumnosController)
                if (!CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(doc, out _))
                    return BadRequest(new { message = "DocumentoIdentidad inválido (cédula NI requerida)." });

                if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(dto.Persona!.NumeroTelefono, out var telE164))
                    return BadRequest(new { message = "Número telefónico inválido para Nicaragua." });

                if (await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == doc))
                    return Conflict(new { message = "DocumentoIdentidad ya existe." });
                // ↑↑↑ NEW

                var p = new Persona
                {
                    Nombres = dto.Persona!.Nombres,
                    Apellidos = dto.Persona!.Apellidos,
                    DocumentoIdentidad = doc,               // ← NEW (doc normalizado)
                    FechaNacimiento = dto.Persona!.FechaNacimiento,
                    Sexo = dto.Persona!.Sexo,
                    Ciudad = dto.Persona!.Ciudad,
                    Direccion = dto.Persona!.Direccion,
                    NumeroTelefono = telE164,               // ← NEW (tel E.164 or null)
                    Email = dto.Email
                };
                _db.Personas.Add(p);
                await _db.SaveChangesAsync();
                personaId = p.Id;
                fullName = $"{p.Nombres} {p.Apellidos}";

                if (requiereDocente)
                {
                    _db.Docentes.Add(new Docente
                    {
                        PersonaId = p.Id,
                        GradoEstudio = dto.Docente?.GradoEstudio
                    });
                    await _db.SaveChangesAsync();
                }
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                FullName = fullName,
                PersonaId = personaId,
                IsApproved = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });

            await _userManager.AddToRolesAsync(user, roles);

            await tx.CommitAsync();
            return Ok(new { message = "Usuario creado y pendiente de aprobación por un Administrador." });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null) return Unauthorized(new { message = "Credenciales inválidas" });

        var ok = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!ok.Succeeded) return Unauthorized(new { message = "Credenciales inválidas" });

        // Bloquea el acceso hasta ser aprobado
        if (!user.IsApproved)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Cuenta pendiente de aprobación por un Administrador." });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);
        return Ok(new
        {
            data = new AuthResponse(token, user.Email!, user.FullName, roles.ToList()),
            message = "Inicio de sesión exitoso"
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { user.Id, user.Email, user.FullName, user.PersonaId, Roles = roles });
    }
}
