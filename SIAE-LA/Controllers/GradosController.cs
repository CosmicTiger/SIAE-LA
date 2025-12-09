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
    public sealed class GradosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public GradosController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GradoSeccionDto>>>> GetAll()
        {
            var items = await _db.GradoSecciones.AsNoTracking().Where(g => g.Activo)
                .Select(g => new GradoSeccionDto(g.Id, g.DescripcionGrado, g.DescripcionSeccion))
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<GradoSeccionDto>>.Success(items));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<GradoSeccionDto>>> GetOne(int id)
        {
            var g = await _db.GradoSecciones.FindAsync(id);
            if (g is null) return NotFound(ApiResponse<GradoSeccionDto>.Fail("Grado no encontrado"));
            return Ok(ApiResponse<GradoSeccionDto>.Success(new GradoSeccionDto(g.Id, g.DescripcionGrado, g.DescripcionSeccion)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<GradoSeccionDto>>> Create([FromBody] GradoSeccionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<GradoSeccionDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var exists = await _db.GradoSecciones.AnyAsync(g => g.DescripcionGrado == dto.DescripcionGrado && g.DescripcionSeccion == dto.DescripcionSeccion);
            if (exists) return Conflict(ApiResponse<GradoSeccionDto>.Fail("Ya existe un grado/sección con la misma descripción."));
            var e = new GradoSeccion { DescripcionGrado = dto.DescripcionGrado, DescripcionSeccion = dto.DescripcionSeccion, Activo = true };
            _db.GradoSecciones.Add(e);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = e.Id }, ApiResponse<GradoSeccionDto>.Success(new GradoSeccionDto(e.Id, e.DescripcionGrado, e.DescripcionSeccion), "Grado creado"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<GradoSeccionDto>>> Update(int id, [FromBody] GradoSeccionUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<GradoSeccionDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var e = await _db.GradoSecciones.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<GradoSeccionDto>.Fail("Grado no encontrado"));
            e.DescripcionGrado = dto.DescripcionGrado;
            e.DescripcionSeccion = dto.DescripcionSeccion;
            e.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<GradoSeccionDto>.Success(new GradoSeccionDto(e.Id, e.DescripcionGrado, e.DescripcionSeccion), "Grado actualizado"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var e = await _db.GradoSecciones.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<string>.Fail("Grado no encontrado"));
            e.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Grado desactivado"));
        }

        // CursosxNivelGrado
        [HttpGet("nivel/{nivelId:int}/grado/{gradoSeccionId:int}/cursos")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DTOs.CursoReadDto>>>> CursosxNivelGrado(int nivelId, int gradoSeccionId)
        {
            var q = from nd in _db.NivelesDetalle.AsNoTracking()
                    where nd.NivelId == nivelId && nd.GradoSeccionId == gradoSeccionId && nd.Activo
                    join ndc in _db.NivelesDetalleCurso on nd.Id equals ndc.NivelDetalleId
                    join c in _db.Cursos on ndc.CursoId equals c.Id
                    where ndc.Activo && c.Activo
                    select new DTOs.CursoReadDto(c.Id, c.Descripcion, c.Codigo, c.Activo);

            var list = await q.Distinct().ToListAsync();
            return Ok(ApiResponse<IEnumerable<DTOs.CursoReadDto>>.Success(list));
        }
    }
}
