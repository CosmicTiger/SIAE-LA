#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Direccion,Subdireccion,JefeArea")]
public sealed class AnioLectivoController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AnioLectivoController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AnioLectivoReadDto>>>> GetAll()
    {
        var items = await _db.AniosLectivos.AsNoTracking()
            .OrderByDescending(a => a.Anio)
            .Select(a => new AnioLectivoReadDto(a.Id, a.Anio, a.Descripcion, a.Activo, a.FechaInicio, a.FechaFin, a.FechaRegistro, null, null, null, null))
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<AnioLectivoReadDto>>.Success(items));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AnioLectivoReadDto>>> GetOne(int id)
    {
        var a = await _db.AniosLectivos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound(ApiResponse<AnioLectivoReadDto>.Fail("Año lectivo no encontrado"));
        var audit = SIAE_LA.Utils.AuditHelper.FromEntry(_db, a);
        var dto = new AnioLectivoReadDto(a.Id, a.Anio, a.Descripcion, a.Activo, a.FechaInicio, a.FechaFin, a.FechaRegistro, audit.CreadoPor, audit.ModificadoPor, audit.FechaModificacion, audit.FechaIngreso);
        return Ok(ApiResponse<AnioLectivoReadDto>.Success(dto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AnioLectivoReadDto>>> Create([FromBody] AnioLectivoCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<AnioLectivoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));

        if (await _db.AniosLectivos.AnyAsync(x => x.Anio == dto.Anio))
            return Conflict(ApiResponse<AnioLectivoReadDto>.Fail("Ya existe un año lectivo con ese año."));

        var entity = new AnioLectivo
        {
            Anio = dto.Anio,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? $"Año {dto.Anio}" : dto.Descripcion,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Activo = true
        };
        _db.AniosLectivos.Add(entity);
        await _db.SaveChangesAsync();

        var audit2 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, entity);
        var result = new AnioLectivoReadDto(entity.Id, entity.Anio, entity.Descripcion, entity.Activo, entity.FechaInicio, entity.FechaFin, entity.FechaRegistro, audit2.CreadoPor, audit2.ModificadoPor, audit2.FechaModificacion, audit2.FechaIngreso);
        return CreatedAtAction(nameof(GetOne), new { id = entity.Id }, ApiResponse<AnioLectivoReadDto>.Success(result, "Año lectivo creado"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AnioLectivoReadDto>>> Update(int id, [FromBody] AnioLectivoUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<AnioLectivoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
        var a = await _db.AniosLectivos.FirstOrDefaultAsync(x => x.Id == id);
        if (a is null) return NotFound(ApiResponse<AnioLectivoReadDto>.Fail("Año lectivo no encontrado"));

        // evitar duplicados de año
        if (a.Anio != dto.Anio && await _db.AniosLectivos.AnyAsync(x => x.Anio == dto.Anio))
            return Conflict(ApiResponse<AnioLectivoReadDto>.Fail("Otro año lectivo con el mismo año ya existe."));

        a.Anio = dto.Anio;
        a.Descripcion = dto.Descripcion;
        a.FechaInicio = dto.FechaInicio;
        a.FechaFin = dto.FechaFin;
        a.Activo = dto.Activo;

        await _db.SaveChangesAsync();
        var audit3 = SIAE_LA.Utils.AuditHelper.FromEntry(_db, a);
        var res = new AnioLectivoReadDto(a.Id, a.Anio, a.Descripcion, a.Activo, a.FechaInicio, a.FechaFin, a.FechaRegistro, audit3.CreadoPor, audit3.ModificadoPor, audit3.FechaModificacion, audit3.FechaIngreso);
        return Ok(ApiResponse<AnioLectivoReadDto>.Success(res, "Año lectivo actualizado"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
    {
        var a = await _db.AniosLectivos.FindAsync(id);
        if (a is null) return NotFound(ApiResponse<string>.Fail("Año lectivo no encontrado"));
        a.Activo = false;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Success("OK", "Año lectivo desactivado"));
    }

    // ---------------- Periodos dentro del Año Lectivo ----------------
    [HttpGet("{id:int}/periodos")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PeriodoReadDto>>>> GetPeriodos(int id)
    {
        var exists = await _db.AniosLectivos.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound(ApiResponse<IEnumerable<PeriodoReadDto>>.Fail("Año lectivo no encontrado"));
        var list = await _db.Periodos.AsNoTracking().Where(p => p.AnioLectivoId == id)
            .OrderBy(p => p.Id)
            .Select(p => new PeriodoReadDto(p.Id, p.Descripcion, p.Activo, null, null, null, null))
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<PeriodoReadDto>>.Success(list));
    }

    [HttpPost("{id:int}/periodos")]
    public async Task<ActionResult<ApiResponse<PeriodoReadDto>>> CreatePeriodo(int id, [FromBody] PeriodoCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<PeriodoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
        var a = await _db.AniosLectivos.FindAsync(id);
        if (a is null) return NotFound(ApiResponse<PeriodoReadDto>.Fail("Año lectivo no encontrado"));
        // evitar duplicados de descripcion en el mismo año
        if (await _db.Periodos.AnyAsync(p2 => p2.AnioLectivoId == id && p2.Descripcion == dto.Descripcion))
            return Conflict(ApiResponse<PeriodoReadDto>.Fail("Ya existe un período con la misma descripción en este año lectivo."));

        // calcular orden: si viene en dto usarlo, sino max + 1
        var maxOrden = await _db.Periodos.Where(p2 => p2.AnioLectivoId == id).MaxAsync(p2 => (int?)p2.Orden) ?? 0;
        var orden = dto.Orden.HasValue && dto.Orden.Value > 0 ? dto.Orden.Value : (maxOrden + 1);
        // si orden inserta en medio, desplazar los existentes hacia abajo
        if (orden <= maxOrden)
        {
            var toShift = await _db.Periodos.Where(p2 => p2.AnioLectivoId == id && p2.Orden >= orden).ToListAsync();
            foreach (var s in toShift) s.Orden++;
        }

        var p = new Periodo { Descripcion = dto.Descripcion, Activo = true, FechaRegistro = DateTime.UtcNow, AnioLectivoId = id, Orden = orden };
        _db.Periodos.Add(p);
        await _db.SaveChangesAsync();

        var res = new PeriodoReadDto(p.Id, p.Descripcion, p.Activo);
        return CreatedAtAction(nameof(GetPeriodos), new { id = id }, ApiResponse<PeriodoReadDto>.Success(res, "Período creado"));
    }

    [HttpPut("{id:int}/periodos/{periodoId:int}")]
    public async Task<ActionResult<ApiResponse<PeriodoReadDto>>> UpdatePeriodo(int id, int periodoId, [FromBody] PeriodoUpdateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<PeriodoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
        var a = await _db.AniosLectivos.FindAsync(id);
        if (a is null) return NotFound(ApiResponse<PeriodoReadDto>.Fail("Año lectivo no encontrado"));
        var p = await _db.Periodos.FirstOrDefaultAsync(x => x.Id == periodoId && x.AnioLectivoId == id);
        if (p is null) return NotFound(ApiResponse<PeriodoReadDto>.Fail("Período no encontrado para el año lectivo"));
        // validar duplicado de descripcion en el mismo año
        if (p.Descripcion != dto.Descripcion && await _db.Periodos.AnyAsync(x => x.AnioLectivoId == id && x.Descripcion == dto.Descripcion))
            return Conflict(ApiResponse<PeriodoReadDto>.Fail("Ya existe un período con la misma descripción en este año lectivo."));

        // manejar cambio de orden si dto.Orden presente
        if (dto.Orden.HasValue && dto.Orden.Value > 0 && dto.Orden.Value != p.Orden)
        {
            var newOrder = dto.Orden.Value;
            var maxOrdenAll = await _db.Periodos.Where(px => px.AnioLectivoId == id).MaxAsync(px => (int?)px.Orden) ?? 0;
            if (newOrder > maxOrdenAll) newOrder = maxOrdenAll;

            if (newOrder < p.Orden)
            {
                // shift up others between newOrder..p.Orden-1
                var between = await _db.Periodos.Where(px => px.AnioLectivoId == id && px.Orden >= newOrder && px.Orden < p.Orden && px.Id != p.Id).ToListAsync();
                foreach (var b in between) b.Orden++;
            }
            else if (newOrder > p.Orden)
            {
                var between = await _db.Periodos.Where(px => px.AnioLectivoId == id && px.Orden <= newOrder && px.Orden > p.Orden && px.Id != p.Id).ToListAsync();
                foreach (var b in between) b.Orden--;
            }

            p.Orden = newOrder;
        }

        p.Descripcion = dto.Descripcion;
        p.Activo = dto.Activo;
        // permitir actualizar orden si dto contiene un Orden (extensión futura) - por ahora no expuesto
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<PeriodoReadDto>.Success(new PeriodoReadDto(p.Id, p.Descripcion, p.Activo), "Período actualizado"));
    }

    [HttpDelete("{id:int}/periodos/{periodoId:int}")]
    public async Task<ActionResult<ApiResponse<string>>> DeletePeriodo(int id, int periodoId)
    {
        var p = await _db.Periodos.FirstOrDefaultAsync(x => x.Id == periodoId && x.AnioLectivoId == id);
        if (p is null) return NotFound(ApiResponse<string>.Fail("Período no encontrado para el año lectivo"));
        p.Activo = false;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Success("OK", "Período desactivado"));
    }

    // Reordenar periodos: acepta lista de ids en el orden deseado
    [HttpPost("{id:int}/periodos/reorder")]
    public async Task<ActionResult<ApiResponse<string>>> ReorderPeriodos(int id, [FromBody] PeriodoReorderDto dto)
    {
        if (dto.PeriodoIds is null || dto.PeriodoIds.Length == 0) return BadRequest(ApiResponse<string>.Fail("Lista de periodos vacía"));
        var periodos = await _db.Periodos.Where(p => p.AnioLectivoId == id && dto.PeriodoIds.Contains(p.Id)).ToListAsync();
        if (periodos.Count != dto.PeriodoIds.Length) return BadRequest(ApiResponse<string>.Fail("Lista de periodos contiene ids inválidos o pertenecientes a otro año lectivo"));

        for (int i = 0; i < dto.PeriodoIds.Length; i++)
        {
            var p = periodos.First(x => x.Id == dto.PeriodoIds[i]);
            p.Orden = i + 1;
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Success("OK", "Periodos reordenados"));
    }
}
