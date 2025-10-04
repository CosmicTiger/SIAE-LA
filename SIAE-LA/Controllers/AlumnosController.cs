// AlumnosController.cs
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
    public sealed class AlumnosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlumnosController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private static int CalcularEdad(DateTime fechaNacUtc)
        {
            var hoy = DateTime.UtcNow.Date;
            var edad = hoy.Year - fechaNacUtc.Date.Year;
            if (fechaNacUtc.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        // ======================================================================
        // GET: api/alumnos  (paginado + búsqueda por nombre/apellido/doc)
        // ======================================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginationResult<AlumnoReadDto>>>> GetAll([FromQuery] QueryParams q)
        {
            // Ahora todo se toma desde Persona
            var query = _db.Alumnos
                .AsNoTracking()
                .Include(a => a.Persona)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(a =>
                    (a.Persona.Nombres + " " + a.Persona.Apellidos).Contains(s) ||
                    (a.Persona.DocumentoIdentidad ?? "").Contains(s));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.Persona.Apellidos).ThenBy(a => a.Persona.Nombres)
                .Skip(q.Skip).Take(q.Take)
                .Select(a => new AlumnoReadDto(
                    a.Id,
                    a.Persona.Nombres,
                    a.Persona.Apellidos,
                    /* Codigo */ null, // ← si Persona no tiene "Codigo", enviamos null
                    a.Persona.DocumentoIdentidad,
                    a.Persona.Ciudad,
                    a.Persona.Direccion,
                    a.Activo))
                .ToListAsync();

            var page = new PaginationResult<AlumnoReadDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalItems = total,
                Items = items
            };

            return Ok(ApiResponse<PaginationResult<AlumnoReadDto>>.Success(page));
        }

        // ======================================================================
        // GET: api/alumnos/{id}
        // ======================================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AlumnoReadDto>>> GetOne(int id)
        {
            var a = await _db.Alumnos
                .AsNoTracking()
                .Include(x => x.Persona)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a is null)
                return NotFound(ApiResponse<AlumnoReadDto>.Fail("Alumno no encontrado"));

            var dto = new AlumnoReadDto(
                a.Id,
                a.Persona.Nombres,
                a.Persona.Apellidos,
                /* Codigo */ null,
                a.Persona.DocumentoIdentidad,
                a.Persona.Ciudad,
                a.Persona.Direccion,
                a.Activo);

            return Ok(ApiResponse<AlumnoReadDto>.Success(dto));
        }

        // ======================================================================
        // POST avanzado: crea Alumno + Persona + Usuario(Estudiante)
        // Si es menor de 18, también crea Tutor (Persona + Apoderado + Usuario(Tutor))
        // ======================================================================
        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<AlumnoCreateResultDto>>> Create([FromBody] AlumnoCreateWithAccountsDto req)
        {
            // Validaciones mínimas
            if (req.AlumnoPersona is null)
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("AlumnoPersona es requerido."));

            if (string.IsNullOrWhiteSpace(req.AlumnoEmail) || string.IsNullOrWhiteSpace(req.AlumnoPassword))
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Credenciales del alumno son requeridas."));

            if (req.AlumnoPersona.FechaNacimiento is null)
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("FechaNacimiento del alumno es obligatoria para validar mayoría de edad."));

            if (await _userManager.FindByEmailAsync(req.AlumnoEmail) is not null)
                return Conflict(ApiResponse<AlumnoCreateResultDto>.Fail("El email del alumno ya está registrado."));

            var edad = CalcularEdad(req.AlumnoPersona.FechaNacimiento.Value);
            var esMenor = edad < 18;

            if (esMenor)
            {
                if (req.Tutor is null)
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("El alumno es menor de edad: la información del Tutor es requerida."));

                if (await _userManager.FindByEmailAsync(req.Tutor.Email) is not null)
                    return Conflict(ApiResponse<AlumnoCreateResultDto>.Fail("El email del tutor ya está registrado."));

                if (string.IsNullOrWhiteSpace(req.Tutor.Password))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("La contraseña del tutor es requerida."));
            }

            // Validar/normalizar doc y teléfonos antes de grabar ↓↓↓
            if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.AlumnoPersona.NumeroTelefono, out var telAlumnoE164))
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Número telefónico de alumno inválido para Nicaragua."));

            string? docAlumno = req.AlumnoPersona.DocumentoIdentidad?.Trim();

            if (esMenor)
            {
                if (req.Tutor is null || string.IsNullOrWhiteSpace(req.Tutor.DocumentoIdentidad))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Tutor: DocumentoIdentidad requerido."));

                if (!CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(req.Tutor.DocumentoIdentidad.Trim(), out _))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Tutor: DocumentoIdentidad inválido (cédula NI)."));

                if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.Tutor.NumeroTelefono, out var _))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Tutor: número telefónico inválido para Nicaragua."));

                // Alumno menor debe quedar con TUTOR-<cedTutor>
                docAlumno = $"TUTOR-{req.Tutor.DocumentoIdentidad.Trim()}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(docAlumno) || !CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(docAlumno, out _))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Alumno: DocumentoIdentidad inválido (cédula NI)."));
            }

            if (await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == docAlumno))
                return Conflict(ApiResponse<AlumnoCreateResultDto>.Fail("DocumentoIdentidad ya existe."));
            // ↑↑↑ NEW

            using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1) Persona del alumno
                var pAlumno = new Persona
                {
                    Nombres = req.AlumnoPersona.Nombres,
                    Apellidos = req.AlumnoPersona.Apellidos,
                    DocumentoIdentidad = docAlumno!,                  // ← NEW
                    FechaNacimiento = req.AlumnoPersona.FechaNacimiento,
                    Sexo = req.AlumnoPersona.Sexo,
                    Ciudad = req.AlumnoPersona.Ciudad,
                    Direccion = req.AlumnoPersona.Direccion,
                    NumeroTelefono = telAlumnoE164,                  // ← NEW
                    Email = req.AlumnoEmail
                };
                _db.Personas.Add(pAlumno);
                await _db.SaveChangesAsync();

                // 2) Alumno
                var alumno = new Alumno
                {
                    PersonaId = pAlumno.Id,
                    Activo = true
                };
                _db.Alumnos.Add(alumno);
                await _db.SaveChangesAsync();

                // 3) Usuario del alumno (rol Estudiante) — pendiente aprobación
                var userAlumno = new ApplicationUser
                {
                    UserName = req.AlumnoEmail,
                    Email = req.AlumnoEmail,
                    EmailConfirmed = true,
                    FullName = $"{pAlumno.Nombres} {pAlumno.Apellidos}",
                    PersonaId = pAlumno.Id,
                    IsApproved = false
                };
                var createAlumno = await _userManager.CreateAsync(userAlumno, req.AlumnoPassword);
                if (!createAlumno.Succeeded)
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail(string.Join("; ", createAlumno.Errors.Select(e => e.Description))));

                await _userManager.AddToRoleAsync(userAlumno, "Estudiante");

                int? personaTutorId = null;
                int? apoderadoId = null;
                string? tutorEmail = null;

                // 4) Si es menor, crear Tutor (Persona + Apoderado + Usuario con rol Tutor)
                if (esMenor && req.Tutor is not null)
                {
                    // Normaliza teléfono tutor (ya validado arriba)
                    TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.Tutor.NumeroTelefono, out var telTutorE164);

                    var pTutor = new Persona
                    {
                        Nombres = req.Tutor.Nombres,
                        Apellidos = req.Tutor.Apellidos,
                        DocumentoIdentidad = req.Tutor.DocumentoIdentidad,
                        FechaNacimiento = req.Tutor.FechaNacimiento,
                        Sexo = req.Tutor.Sexo,
                        Ciudad = req.Tutor.Ciudad,
                        Direccion = req.Tutor.Direccion,
                        NumeroTelefono = telTutorE164,                // ← NEW
                        Email = req.Tutor.Email
                    };
                    _db.Personas.Add(pTutor);
                    await _db.SaveChangesAsync();
                    personaTutorId = pTutor.Id;

                    var apoderado = new Apoderado
                    {
                        PersonaId = pTutor.Id,
                        TipoParentesco = req.Tutor.TipoParentesco ?? "Tutor",
                        Activo = true
                    };
                    _db.Apoderados.Add(apoderado);
                    await _db.SaveChangesAsync();
                    apoderadoId = apoderado.Id;

                    var userTutor = new ApplicationUser
                    {
                        UserName = req.Tutor.Email,
                        Email = req.Tutor.Email,
                        EmailConfirmed = true,
                        FullName = $"{pTutor.Nombres} {pTutor.Apellidos}",
                        PersonaId = pTutor.Id,
                        IsApproved = false
                    };
                    var createTutor = await _userManager.CreateAsync(userTutor, req.Tutor.Password);
                    if (!createTutor.Succeeded)
                        return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail(string.Join("; ", createTutor.Errors.Select(e => e.Description))));

                    await _userManager.AddToRoleAsync(userTutor, "Tutor");
                    tutorEmail = req.Tutor.Email;
                }

                await tx.CommitAsync();

                var result = new AlumnoCreateResultDto
                {
                    AlumnoId = alumno.Id,
                    AlumnoPersonaId = pAlumno.Id,
                    AlumnoEmail = req.AlumnoEmail,
                    EsMenorDeEdad = esMenor,
                    TutorPersonaId = personaTutorId,
                    ApoderadoId = apoderadoId,
                    TutorEmail = tutorEmail
                };

                return CreatedAtAction(nameof(GetOne), new { id = alumno.Id }, ApiResponse<AlumnoCreateResultDto>.Success(result, "Alumno creado"));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ======================================================================
        // PUT: api/alumnos/{id}  (actualiza datos en Persona + Activo en Alumno)
        // ======================================================================
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<AlumnoReadDto>>> Update(int id, [FromBody] AlumnoUpdateDto dto)
        {
            var a = await _db.Alumnos
                .Include(x => x.Persona)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a is null)
                return NotFound(ApiResponse<AlumnoReadDto>.Fail("Alumno no encontrado"));

            // ↓↓↓ NEW: Validación básica de documento (cédula NI o TUTOR-<ced>) y unicidad ↓↓↓
            if (!string.IsNullOrWhiteSpace(dto.DocumentoIdentidad))
            {
                var doc = dto.DocumentoIdentidad.Trim();
                var okDoc = CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(doc, out _) || CedulaNicaraguenseValidadorHelper.IsTutorPattern(doc, out _);
                if (!okDoc) return BadRequest(ApiResponse<AlumnoReadDto>.Fail("DocumentoIdentidad inválido."));

                if (!string.Equals(doc, a.Persona.DocumentoIdentidad, StringComparison.OrdinalIgnoreCase) &&
                    await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == doc))
                    return Conflict(ApiResponse<AlumnoReadDto>.Fail("DocumentoIdentidad ya existe."));

                a.Persona.DocumentoIdentidad = doc;
            }
            // ↑↑↑ NEW

            // Actualiza datos en Persona (ya no en Alumno)
            a.Persona.Nombres = dto.Nombres;
            a.Persona.Apellidos = dto.Apellidos;
            a.Persona.Ciudad = dto.Ciudad;
            a.Persona.Direccion = dto.Direccion;
            a.Activo = dto.Activo;

            await _db.SaveChangesAsync();

            var read = new AlumnoReadDto(
                a.Id,
                a.Persona.Nombres,
                a.Persona.Apellidos,
                /* Codigo */ null,
                a.Persona.DocumentoIdentidad,
                a.Persona.Ciudad,
                a.Persona.Direccion,
                a.Activo);

            return Ok(ApiResponse<AlumnoReadDto>.Success(read, "Alumno actualizado"));
        }

        // ======================================================================
        // DELETE: api/alumnos/{id} (soft-delete → Activo=false)
        // ======================================================================
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var a = await _db.Alumnos.FindAsync(id);
            if (a is null) return NotFound(ApiResponse<string>.Fail("Alumno no encontrado"));

            a.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Alumno desactivado"));
        }
    }
}
