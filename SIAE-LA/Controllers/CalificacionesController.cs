using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.Domain.Entities;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIAE_LA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class CalificacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CalificacionesController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CalificacionReadDto>>> Crear([FromBody] CalificacionCreateDto dto)
        {
            // Regla de unicidad: (CurriculaId, AlumnoId)
            var exists = await _db.Calificaciones.AnyAsync(c => c.CurriculaId == dto.CurriculaId && c.AlumnoId == dto.AlumnoId);
            if (exists) return Conflict(ApiResponse<CalificacionReadDto>.Fail("Ya existe una calificación para este alumno en esta currícula."));


            var entity = new Calificacion
            {
                CurriculaId = dto.CurriculaId,
                AlumnoId = dto.AlumnoId,
                Nota = dto.Nota,
                Activo = true
            };
            _db.Calificaciones.Add(entity);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<CalificacionReadDto>.Success(new(entity.Id, entity.CurriculaId, entity.AlumnoId, entity.Nota, entity.FechaRegistro, entity.Activo), "Calificación registrada"));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<CalificacionReadDto>>> Editar(int id, [FromBody] CalificacionUpdateDto dto)
        {
            var c = await _db.Calificaciones.FindAsync(id);
            if (c is null) return NotFound(ApiResponse<CalificacionReadDto>.Fail("No existe la calificación"));
            c.Nota = dto.Nota; c.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<CalificacionReadDto>.Success(new(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo), "Calificación actualizada"));
        }

        [HttpGet("by-alumno/{alumnoId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CalificacionReadDto>>>> PorAlumno(int alumnoId, [FromQuery] int? periodoId)
        {
            // Si envían periodoId, filtramos por la matrícula del alumno en ese periodo y el mismo NivelDetalle de la currícula
            var q = _db.Calificaciones.AsNoTracking().Where(c => c.AlumnoId == alumnoId);
            if (periodoId is not null)
            {
                q = from c in _db.Calificaciones.AsNoTracking()
                    join cur in _db.Curriculas on c.CurriculaId equals cur.Id
                    join dndc in _db.DocentesNivelDetalleCurso on cur.DocenteNivelDetalleCursoId equals dndc.Id
                    join ndc in _db.NivelesDetalleCurso on dndc.NivelDetalleCursoId equals ndc.Id
                    join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                    join m in _db.Matriculas on new { c.AlumnoId, nd.Id, PeriodoId = periodoId.Value } equals new { m.AlumnoId, Id = m.NivelDetalleId, m.PeriodoId }
                    where c.AlumnoId == alumnoId
                    select c;
            }
            var list = await q.OrderByDescending(c => c.FechaRegistro)
            .Select(c => new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo))
            .ToListAsync();
            return Ok(ApiResponse<IEnumerable<CalificacionReadDto>>.Success(list));
        }
    }
}
