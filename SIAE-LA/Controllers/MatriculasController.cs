using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.Domain.Entities;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;

#nullable enable

namespace SIAE_LA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class MatriculasController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public MatriculasController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MatriculaReadDto>>> Matricular([FromBody] MatriculaCreateDto dto)
        {
            if (dto is null) return BadRequest(ApiResponse<MatriculaReadDto>.Fail("Payload requerido"));

            // Validación de duplicado: Alumno + NivelDetalle + AnioLectivo
            var exists = await _db.Matriculas.AnyAsync(m => m.AlumnoId == dto.AlumnoId && m.NivelDetalleId == dto.NivelDetalleId && m.AnioLectivoId == dto.AnioLectivoId);
            if (exists) return Conflict(ApiResponse<MatriculaReadDto>.Fail("El alumno ya está matriculado en ese nivel para el año lectivo."));

            // Validar existencia de AnioLectivo
            var anio = await _db.AniosLectivos.FindAsync(dto.AnioLectivoId);
            if (anio is null) return NotFound(ApiResponse<MatriculaReadDto>.Fail("Año lectivo no encontrado"));

            var entity = new Matricula
            {
                AlumnoId = dto.AlumnoId,
                NivelDetalleId = dto.NivelDetalleId,
                AnioLectivoId = dto.AnioLectivoId,
                ApoderadoId = dto.ApoderadoId,
                Situacion = dto.Situacion,
                InstitucionProcedencia = dto.InstitucionProcedencia,
                EsRepitente = dto.EsRepitente,
                Activo = true
            };
            _db.Matriculas.Add(entity);
            await _db.SaveChangesAsync();

            var ai = SIAE_LA.Utils.AuditHelper.FromEntry(_db, entity);
            var read = new MatriculaReadDto(entity.Id, entity.AlumnoId, entity.NivelDetalleId, entity.AnioLectivoId, entity.ApoderadoId, entity.Situacion, entity.InstitucionProcedencia, entity.EsRepitente, entity.FechaRegistro, ai.CreadoPor, ai.ModificadoPor, ai.FechaModificacion, ai.FechaIngreso);
            return Ok(ApiResponse<MatriculaReadDto>.Success(read, "Matrícula creada"));
        }

        /// <summary>
        /// Devuelve las matrículas de un alumno. Ahora incluye información completa del NivelDetalle
        /// (Nivel y GradoSeccion) en lugar de solo el Id.
        /// </summary>
        [HttpGet("by-alumno/{alumnoId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MatriculaWithDetalleDto>>>> ByAlumno(int alumnoId, [FromQuery] int? periodoId, [FromQuery] int? anioLectivoId)
        {
            // construir query base
            IQueryable<Matricula> q = _db.Matriculas.AsNoTracking()
                .Where(m => m.AlumnoId == alumnoId)
                .Include(m => m.Alumno).ThenInclude(a => a.Persona)
                .Include(m => m.NivelDetalle).ThenInclude(nd => nd.Nivel)
                .Include(m => m.NivelDetalle).ThenInclude(nd => nd.GradoSeccion)
                .Include(m => m.Apoderado).ThenInclude(ap => ap!.Persona);

            int? effectiveAnioId = anioLectivoId;
            if (effectiveAnioId is null && periodoId is not null)
            {
                var periodo = await _db.Periodos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == periodoId.Value);
                if (periodo is not null) effectiveAnioId = periodo.AnioLectivoId;
            }
            if (effectiveAnioId is not null)
            {
                q = q.Where(m => m.AnioLectivoId == effectiveAnioId);
            }

            var entities = await q.OrderByDescending(m => m.FechaRegistro).ToListAsync();

            // precargar primeros periodos activos por año lectivo para mostrar en la respuesta
            var anioIds = entities.Where(x => x.AnioLectivoId.HasValue).Select(x => x.AnioLectivoId!.Value).Distinct().ToList();
            var periodos = await _db.Periodos.AsNoTracking()
                .Where(p => p.AnioLectivoId != null && anioIds.Contains(p.AnioLectivoId.Value) && p.Activo)
                .OrderBy(p => p.Orden)
                .ToListAsync();
            var firstPorAnio = periodos.GroupBy(p => p.AnioLectivoId).ToDictionary(g => g.Key!.Value, g => g.First());

            var items = entities.Select(m => {
                PeriodoReadDto? periodoDto = m.AnioLectivoId.HasValue && firstPorAnio.TryGetValue(m.AnioLectivoId.Value, out var fp)
                    ? new PeriodoReadDto(fp.Id, fp.Descripcion, fp.Activo, null, null, null, null)
                    : null;

                return new MatriculaWithDetalleDto(
                    m.Id,
                    new AlumnoReadDto(
                        m.Alumno!.Id, // non-null by design: Alumno always has Persona
                        m.Alumno!.Persona!.Nombres, // non-null by design
                        m.Alumno!.Persona!.Apellidos, // non-null by design
                        string.Empty,
                        m.Alumno!.Persona!.DocumentoIdentidad, // non-null by design
                        m.Alumno!.Persona!.Ciudad, // non-null by design
                        m.Alumno!.Persona!.Direccion, // non-null by design
                        m.Alumno!.Activo
                    ),
                    new NivelDetalleResumenDto(
                        m.NivelDetalle!.Id, // non-null by design: Matricula must reference NivelDetalle
                        m.NivelDetalle.NivelId,
                        new NivelDto(
                            m.NivelDetalle.Nivel!.Id, // non-null by design
                            m.NivelDetalle.Nivel.DescripcionNivel, // non-null by design
                            m.NivelDetalle.Nivel.DescripcionTurno, // non-null by design
                            m.NivelDetalle.Nivel.Horario // non-null by design
                        ),
                        m.NivelDetalle.GradoSeccionId,
                        new GradoSeccionDto(
                            m.NivelDetalle.GradoSeccion!.Id, // non-null by design
                            m.NivelDetalle.GradoSeccion.DescripcionGrado, // non-null by design
                            m.NivelDetalle.GradoSeccion.DescripcionSeccion, // non-null by design
                            m.Activo,
                            m.FechaRegistro
                        ),
                        m.NivelDetalle.TotalVacantes,
                        m.NivelDetalle.VacantesOcupadas
                    ),
                    periodoDto,
                    m.Apoderado != null && m.Apoderado.Persona != null
                        ? new TutorDto(
                            m.Apoderado.Id,
                            m.Apoderado.PersonaId,
                            m.Apoderado.Persona!.Nombres, // non-null by design when Apoderado.Persona != null
                            m.Apoderado.Persona!.Apellidos, // non-null by design
                            m.Apoderado.Persona!.DocumentoIdentidad, // non-null by design
                            m.Apoderado.Persona!.Email, // non-null by design
                            m.Apoderado.Persona!.NumeroTelefono // non-null by design
                        )
                        : null,
                    m.Situacion,
                    m.InstitucionProcedencia,
                    m.EsRepitente,
                    m.Activo,
                    m.FechaRegistro
                );
            }).ToList();

            return Ok(ApiResponse<IEnumerable<MatriculaWithDetalleDto>>.Success(items));
        }

        [HttpGet("by-nivel-detalle/{nivelDetalleId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MatriculaReadDto>>>> ByNivelDetalle(int nivelDetalleId, [FromQuery] int? periodoId, [FromQuery] int? anioLectivoId)
        {
            var q = _db.Matriculas.AsNoTracking().Where(m => m.NivelDetalleId == nivelDetalleId);
            int? effectiveAnioId = anioLectivoId;
            if (effectiveAnioId is null && periodoId is not null)
            {
                var periodo = await _db.Periodos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == periodoId.Value);
                if (periodo is not null) effectiveAnioId = periodo.AnioLectivoId;
            }
            if (effectiveAnioId is not null)
            {
                q = q.Where(m => m.AnioLectivoId == effectiveAnioId);
            }

            var items = await q.OrderByDescending(m => m.FechaRegistro)
                .Select(m => new MatriculaReadDto(m.Id, m.AlumnoId, m.NivelDetalleId, m.AnioLectivoId, m.ApoderadoId, m.Situacion, m.InstitucionProcedencia, m.EsRepitente, m.FechaRegistro, null, null, null, null))
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<MatriculaReadDto>>.Success(items));
        }
    }
}
