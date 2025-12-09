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
    public sealed class VacantesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public VacantesController(ApplicationDbContext db) => _db = db;

        [HttpGet("cantidad/{nivelDetalleId:int}")]
        public async Task<ActionResult<ApiResponse<VacanteDto>>> ObtenerCantidadVacantes(int nivelDetalleId)
        {
            var nd = await _db.NivelesDetalle.FindAsync(nivelDetalleId);
            if (nd is null) return NotFound(ApiResponse<VacanteDto>.Fail("NivelDetalle no encontrado"));
            var dto = new VacanteDto(nd.Id, nd.TotalVacantes, nd.VacantesOcupadas);
            return Ok(ApiResponse<VacanteDto>.Success(dto));
        }

        [HttpPut("{nivelDetalleId:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<VacanteDto>>> UpdateVacantes(int nivelDetalleId, [FromBody] VacanteUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<VacanteDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var nd = await _db.NivelesDetalle.FindAsync(nivelDetalleId);
            if (nd is null) return NotFound(ApiResponse<VacanteDto>.Fail("NivelDetalle no encontrado"));
            nd.TotalVacantes = dto.TotalVacantes ?? nd.TotalVacantes;
            nd.VacantesOcupadas = dto.VacantesOcupadas ?? nd.VacantesOcupadas;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<VacanteDto>.Success(new VacanteDto(nd.Id, nd.TotalVacantes, nd.VacantesOcupadas), "Vacantes actualizadas"));
        }
    }
}
