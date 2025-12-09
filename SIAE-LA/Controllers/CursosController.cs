#nullable enable
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
    public sealed class CursosController : Controller
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
            .Select(c => new CursoReadDto(c.Id, c.Descripcion, c.Codigo, c.Activo))
            .ToListAsync();
            var page = new PaginationResult<CursoReadDto> { Page = q.Page, PageSize = q.PageSize, TotalItems = total, Items = items };
            return Ok(ApiResponse<PaginationResult<CursoReadDto>>.Success(page));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CursoReadDto>>> Create([FromBody] CursoCreateDto dto)
        {
            var e = new Domain.Entities.Curso { Descripcion = dto.Descripcion, Codigo = dto.Codigo, Activo = true };
            _db.Cursos.Add(e);
            await _db.SaveChangesAsync();
            var read = new CursoReadDto(e.Id, e.Descripcion, e.Codigo, e.Activo);
            return CreatedAtAction(nameof(GetAll), new { id = e.Id }, ApiResponse<CursoReadDto>.Success(read, "Curso creado"));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<CursoReadDto>>> Update(int id, [FromBody] CursoUpdateDto dto)
        {
            var e = await _db.Cursos.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<CursoReadDto>.Fail("Curso no encontrado"));
            e.Descripcion = dto.Descripcion; e.Codigo = dto.Codigo; e.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<CursoReadDto>.Success(new(e.Id, e.Descripcion, e.Codigo, e.Activo), "Curso actualizado"));
        }

        [HttpDelete("{id:int}")]
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
