using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;

namespace SIAE_LA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class CursosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CursosController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginationResult<CursoReadDto>>>> GetAll([FromQuery] QueryParams q)
        {
            var query = _db.Cursos.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                query = query.Where(c => c.Descripcion.Contains(s) || (c.Codigo ?? "").Contains(s));
            }
            var total = await query.CountAsync();
            var items = await query.OrderBy(c => c.Descripcion).Skip(q.Skip).Take(q.Take)
            .Select(c => new CursoReadDto(c.Id, c.Descripcion, c.Codigo, c.Activo, null, null, null, null))
            .ToListAsync();
            var page = new PaginationResult<CursoReadDto> { Page = q.Page, PageSize = q.PageSize, TotalItems = total, Items = items };
            return Ok(ApiResponse<PaginationResult<CursoReadDto>>.Success(page));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CursoReadDto>>> GetOne(int id)
        {
            var c = await _db.Cursos.FindAsync(id);
            if (c is null) return NotFound(ApiResponse<CursoReadDto>.Fail("Curso no encontrado"));
            return Ok(ApiResponse<CursoReadDto>.Success(new CursoReadDto(c.Id, c.Descripcion, c.Codigo, c.Activo, null, null, null, null)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<CursoReadDto>>> Create([FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<CursoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var exists = await _db.Cursos.AnyAsync(c => c.Descripcion == dto.Descripcion);
            if (exists) return Conflict(ApiResponse<CursoReadDto>.Fail("Ya existe un curso con la misma descripción."));
            var e = new Domain.Entities.Curso { Descripcion = dto.Descripcion, Codigo = dto.Codigo, Activo = true };
            _db.Cursos.Add(e);
            await _db.SaveChangesAsync();
            var ai = SIAE_LA.Utils.AuditHelper.FromEntry(_db, e);
            var read = new CursoReadDto(e.Id, e.Descripcion, e.Codigo, e.Activo, ai.CreadoPor, ai.ModificadoPor, ai.FechaModificacion, ai.FechaIngreso);
            return CreatedAtAction(nameof(GetOne), new { id = e.Id }, ApiResponse<CursoReadDto>.Success(read, "Curso creado"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<CursoReadDto>>> Update(int id, [FromBody] CursoUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<CursoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var e = await _db.Cursos.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<CursoReadDto>.Fail("Curso no encontrado"));
            e.Descripcion = dto.Descripcion; e.Codigo = dto.Codigo; e.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            var ai2 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, e);
            return Ok(ApiResponse<CursoReadDto>.Success(new(e.Id, e.Descripcion, e.Codigo, e.Activo, ai2.CreadoPor, ai2.ModificadoPor, ai2.FechaModificacion, ai2.FechaIngreso), "Curso actualizado"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var e = await _db.Cursos.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<string>.Fail("Curso no encontrado"));
            e.Activo = false; // soft delete
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Curso desactivado"));
        }
    }
}
