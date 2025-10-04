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
                EsRepetente = dto.EsRepetente,
                Activo = true
            };
            _db.Matriculas.Add(entity);
            await _db.SaveChangesAsync();


            var read = new MatriculaReadDto(entity.Id, entity.AlumnoId, entity.NivelDetalleId, entity.PeriodoId, entity.ApoderadoId, entity.Situacion, entity.InstitucionProcedencia, entity.EsRepetente, entity.FechaRegistro);
            return Ok(ApiResponse<MatriculaReadDto>.Success(read, "Matrícula creada"));
        }


        [HttpGet("by-alumno/{alumnoId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MatriculaReadDto>>>> ByAlumno(int alumnoId, [FromQuery] int? periodoId)
        {
            var q = _db.Matriculas.AsNoTracking().Where(m => m.AlumnoId == alumnoId);
            if (periodoId is not null) q = q.Where(m => m.PeriodoId == periodoId);
            var items = await q.OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MatriculaReadDto(m.Id, m.AlumnoId, m.NivelDetalleId, m.PeriodoId, m.ApoderadoId, m.Situacion, m.InstitucionProcedencia, m.EsRepetente, m.FechaRegistro))
            .ToListAsync();
            return Ok(ApiResponse<IEnumerable<MatriculaReadDto>>.Success(items));
        }


        [HttpGet("by-nivel-detalle/{nivelDetalleId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MatriculaReadDto>>>> ByNivelDetalle(int nivelDetalleId, [FromQuery] int? periodoId)
        {
            var q = _db.Matriculas.AsNoTracking().Where(m => m.NivelDetalleId == nivelDetalleId);
            if (periodoId is not null) q = q.Where(m => m.PeriodoId == periodoId);
            var items = await q.OrderByDescending(m => m.FechaRegistro)
            .Select(m => new MatriculaReadDto(m.Id, m.AlumnoId, m.NivelDetalleId, m.PeriodoId, m.ApoderadoId, m.Situacion, m.InstitucionProcedencia, m.EsRepetente, m.FechaRegistro))
            .ToListAsync();
            return Ok(ApiResponse<IEnumerable<MatriculaReadDto>>.Success(items));
        }
    }
}
