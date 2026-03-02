
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIAE_LA.Domain.Entities;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;


namespace SIAE_LA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class CalificacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CalificacionesController> _logger;

        public CalificacionesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<CalificacionesController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Registra una nueva calificación.
        /// Roles permitidos: Admin, Direccion, Subdireccion, JefeArea, Docente.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente")]
        public async Task<ActionResult<ApiResponse<CalificacionReadDto>>> Crear([FromBody] CalificacionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CalificacionReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));

            // Validar existencia de Curricula (incluyendo asignación docente->niveldetallecurso)
            var curricula = await _db.Curriculas
                .AsNoTracking()
                .Include(c => c.DocenteNivelDetalleCurso)
                    .ThenInclude(d => d.NivelDetalleCurso)
                .FirstOrDefaultAsync(c => c.Id == dto.CurriculaId);
            if (curricula is null) return NotFound(ApiResponse<CalificacionReadDto>.Fail("Currícula no encontrada"));

            var alumnoExists = await _db.Alumnos.AnyAsync(a => a.Id == dto.AlumnoId);
            if (!alumnoExists) return NotFound(ApiResponse<CalificacionReadDto>.Fail("Alumno no encontrado"));

            // Validar Periodo y coherencia con la matrícula del alumno
            var periodo = await _db.Periodos.FindAsync(dto.PeriodoId);
            if (periodo is null) return NotFound(ApiResponse<CalificacionReadDto>.Fail("Período no encontrado"));

            // Determinar el NivelDetalleId asociado a la currícula a través de DocenteNivelDetalleCurso -> NivelDetalleCurso
            if (curricula.DocenteNivelDetalleCurso is null || curricula.DocenteNivelDetalleCurso.NivelDetalleCurso is null)
                return BadRequest(ApiResponse<CalificacionReadDto>.Fail("La currícula no tiene asignación de nivel/curso válida."));

            var nivelDetalleId = curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.NivelDetalleId;

            // Asegurar que el alumno está matriculado en el año lectivo del periodo (si existe asignación)
            var matricula = await _db.Matriculas.AsNoTracking().FirstOrDefaultAsync(m => m.AlumnoId == dto.AlumnoId && m.NivelDetalleId == nivelDetalleId && m.AnioLectivoId == periodo.AnioLectivoId);
            if (matricula is null)
            {
                // Fallback: permitir si existe alguna matrícula del alumno en el mismo año lectivo
                var any = await _db.Matriculas.AnyAsync(m => m.AlumnoId == dto.AlumnoId && m.AnioLectivoId == periodo.AnioLectivoId);
                if (!any)
                    return BadRequest(ApiResponse<CalificacionReadDto>.Fail("El alumno no está matriculado en el año lectivo correspondiente para esta currícula."));
            }

            // Validaciones de negocio
            if (dto.Nota < 0 || dto.Nota > 100) return BadRequest(ApiResponse<CalificacionReadDto>.Fail("La nota debe estar entre 0 y 100"));

            // Regla de unicidad: (CurriculaId, AlumnoId)
            var exists = await _db.Calificaciones.AnyAsync(c => c.CurriculaId == dto.CurriculaId && c.AlumnoId == dto.AlumnoId);
            if (exists) return Conflict(ApiResponse<CalificacionReadDto>.Fail("Ya existe una calificación para este alumno en esta currícula."));

            var entity = new Calificacion
            {
                CurriculaId = dto.CurriculaId,
                AlumnoId = dto.AlumnoId,
                PeriodoId = dto.PeriodoId,
                Nota = dto.Nota,
                Activo = true
            };
            _db.Calificaciones.Add(entity);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Error guardando calificación (CurriculaId={CurriculaId}, AlumnoId={AlumnoId})", dto.CurriculaId, dto.AlumnoId);
                return StatusCode(500, ApiResponse<CalificacionReadDto>.Fail("Error al guardar la calificación. Ver logs para más detalles."));
            }

            var ai = SIAE_LA.Utils.AuditHelper.FromEntry(_db, entity);
            return Ok(ApiResponse<CalificacionReadDto>.Success(new CalificacionReadDto(entity.Id, entity.CurriculaId, entity.AlumnoId, entity.Nota, entity.FechaRegistro, entity.Activo, ai.CreadoPor, ai.ModificadoPor, ai.FechaModificacion, ai.FechaIngreso), "Calificación registrada"));
        }

        /// <summary>
        /// Actualiza una calificación existente.
        /// Roles permitidos: Admin, Direccion, Subdireccion, JefeArea, Docente.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente")]
        public async Task<ActionResult<ApiResponse<CalificacionReadDto>>> Editar(int id, [FromBody] CalificacionUpdateDto dto)
        {
            var c = await _db.Calificaciones.FindAsync(id);
            if (c is null) return NotFound(ApiResponse<CalificacionReadDto>.Fail("No existe la calificación"));
            c.Nota = dto.Nota; c.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            var ai2 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, c);
            return Ok(ApiResponse<CalificacionReadDto>.Success(new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo, ai2.CreadoPor, ai2.ModificadoPor, ai2.FechaModificacion, ai2.FechaIngreso), "Calificación actualizada"));
        }

        /// <summary>
        /// Obtiene las calificaciones de un alumno.
        /// Roles permitidos: Admin, Direccion, Subdireccion, JefeArea, Docente, Estudiante, Tutor.
        /// - Si el caller es Estudiante sólo puede ver sus propias calificaciones.
        /// - Si el caller es Tutor sólo puede ver calificaciones de sus pupilos con asignación activa.
        /// - Docentes y administrativos pueden consultar cualquier alumno.
        /// </summary>
        [HttpGet("by-alumno/{alumnoId:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CalificacionReadDto>>>> PorAlumno(int alumnoId, [FromQuery] int? periodoId, [FromQuery] int? anioLectivoId)
        {
            // Control de acceso para Estudiante/Tutor
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Estudiante"))
            {
                if (user?.PersonaId is null) return Forbid();
                var alumno = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.PersonaId == user.PersonaId);
                if (alumno is null || alumno.Id != alumnoId) return Forbid();
            }
            else if (User.IsInRole("Tutor"))
            {
                if (user?.PersonaId is null) return Forbid();
                var ap = await _db.Apoderados.AsNoTracking().FirstOrDefaultAsync(a => a.PersonaId == user.PersonaId);
                if (ap is null) return Forbid();
                var has = await _db.AlumnosApoderados.AnyAsync(a => a.ApoderadoId == ap.Id && a.AlumnoId == alumnoId && a.FechaFin == null);
                if (!has) return Forbid();
            }

            // Construir consulta de calificaciones para el alumno
            var q = _db.Calificaciones.AsNoTracking().Where(c => c.AlumnoId == alumnoId);
            if (periodoId is not null)
            {
                var periodo = await _db.Periodos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == periodoId.Value);
                if (periodo is not null && periodo.AnioLectivoId is not null)
                {
                    var anioId = periodo.AnioLectivoId.Value;
                    q = from c in _db.Calificaciones.AsNoTracking()
                        join cur in _db.Curriculas on c.CurriculaId equals cur.Id
                        join dndc in _db.DocentesNivelDetalleCurso on cur.DocenteNivelDetalleCursoId equals dndc.Id
                        join ndc in _db.NivelesDetalleCurso on dndc.NivelDetalleCursoId equals ndc.Id
                        join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                        join m in _db.Matriculas on new { c.AlumnoId, Id = nd.Id, AnioLectivoId = (int?)anioId } equals new { m.AlumnoId, Id = m.NivelDetalleId, AnioLectivoId = m.AnioLectivoId }
                        where c.AlumnoId == alumnoId
                        select c;
                }
                else
                {
                    // periodo not found or not linked to anio lectivo -> no results
                    q = _db.Calificaciones.Where(c => false);
                }
            }

            var list = await q.OrderByDescending(c => c.FechaRegistro)
                .Select(c => new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo, null, null, null, null))
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<CalificacionReadDto>>.Success(list));
        }
    }
}
