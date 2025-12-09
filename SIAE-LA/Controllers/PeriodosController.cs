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
            .Select(p => new PeriodoReadDto(p.Id, p.Descripcion, p.Activo)).ToListAsync();
            return Ok(ApiResponse<IEnumerable<PeriodoReadDto>>.Success(items));
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse<PeriodoReadDto>>> Create(PeriodoCreateDto dto)
        {
            var e = new Domain.Entities.Periodo { Descripcion = dto.Descripcion, Activo = true };
            _db.Periodos.Add(e);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<PeriodoReadDto>.Success(new(e.Id, e.Descripcion, e.Activo), "Período creado"));
        }
    }
}
