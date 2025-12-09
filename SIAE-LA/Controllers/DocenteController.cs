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

        [HttpGet]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<PaginationResult<DocenteReadDto>>>> GetAll(
            [FromQuery] QueryParams q,
            [FromQuery] string? name = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phone = null,
            [FromQuery] string? list = "ACTIVE",
            [FromQuery] string? sortBy = "apellido",
            [FromQuery] string? sortDir = "asc"
        ) {
            // Normalizamos parámetros
            var listMode = (list ?? "ACTIVE").Trim().ToUpperInvariant();
            var sort = (sortBy ?? "apellido").Trim().ToLowerInvariant();
            var dir = (sortDir ?? "asc").Trim().ToLowerInvariant();
            if (dir != "asc" && dir != "desc") dir = "asc";

            // Base query (desde Alumnos con Persona)
            var query = _db.Docentes.AsNoTracking().Include(d => d.Persona).AsQueryable();

            // Filtrar por activos por defectos
            if (listMode != "ALL")
            {
                query = query.Where(d => d.Activo);
            }

            // Aplicar filtros de búsqueda específicos (si se pasan)
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                // mezcla de criterios: nombre/apellido o documento
                query = query.Where(d =>
                    EF.Functions.Like(d.Persona.Nombres + " " + d.Persona.Apellidos, $"%{s}%") ||
                    EF.Functions.Like(d.Persona.Apellidos + " " + d.Persona.Nombres, $"%{s}%") ||
                    EF.Functions.Like(d.Persona.DocumentoIdentidad ?? "", $"%{s}%") ||
                    EF.Functions.Like(d.Persona.Email ?? "", $"%{s}%")
                );
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var s = name.Trim();
                query = query.Where(d => 
                    EF.Functions.Like(d.Persona.Nombres + " " + d.Persona.Apellidos, $"%{s}%") ||
                    EF.Functions.Like(d.Persona.Apellidos + " " + d.Persona.Nombres, $"%{s}%")
                );
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var s = email.Trim();
                query = query.Where(a => EF.Functions.Like(a.Persona.Email ?? "", $"%{s}%"));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var s = phone.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Persona.NumeroTelefono ?? "", $"%{s}%")
                );
            }

            // Contar total antes de paginar
            var total = await query.CountAsync();

            var withLatest = query.Select(d => new
            {
                Docente = d,
            });

            // Aplicar ordenamiento
            switch (sort)
            {
                case "created":
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.Docente.FechaRegistro);
                    else
                        withLatest = withLatest.OrderBy(x => x.Docente.FechaRegistro);
                    break;

                case "email":
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.Docente.Persona.Email);
                    else
                        withLatest = withLatest.OrderBy(x => x.Docente.Persona.Email);
                    break;

                case "apellido":
                default:
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.Docente.Persona.Apellidos)
                            .ThenByDescending(x => x.Docente.Persona.Nombres);
                    else
                        withLatest = withLatest.OrderBy(x => x.Docente.Persona.Apellidos)
                            .ThenBy(x => x.Docente.Persona.Nombres);
                    break;
            }

            // Paginación
            var page = Math.Max(1, q.Page);
            var pageSize = Math.Max(1, q.PageSize);
            var skip = (page - 1) * pageSize;

            var listedItems = await withLatest
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new DocenteReadDto(
                    x.Docente.Id,
                    x.Docente.Persona.Nombres,
                    x.Docente.Persona.Apellidos,
                    /* Codigo */ null,
                    x.Docente.Persona.DocumentoIdentidad,
                    x.Docente.Persona.Ciudad,
                    x.Docente.Persona.Direccion,
                    x.Docente.Activo
                ))
                .ToListAsync();

            var result = new PaginationResult<DocenteReadDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalItems = total,
                Items = listedItems
            };

            return Ok(ApiResponse<PaginationResult<DocenteReadDto>>.Success(result));
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
                FechaNacimiento = req.DocentePersona.FechaNacimiento.HasValue ? req.DocentePersona.FechaNacimiento.Value : (DateTime?)null,
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