#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SIAE_LA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class PeriodosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public PeriodosController(ApplicationDbContext db) => _db = db;


        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PeriodoReadDto>>>> Get()
        {
            var items = await _db.Periodos.AsNoTracking().OrderByDescending(p => p.Id)
            .Select(p => new PeriodoReadDto(p.Id, p.Descripcion, p.Activo, null, null, null, null)).ToListAsync();
            return Ok(ApiResponse<IEnumerable<PeriodoReadDto>>.Success(items));
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<PeriodoReadDto>>> Create(PeriodoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<PeriodoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var e = new Domain.Entities.Periodo { Descripcion = dto.Descripcion, Activo = true };
            _db.Periodos.Add(e);
            await _db.SaveChangesAsync();
            var ai = SIAE_LA.Utils.AuditHelper.FromEntry(_db, e);
            return CreatedAtAction(nameof(Get), new { id = e.Id }, ApiResponse<PeriodoReadDto>.Success(new(e.Id, e.Descripcion, e.Activo, ai.CreadoPor, ai.ModificadoPor, ai.FechaModificacion, ai.FechaIngreso), "Período creado"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<PeriodoReadDto>>> Update(int id, [FromBody] PeriodoUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<PeriodoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var e = await _db.Periodos.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<PeriodoReadDto>.Fail("Período no encontrado"));
            e.Descripcion = dto.Descripcion;
            e.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            var ai2 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, e);
            return Ok(ApiResponse<PeriodoReadDto>.Success(new(e.Id, e.Descripcion, e.Activo, ai2.CreadoPor, ai2.ModificadoPor, ai2.FechaModificacion, ai2.FechaIngreso), "Período actualizado"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var e = await _db.Periodos.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<string>.Fail("Período no encontrado"));
            e.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Período desactivado"));
        }
    }
}
