// DocentesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Utils;

namespace SIAE_LA.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class DocentesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocentesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        
        // ======================================================
        // GET: api/docentes/{id}
        // ======================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<DocenteReadDto>>> GetOne(int id)
        {
            var d = await _db.Docentes.AsNoTracking().Include(x => x.Persona)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (d is null)
                return NotFound(ApiResponse<DocenteReadDto>.Fail("Docente no encontrado."));

            var dto = new DocenteReadDto(
                d.Id,
                d.Persona.Nombres,
                d.Persona.Apellidos,
                null,
                d.Persona.DocumentoIdentidad,
                d.Persona.Ciudad,
                d.Persona.Direccion,
                d.Activo);
            return Ok(ApiResponse<DocenteReadDto>.Success(dto));
        }
        
        // ==============================================
        // POST Avanzado: Crear Docente + Persona + Usuario(Docente)
        // ==============================================
        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<DocenteCreateResultDto>>>Create(
            [FromBody] DocenteCreateWithAccountsDto req)
        {
            if (req.DocentePersona is null)
                return BadRequest(ApiResponse<DocenteCreateResultDto>.Fail("DocentePersona es requerido."));

            if (string.IsNullOrWhiteSpace(req.DocenteEmail) || string.IsNullOrWhiteSpace(req.DocentePassword))
                return BadRequest(ApiResponse<DocenteCreateResultDto>.Fail("Credenciales del docente son requeridas."));
            
            if (await _userManager.FindByEmailAsync(req.DocenteEmail) is not null)
                return Conflict(ApiResponse<DocenteCreateResultDto>.Fail("El email del docente ya está registrado."));

            if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.DocentePersona.NumeroTelefono,
                    out var telDocenteE164))
                return BadRequest(
                    ApiResponse<DocenteCreateResultDto>.Fail("Número telefónico de docente inválido para Nicaragua."));

            string? docDocente = req.DocentePersona.DocumentoIdentidad?.Trim();

            if (string.IsNullOrWhiteSpace(docDocente) ||
                !CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(docDocente, out _))
                return BadRequest(
                    ApiResponse<DocenteCreateResultDto>.Fail("Docente: DocumentoIdentidad Inválido (Cédula NI)."));

            if (await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == docDocente))
                return Conflict(ApiResponse<DocenteCreateResultDto>.Fail("DocumentoIdentidad ya existe."));

            using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                var pDocente = new Persona
                {
                    Nombres = req.DocentePersona.Nombres,
                    Apellidos = req.DocentePersona.Apellidos,
                    DocumentoIdentidad = docDocente,
                    FechaNacimiento = req.DocentePersona.FechaNacimiento,
                    Sexo = req.DocentePersona.Sexo,
                    Ciudad = req.DocentePersona.Ciudad,
                    Direccion = req.DocentePersona.Direccion,
                    NumeroTelefono = telDocenteE164,
                    Email = req.DocenteEmail
                };
                _db.Personas.Add(pDocente);

                await _db.SaveChangesAsync();

                var docente = new Docente
                {
                    PersonaId = pDocente.Id,
                    Activo = true
                };

                _db.Docentes.Add(docente);
                await _db.SaveChangesAsync();

                var userDocente = new ApplicationUser
                {
                    UserName = req.DocenteEmail,
                    Email = req.DocenteEmail,
                    EmailConfirmed = true,
                    FullName = $"{pDocente.Nombres} {pDocente.Apellidos}",
                    PersonaId = pDocente.Id,
                    IsApproved = false
                };

                var createDocente = await _userManager.CreateAsync(userDocente, req.DocentePassword);

                if (!createDocente.Succeeded)
                    return BadRequest(ApiResponse<DocenteCreateResultDto>.Fail(string.Join("; ",
                        createDocente.Errors.Select(e => e.Description))));

                await _userManager.AddToRoleAsync(userDocente, "Docente");

                await tx.CommitAsync();

                var result = new DocenteCreateResultDto
                {
                    DocenteId = docente.Id,
                    DocentePersonaId = pDocente.Id,
                    DocenteEmail = req.DocenteEmail,
                };

                return CreatedAtAction(nameof(GetOne), new { id = docente.Id },
                    ApiResponse<DocenteCreateResultDto>.Success(result, "Docente Creado"));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}