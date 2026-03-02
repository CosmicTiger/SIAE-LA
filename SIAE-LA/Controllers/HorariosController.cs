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
        public async Task<ActionResult<ApiResponse<IEnumerable<HorarioDto>>>> GetAll()
        {
            var items = await _db.Horarios.AsNoTracking()
                .Select(h => new HorarioDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro, null, null, null, null))
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<HorarioDto>>.Success(items));
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<HorarioDto>>> GetOne(int id)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h is null) return NotFound(ApiResponse<HorarioReadDto>.Fail("Horario no encontrado"));
            var ai = SIAE_LA.Utils.AuditHelper.FromEntry(_db, h);
            return Ok(ApiResponse<HorarioDto>.Success(new HorarioDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro, ai.CreadoPor, ai.ModificadoPor, ai.FechaModificacion, ai.FechaIngreso)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<HorarioDto>>> Create([FromBody] HorarioCreateInputDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<HorarioReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            // Validar que NivelDetalleCurso exista
            var ndc = await _db.NivelesDetalleCurso.FindAsync(dto.NivelDetalleCursoId);
            if (ndc is null) return BadRequest(ApiResponse<HorarioReadDto>.Fail("NivelDetalleCurso no existe"));

            var h = new Horario { NivelDetalleCursoId = dto.NivelDetalleCursoId, DiaSemana = dto.DiaSemana, HoraInicio = dto.HoraInicio, HoraFin = dto.HoraFin, Activo = true };
            _db.Horarios.Add(h);
            await _db.SaveChangesAsync();
            var ai2 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, h);
            return CreatedAtAction(nameof(GetOne), new { id = h.Id }, ApiResponse<HorarioDto>.Success(new HorarioDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro, ai2.CreadoPor, ai2.ModificadoPor, ai2.FechaModificacion, ai2.FechaIngreso), "Horario creado"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<HorarioDto>>> Update(int id, [FromBody] HorarioUpdateInputDto dto)
        {
            var h = await _db.Horarios.FindAsync(id);
            if (h is null) return NotFound(ApiResponse<HorarioReadDto>.Fail("Horario no encontrado"));
            h.DiaSemana = dto.DiaSemana;
            h.HoraInicio = dto.HoraInicio;
            h.HoraFin = dto.HoraFin;
            h.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            var ai3 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, h);
            return Ok(ApiResponse<HorarioDto>.Success(new HorarioDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro, ai3.CreadoPor, ai3.ModificadoPor, ai3.FechaModificacion, ai3.FechaIngreso), "Horario actualizado"));
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
        public async Task<ActionResult<ApiResponse<HorarioDto>>> AsignarHorario([FromBody] HorarioCreateInputDto dto)
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
