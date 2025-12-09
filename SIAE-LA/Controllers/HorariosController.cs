using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class HorariosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public HorariosController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<IEnumerable<HorarioReadDto>>>> GetAll()
        {
            var items = await _db.Horarios.AsNoTracking()
                .Select(h => new HorarioReadDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro))
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<HorarioReadDto>>.Success(items));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<HorarioReadDto>>> GetOne(int id)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h is null) return NotFound(ApiResponse<HorarioReadDto>.Fail("Horario no encontrado"));
            return Ok(ApiResponse<HorarioReadDto>.Success(new HorarioReadDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<HorarioReadDto>>> Create([FromBody] HorarioCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<HorarioReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            // Validar que NivelDetalleCurso exista
            var ndc = await _db.NivelesDetalleCurso.FindAsync(dto.NivelDetalleCursoId);
            if (ndc is null) return BadRequest(ApiResponse<HorarioReadDto>.Fail("NivelDetalleCurso no existe"));

            var h = new Horario { NivelDetalleCursoId = dto.NivelDetalleCursoId, DiaSemana = dto.DiaSemana, HoraInicio = dto.HoraInicio, HoraFin = dto.HoraFin, Activo = true };
            _db.Horarios.Add(h);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = h.Id }, ApiResponse<HorarioReadDto>.Success(new HorarioReadDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro), "Horario creado"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<HorarioReadDto>>> Update(int id, [FromBody] HorarioUpdateDto dto)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h is null) return NotFound(ApiResponse<HorarioReadDto>.Fail("Horario no encontrado"));
            h.DiaSemana = dto.DiaSemana;
            h.HoraInicio = dto.HoraInicio;
            h.HoraFin = dto.HoraFin;
            h.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<HorarioReadDto>.Success(new HorarioReadDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro), "Horario actualizado"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h is null) return NotFound(ApiResponse<string>.Fail("Horario no encontrado"));
            h.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Horario desactivado"));
        }

        // AsignarHorario (crea horario para nivel_detalle_curso)
        [HttpPost("asignar")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<HorarioReadDto>>> AsignarHorario([FromBody] HorarioCreateDto dto)
        {
            // reusar Create
            return await Create(dto);
        }

        // EliminarHorario (soft delete)
        [HttpPost("eliminar/{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> EliminarHorario(int id)
        {
            return await Delete(id);
        }
    }
}
