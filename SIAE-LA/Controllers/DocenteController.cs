// DocentesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class DocentesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocentesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ======================================================================
        // POST: api/docentes/asignacion
        // Acepta lista de asignaciones { DocenteId, NivelDetalleCursoId, Activo }
        // ======================================================================
        [HttpPost("asignacion")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DocenteCursoDto>>>> PostAsignacion([FromBody] IEnumerable<DocenteAsignacionDto> asignaciones)
        {
            if (asignaciones is null) return BadRequest(ApiResponse<IEnumerable<DocenteCursoDto>>.Fail("Payload requerido"));

            var list = asignaciones.ToList();
            if (list.Count == 0) return BadRequest(ApiResponse<IEnumerable<DocenteCursoDto>>.Fail("Lista vacía"));

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var result = new List<DocenteCursoDto>();

                foreach (var item in list)
                {
                    // validar existencia de docente y nivel_detalle_curso
                    var docenteExists = await _db.Docentes.AnyAsync(d => d.Id == item.DocenteId);
                    if (!docenteExists) return BadRequest(ApiResponse<IEnumerable<DocenteCursoDto>>.Fail($"Docente {item.DocenteId} no existe"));

                    var ndc = await _db.NivelesDetalleCurso
                        .Include(n => n.NivelDetalle).ThenInclude(nd => nd.Nivel)
                        .Include(n => n.NivelDetalle).ThenInclude(nd => nd.GradoSeccion)
                        .Include(n => n.Curso)
                        .FirstOrDefaultAsync(n => n.Id == item.NivelDetalleCursoId);

                    if (ndc is null) return BadRequest(ApiResponse<IEnumerable<DocenteCursoDto>>.Fail($"NivelDetalleCurso {item.NivelDetalleCursoId} no existe"));

                    // buscar asignación existente
                    var existing = await _db.DocentesNivelDetalleCurso
                        .FirstOrDefaultAsync(a => a.DocenteId == item.DocenteId && a.NivelDetalleCursoId == item.NivelDetalleCursoId);

                    if (existing is null)
                    {
                        existing = new DocenteNivelDetalleCurso
                        {
                            DocenteId = item.DocenteId,
                            NivelDetalleCursoId = item.NivelDetalleCursoId,
                            Activo = item.Activo
                        };
                        _db.DocentesNivelDetalleCurso.Add(existing);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        existing.Activo = item.Activo;
                        _db.DocentesNivelDetalleCurso.Update(existing);
                        await _db.SaveChangesAsync();
                    }

                    // construir DTO de salida
                    var dto = new DocenteCursoDto
                    {
                        Id = existing.Id,
                        DocenteId = existing.DocenteId,
                        NivelDetalleCursoId = existing.NivelDetalleCursoId,
                        NivelId = ndc.NivelDetalle.NivelId,
                        NivelDescripcion = ndc.NivelDetalle.Nivel.DescripcionNivel,
                        GradoSeccionId = ndc.NivelDetalle.GradoSeccionId,
                        GradoDescripcion = ndc.NivelDetalle.GradoSeccion.DescripcionGrado + " - " + ndc.NivelDetalle.GradoSeccion.DescripcionSeccion,
                        CursoId = ndc.CursoId,
                        CursoDescripcion = ndc.Curso.Descripcion,
                        Activo = existing.Activo,
                        FechaRegistro = existing.FechaRegistro
                    };

                    result.Add(dto);
                }

                await tx.CommitAsync();
                return Ok(ApiResponse<IEnumerable<DocenteCursoDto>>.Success(result, "Asignaciones procesadas"));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ======================================================================
        // GET: api/docentes/obtener-notas
        // Parámetros opcionales: periodoId, docenteId, nivelDetalleId, cursoId, alumnoId
        // ======================================================================
        [HttpGet("obtener-notas")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CalificacionReadDto>>>> ObtenerNotas(
            [FromQuery] int? periodoId = null,
            [FromQuery] int? docenteId = null,
            [FromQuery] int? nivelDetalleId = null,
            [FromQuery] int? cursoId = null,
            [FromQuery] int? alumnoId = null)
        {
            var q = _db.Calificaciones
                .AsNoTracking()
                .Include(c => c.Curricula).ThenInclude(cur => cur.DocenteNivelDetalleCurso).ThenInclude(dndc => dndc.NivelDetalleCurso).ThenInclude(ndc => ndc.NivelDetalle)
                .AsQueryable();

            if (docenteId.HasValue)
                q = q.Where(c => c.Curricula.DocenteNivelDetalleCurso.DocenteId == docenteId.Value);

            if (nivelDetalleId.HasValue)
                q = q.Where(c => c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.NivelDetalleId == nivelDetalleId.Value);

            if (cursoId.HasValue)
                q = q.Where(c => c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.CursoId == cursoId.Value);

            if (alumnoId.HasValue)
                q = q.Where(c => c.AlumnoId == alumnoId.Value);

            if (periodoId.HasValue)
            {
                var pid = periodoId.Value;
                q = q.Where(c => _db.Matriculas.Any(m => m.AlumnoId == c.AlumnoId && m.NivelDetalleId == c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.NivelDetalleId && m.PeriodoId == pid));
            }

            var list = await q.OrderByDescending(c => c.FechaRegistro)
                .Select(c => new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo))
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<CalificacionReadDto>>.Success(list));
        }

        // ======================================================================
        // POST: api/docentes/guardar-notas
        // Acepta array JSON de CalificacionCreateDto y realiza upsert (crear o actualizar)
        // ======================================================================
        [HttpPost("guardar-notas")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CalificacionReadDto>>>> GuardarNotas([FromBody] IEnumerable<CalificacionCreateDto> notas)
        {
            if (notas is null) return BadRequest(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail("Payload requerido"));
            var list = notas.ToList();
            if (list.Count == 0) return BadRequest(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail("Lista vacía"));

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var processed = new List<Calificacion>();

                foreach (var n in list)
                {
                    // validar existencia de curricula y alumno
                    var cur = await _db.Curriculas
                        .Include(cu => cu.DocenteNivelDetalleCurso).ThenInclude(dndc => dndc.NivelDetalleCurso)
                        .FirstOrDefaultAsync(cu => cu.Id == n.CurriculaId);
                    if (cur is null) return BadRequest(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail($"Curricula {n.CurriculaId} no existe"));

                    var alumnoExists = await _db.Alumnos.AnyAsync(a => a.Id == n.AlumnoId);
                    if (!alumnoExists) return BadRequest(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail($"Alumno {n.AlumnoId} no existe"));

                    // opcional: verificar matrícula (no obligatorio aquí)

                    var existing = await _db.Calificaciones.FirstOrDefaultAsync(c => c.CurriculaId == n.CurriculaId && c.AlumnoId == n.AlumnoId);
                    if (existing is null)
                    {
                        var c = new Calificacion
                        {
                            CurriculaId = n.CurriculaId,
                            AlumnoId = n.AlumnoId,
                            Nota = n.Nota,
                            Activo = true
                        };
                        _db.Calificaciones.Add(c);
                        processed.Add(c);
                    }
                    else
                    {
                        existing.Nota = n.Nota;
                        existing.Activo = true;
                        _db.Calificaciones.Update(existing);
                        processed.Add(existing);
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // construir DTOs de salida
                var outList = processed.Select(c => new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo)).ToList();
                return Ok(ApiResponse<IEnumerable<CalificacionReadDto>>.Success(outList, "Notas guardadas"));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ======================================================================
        // CURRICULA: list / create / delete
        // GET: api/docentes/curriculas?docenteNivelDetalleCursoId=123
        // POST: api/docentes/curricula
        // DELETE: api/docentes/curricula/{id}
        // ======================================================================
        [HttpGet("curriculas")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CurriculaDto>>>> GetCurriculas([FromQuery] int? docenteNivelDetalleCursoId = null)
        {
            var q = _db.Curriculas.AsNoTracking().Include(c => c.DocenteNivelDetalleCurso).AsQueryable();
            if (docenteNivelDetalleCursoId.HasValue)
                q = q.Where(c => c.DocenteNivelDetalleCursoId == docenteNivelDetalleCursoId.Value);

            var list = await q.OrderByDescending(c => c.FechaRegistro)
                .Select(c => new CurriculaDto
                {
                    Id = c.Id,
                    DocenteNivelDetalleCursoId = c.DocenteNivelDetalleCursoId,
                    Titulo = c.Descripcion ?? string.Empty,
                    Descripcion = c.Descripcion ?? string.Empty,
                    Activo = c.Activo,
                    FechaRegistro = c.FechaRegistro
                }).ToListAsync();

            return Ok(ApiResponse<IEnumerable<CurriculaDto>>.Success(list));
        }

        [HttpPost("curricula")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<CurriculaDto>>> CreateCurricula([FromBody] CurriculaCreateDto dto)
        {
            if (dto is null) return BadRequest(ApiResponse<CurriculaDto>.Fail("Payload requerido"));

            var asign = await _db.DocentesNivelDetalleCurso.FindAsync(dto.DocenteNivelDetalleCursoId);
            if (asign is null) return BadRequest(ApiResponse<CurriculaDto>.Fail("Asignación docente no encontrada"));

            var c = new Curricula { DocenteNivelDetalleCursoId = dto.DocenteNivelDetalleCursoId, Descripcion = dto.Descripcion, Activo = true };
            _db.Curriculas.Add(c);
            await _db.SaveChangesAsync();

            var outDto = new CurriculaDto { Id = c.Id, DocenteNivelDetalleCursoId = c.DocenteNivelDetalleCursoId, Titulo = c.Descripcion ?? string.Empty, Descripcion = c.Descripcion ?? string.Empty, Activo = c.Activo, FechaRegistro = c.FechaRegistro };
            return CreatedAtAction(nameof(GetCurriculas), new { id = c.Id }, ApiResponse<CurriculaDto>.Success(outDto, "Currícula creada"));
        }

        [HttpDelete("curricula/{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCurricula(int id)
        {
            var c = await _db.Curriculas.FindAsync(id);
            if (c is null) return NotFound(ApiResponse<string>.Fail("Currícula no encontrada"));
            c.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Currícula desactivada"));
        }

        // ======================================================================
        // GET: api/docentes/niveles/{docenteId}
        // Devuelve niveles (NivelDetalle) asociados al docente
        // ======================================================================
        [HttpGet("niveles/{docenteId:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<IEnumerable<NivelResumenDto>>>> GetNivelesByDocente(int docenteId)
        {
            var docenteExists = await _db.Docentes.AnyAsync(d => d.Id == docenteId);
            if (!docenteExists) return NotFound(ApiResponse<IEnumerable<NivelResumenDto>>.Fail("Docente no encontrado"));

            var q = _db.DocentesNivelDetalleCurso
                .AsNoTracking()
                .Where(a => a.DocenteId == docenteId && a.Activo)
                .Select(a => new
                {
                    NivelDetalleId = a.NivelDetalleCurso.NivelDetalle.Id,
                    NivelId = a.NivelDetalleCurso.NivelDetalle.NivelId,
                    NivelDescripcion = a.NivelDetalleCurso.NivelDetalle.Nivel.DescripcionNivel,
                    NivelTurno = a.NivelDetalleCurso.NivelDetalle.Nivel.DescripcionTurno,
                    GradoSeccionId = a.NivelDetalleCurso.NivelDetalle.GradoSeccionId,
                    GradoDescripcion = a.NivelDetalleCurso.NivelDetalle.GradoSeccion.DescripcionGrado,
                    SeccionDescripcion = a.NivelDetalleCurso.NivelDetalle.GradoSeccion.DescripcionSeccion
                })
                .GroupBy(x => x.NivelDetalleId)
                .Select(g => g.First());

            var list = await q
                .Select(x => new NivelResumenDto(
                    x.NivelDetalleId,
                    x.NivelId,
                    x.NivelDescripcion,
                    x.NivelTurno,
                    x.GradoSeccionId,
                    x.GradoDescripcion,
                    x.SeccionDescripcion
                ))
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<NivelResumenDto>>.Success(list));
        }

        // ======================================================================
        // GET: api/docentes/grados
        // Devuelve lista de GradoSeccion activos (opcional búsqueda)
        // ======================================================================
        [HttpGet("grados")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<IEnumerable<GradoSeccionDto>>>> GetGrados([FromQuery] QueryParams q)
        {
            var query = _db.GradoSecciones.AsNoTracking().Where(g => g.Activo);
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(g => EF.Functions.Like(g.DescripcionGrado ?? "", $"%{s}%") || EF.Functions.Like(g.DescripcionSeccion ?? "", $"%{s}%"));
            }

            var items = await query.OrderBy(g => g.DescripcionGrado).ThenBy(g => g.DescripcionSeccion)
                .Select(g => new GradoSeccionDto(g.Id, g.DescripcionGrado, g.DescripcionSeccion))
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<GradoSeccionDto>>.Success(items));
        }

        // ======================================================================
        // GET: api/docentes/cursos
        // Devuelve lista de cursos; si se pasa docenteId, devuelve cursos asignados a ese docente
        // ======================================================================
        [HttpGet("cursos")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CursoReadDto>>>> GetCursos([FromQuery] QueryParams q, [FromQuery] int? docenteId = null, [FromQuery] string? list = "ACTIVE")
        {
            var listMode = (list ?? "ACTIVE").Trim().ToUpperInvariant();

            if (docenteId.HasValue)
            {
                var exists = await _db.Docentes.AnyAsync(d => d.Id == docenteId.Value);
                if (!exists) return NotFound(ApiResponse<IEnumerable<CursoReadDto>>.Fail("Docente no encontrado"));

                var asigns = _db.DocentesNivelDetalleCurso
                    .AsNoTracking()
                    .Where(a => a.DocenteId == docenteId.Value);

                if (listMode != "ALL") asigns = asigns.Where(a => a.Activo);

                var cursosQ = asigns
                    .Select(a => a.NivelDetalleCurso.Curso)
                    .Distinct();

                if (!string.IsNullOrWhiteSpace(q.Search))
                {
                    var s = q.Search.Trim();
                    cursosQ = cursosQ.Where(c => EF.Functions.Like(c.Descripcion, $"%{s}%") || (c.Codigo ?? "") .Contains(s));
                }

                var listCursos = await cursosQ.OrderBy(c => c.Descripcion)
                    .Select(c => new CursoReadDto(c.Id, c.Descripcion, c.Codigo, c.Activo))
                    .ToListAsync();

                return Ok(ApiResponse<IEnumerable<CursoReadDto>>.Success(listCursos));
            }

            var query = _db.Cursos.AsNoTracking();
            if (listMode != "ALL") query = query.Where(c => c.Activo);
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(c => EF.Functions.Like(c.Descripcion, $"%{s}%") || (c.Codigo ?? "").Contains(s));
            }

            var items = await query.OrderBy(c => c.Descripcion)
                .Select(c => new CursoReadDto(c.Id, c.Descripcion, c.Codigo, c.Activo))
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<CursoReadDto>>.Success(items));
        }

        // ======================================================================
        // GET: api/docentes/asignados
        // ======================================================================
        [HttpGet("asignados")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<PaginationResult<DocenteCursoDto>>>> GetAsignados(
            [FromQuery] QueryParams q,
            [FromQuery] int? docenteId = null,
            [FromQuery] int? nivelId = null,
            [FromQuery] int? gradoSeccionId = null,
            [FromQuery] int? cursoId = null,
            [FromQuery] string? list = "ACTIVE"
        )
        {
            var listMode = (list ?? "ACTIVE").Trim().ToUpperInvariant();

            var query = _db.DocentesNivelDetalleCurso
                .AsNoTracking()
                .Include(x => x.Docente).ThenInclude(d => d.Persona)
                .Include(x => x.NivelDetalleCurso).ThenInclude(ndc => ndc.NivelDetalle).ThenInclude(nd => nd.Nivel)
                .Include(x => x.NivelDetalleCurso).ThenInclude(ndc => ndc.NivelDetalle).ThenInclude(nd => nd.GradoSeccion)
                .Include(x => x.NivelDetalleCurso).ThenInclude(ndc => ndc.Curso)
                .AsQueryable();

            if (listMode != "ALL")
                query = query.Where(x => x.Activo);

            if (docenteId.HasValue)
                query = query.Where(x => x.DocenteId == docenteId.Value);

            if (nivelId.HasValue)
                query = query.Where(x => x.NivelDetalleCurso.NivelDetalle.NivelId == nivelId.Value);

            if (gradoSeccionId.HasValue)
                query = query.Where(x => x.NivelDetalleCurso.NivelDetalle.GradoSeccionId == gradoSeccionId.Value);

            if (cursoId.HasValue)
                query = query.Where(x => x.NivelDetalleCurso.CursoId == cursoId.Value);

            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(x =>
                    EF.Functions.Like(x.Docente.Persona.Nombres + " " + x.Docente.Persona.Apellidos, $"%{s}%") ||
                    EF.Functions.Like(x.NivelDetalleCurso.Curso.Descripcion ?? "", $"%{s}%")
                );
            }

            var total = await query.CountAsync();

            var page = Math.Max(1, q.Page);
            var pageSize = Math.Max(1, q.PageSize);
            var skip = (page - 1) * pageSize;

            var items = await query
                .OrderBy(x => x.Docente.Persona.Apellidos).ThenBy(x => x.Docente.Persona.Nombres)
                .Skip(skip).Take(pageSize)
                .Select(x => new DocenteCursoDto
                {
                    Id = x.Id,
                    DocenteId = x.DocenteId,
                    NivelDetalleCursoId = x.NivelDetalleCursoId,
                    NivelId = x.NivelDetalleCurso.NivelDetalle.NivelId,
                    NivelDescripcion = x.NivelDetalleCurso.NivelDetalle.Nivel.DescripcionNivel,
                    GradoSeccionId = x.NivelDetalleCurso.NivelDetalle.GradoSeccionId,
                    GradoDescripcion = x.NivelDetalleCurso.NivelDetalle.GradoSeccion.DescripcionGrado + " - " + x.NivelDetalleCurso.NivelDetalle.GradoSeccion.DescripcionSeccion,
                    CursoId = x.NivelDetalleCurso.CursoId,
                    CursoDescripcion = x.NivelDetalleCurso.Curso.Descripcion,
                    Activo = x.Activo,
                    FechaRegistro = x.FechaRegistro
                })
                .ToListAsync();

            var result = new PaginationResult<DocenteCursoDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalItems = total,
                Items = items
            };

            return Ok(ApiResponse<PaginationResult<DocenteCursoDto>>.Success(result));
        }

        // ======================================================================
        // PUT: api/docentes/{id}  (actualiza datos en Persona + Activo en Docente)
        // ======================================================================
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<DocenteReadDto>>> Update(int id, [FromBody] DocenteUpdateDto dto)
        {
            var d = await _db.Docentes
                .Include(x => x.Persona)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (d is null)
                return NotFound(ApiResponse<DocenteReadDto>.Fail("Docente no encontrado"));

            // Validar documento (cédula NI) y unicidad
            if (!string.IsNullOrWhiteSpace(dto.DocumentoIdentidad))
            {
                var doc = dto.DocumentoIdentidad.Trim();
                if (!CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(doc, out _))
                    return BadRequest(ApiResponse<DocenteReadDto>.Fail("DocumentoIdentidad inválido (cédula NI)."));

                if (!string.Equals(doc, d.Persona.DocumentoIdentidad, StringComparison.OrdinalIgnoreCase) &&
                    await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == doc))
                    return Conflict(ApiResponse<DocenteReadDto>.Fail("DocumentoIdentidad ya existe."));

                d.Persona.DocumentoIdentidad = doc;
            }

            // Normalizar teléfono (si existe)
            if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(dto.NumeroTelefono, out var telE164))
                return BadRequest(ApiResponse<DocenteReadDto>.Fail("Número telefónico de docente inválido para Nicaragua."));

            // Actualiza datos en Persona
            d.Persona.Nombres = dto.Nombres;
            d.Persona.Apellidos = dto.Apellidos;
            d.Persona.Ciudad = dto.Ciudad;
            d.Persona.Direccion = dto.Direccion;
            d.Persona.NumeroTelefono = telE164;
            d.Activo = dto.Activo;

            await _db.SaveChangesAsync();

            var read = new DocenteReadDto(
                d.Id,
                d.Persona.Nombres,
                d.Persona.Apellidos,
                /* Codigo */ null,
                d.Persona.DocumentoIdentidad,
                d.Persona.Ciudad,
                d.Persona.Direccion,
                d.Activo
            );

            return Ok(ApiResponse<DocenteReadDto>.Success(read, "Docente actualizado"));
        }

        // ======================================================================
        // DELETE: api/docentes/{id} (soft-delete → Activo=false)
        // ======================================================================
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var d = await _db.Docentes.FindAsync(id);
            if (d is null) return NotFound(ApiResponse<string>.Fail("Docente no encontrado"));

            d.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Docente desactivado"));
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