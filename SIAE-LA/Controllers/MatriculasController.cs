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
            // Validación de duplicado: Alumno + NivelDetalle + Periodo
            var exists = await _db.Matriculas.AnyAsync(m => m.AlumnoId == dto.AlumnoId && m.NivelDetalleId == dto.NivelDetalleId && m.PeriodoId == dto.PeriodoId);
            if (exists) return Conflict(ApiResponse<MatriculaReadDto>.Fail("El alumno ya está matriculado en ese nivel y período."));

            var entity = new Matricula
            {
                AlumnoId = dto.AlumnoId,
                NivelDetalleId = dto.NivelDetalleId,
                PeriodoId = dto.PeriodoId,
                ApoderadoId = dto.ApoderadoId,
                Situacion = dto.Situacion,
                InstitucionProcedencia = dto.InstitucionProcedencia,
                EsRepitente = dto.EsRepitente,
                Activo = true
            };
            _db.Matriculas.Add(entity);
            await _db.SaveChangesAsync();

            var read = new MatriculaReadDto(entity.Id, entity.AlumnoId, entity.NivelDetalleId, entity.PeriodoId, entity.ApoderadoId, entity.Situacion, entity.InstitucionProcedencia, entity.EsRepitente, entity.FechaRegistro);
            return Ok(ApiResponse<MatriculaReadDto>.Success(read, "Matrícula creada"));
        }

        /// <summary>
        /// Devuelve las matrículas de un alumno. Ahora incluye información completa del NivelDetalle
        /// (Nivel y GradoSeccion) en lugar de solo el Id.
        /// </summary>
        [HttpGet("by-alumno/{alumnoId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MatriculaWithDetalleDto>>>> ByAlumno(int alumnoId, [FromQuery] int? periodoId)
        {
            var q = _db.Matriculas.AsNoTracking().Where(m => m.AlumnoId == alumnoId);
            if (periodoId is not null) q = q.Where(m => m.PeriodoId == periodoId);

            var items = await q
                .OrderByDescending(m => m.FechaRegistro)
                .Select(m => new MatriculaWithDetalleDto(
                    m.Id,
                    new AlumnoReadDto(
                        m.Alumno.Id,
                        m.Alumno.Persona.Nombres,
                        m.Alumno.Persona.Apellidos,
                        "",
                        m.Alumno.Persona.DocumentoIdentidad,
                        m.Alumno.Persona.Ciudad,
                        m.Alumno.Persona.Direccion,
                        m.Alumno.Activo
                    ),
                    new NivelDetalleDto(
                        m.NivelDetalle.Id,
                        m.NivelDetalle.NivelId,
                        new NivelDto(
                            m.NivelDetalle.Nivel.Id,
                            m.NivelDetalle.Nivel.DescripcionNivel,
                            m.NivelDetalle.Nivel.DescripcionTurno,
                            m.NivelDetalle.Nivel.Horario
                        ),
                        m.NivelDetalle.GradoSeccionId,
                        new GradoSeccionDto(
                            m.NivelDetalle.GradoSeccion.Id,
                            m.NivelDetalle.GradoSeccion.DescripcionGrado,
                            m.NivelDetalle.GradoSeccion.DescripcionSeccion
                        ),
                        m.NivelDetalle.TotalVacantes,
                        m.NivelDetalle.VacantesOcupadas
                    ),
                    new PeriodoReadDto(
                        m.Periodo.Id,
                        m.Periodo.Descripcion,
                        m.Periodo.Activo
                    ),
                    m.Apoderado != null && m.Apoderado.Persona != null
                        ? new TutorDto(
                            m.Apoderado.Id,
                            m.Apoderado.PersonaId,
                            m.Apoderado.Persona.Nombres,
                            m.Apoderado.Persona.Apellidos,
                            m.Apoderado.Persona.DocumentoIdentidad,
                            m.Apoderado.Persona.Email,
                            m.Apoderado.Persona.NumeroTelefono
                        )
                        : null,
                    m.Situacion,
                    m.InstitucionProcedencia,
                    m.EsRepitente,
                    m.Activo,
                    m.FechaRegistro
                ))
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<MatriculaWithDetalleDto>>.Success(items));
        }

        [HttpGet("by-nivel-detalle/{nivelDetalleId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MatriculaReadDto>>>> ByNivelDetalle(int nivelDetalleId, [FromQuery] int? periodoId)
        {
            var q = _db.Matriculas.AsNoTracking().Where(m => m.NivelDetalleId == nivelDetalleId);
            if (periodoId is not null) q = q.Where(m => m.PeriodoId == periodoId);
            var items = await q.OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MatriculaReadDto(m.Id, m.AlumnoId, m.NivelDetalleId, m.PeriodoId, m.ApoderadoId, m.Situacion, m.InstitucionProcedencia, m.EsRepitente, m.FechaRegistro))
            .ToListAsync();
            return Ok(ApiResponse<IEnumerable<MatriculaReadDto>>.Success(items));
        }
    }
}
