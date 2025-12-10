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
    public sealed class NivelesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public NivelesController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<NivelReadDto>>>> GetAll()
        {
            var items = await _db.Niveles.AsNoTracking()
                .OrderBy(n => n.DescripcionNivel)
                .Select(n => new NivelReadDto(n.Id, n.DescripcionNivel, n.DescripcionTurno, n.Horario, n.Activo))
                .ToListAsync();
            return Ok(ApiResponse<IEnumerable<NivelReadDto>>.Success(items));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<NivelReadDto>>> GetOne(int id)
        {
            var n = await _db.Niveles.FindAsync(id);
            if (n is null) return NotFound(ApiResponse<NivelReadDto>.Fail("Nivel no encontrado"));
            return Ok(ApiResponse<NivelReadDto>.Success(new NivelReadDto(n.Id, n.DescripcionNivel, n.DescripcionTurno, n.Horario, n.Activo)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<NivelReadDto>>> Create([FromBody] NivelCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<NivelReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var exists = await _db.Niveles.AnyAsync(n => n.DescripcionNivel == dto.DescripcionNivel);
            if (exists) return Conflict(ApiResponse<NivelReadDto>.Fail("Ya existe un nivel con la misma descripción."));
            var e = new Nivel { DescripcionNivel = dto.DescripcionNivel, DescripcionTurno = dto.DescripcionTurno, Horario = dto.Horario, Activo = true };
            _db.Niveles.Add(e);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = e.Id }, ApiResponse<NivelReadDto>.Success(new NivelReadDto(e.Id, e.DescripcionNivel, e.DescripcionTurno, e.Horario, e.Activo), "Nivel creado"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<NivelReadDto>>> Update(int id, [FromBody] NivelUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<NivelReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));
            var e = await _db.Niveles.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<NivelReadDto>.Fail("Nivel no encontrado"));
            e.DescripcionNivel = dto.DescripcionNivel;
            e.DescripcionTurno = dto.DescripcionTurno;
            e.Horario = dto.Horario;
            e.Activo = dto.Activo;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<NivelReadDto>.Success(new NivelReadDto(e.Id, e.DescripcionNivel, e.DescripcionTurno, e.Horario, e.Activo), "Nivel actualizado"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var e = await _db.Niveles.FindAsync(id);
            if (e is null) return NotFound(ApiResponse<string>.Fail("Nivel no encontrado"));
            e.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Nivel desactivado"));
        }

        // ListarGradosxNivel
        [HttpGet("{nivelId:int}/grados")]
        public async Task<ActionResult<ApiResponse<IEnumerable<GradoSeccionDto>>>> ListarGradosxNivel(int nivelId)
        {
            var q = from nd in _db.NivelesDetalle.AsNoTracking()
                    where nd.NivelId == nivelId && nd.Activo
                    join gs in _db.GradoSecciones on nd.GradoSeccionId equals gs.Id
                    where gs.Activo
                    select new GradoSeccionDto(gs.Id, gs.DescripcionGrado, gs.DescripcionSeccion);

            var list = await q.Distinct().ToListAsync();
            return Ok(ApiResponse<IEnumerable<GradoSeccionDto>>.Success(list));
        }

        /// <summary>
        /// Asignar un curso a un nivelDetalle (crear NivelDetalleCurso)
        /// Roles: Admin, Direccion, Subdireccion, JefeArea
        /// </summary>
        [HttpPost("{nivelId:int}/cursos")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<NivelDetalleCursoReadDto>>> AddCursoToNivel(int nivelId, [FromBody] NivelDetalleCursoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ApiResponse<NivelDetalleCursoReadDto>.Fail(SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));

            // Validar nivelDetalle existe y pertenece al nivelId
            var nivelDetalle = await _db.NivelesDetalle.FindAsync(dto.NivelDetalleId);
            if (nivelDetalle is null) return NotFound(ApiResponse<NivelDetalleCursoReadDto>.Fail("NivelDetalle no encontrado"));
            if (nivelDetalle.NivelId != nivelId) return BadRequest(ApiResponse<NivelDetalleCursoReadDto>.Fail("NivelDetalle no pertenece al nivel especificado"));

            // Validar curso existe
            var curso = await _db.Cursos.FindAsync(dto.CursoId);
            if (curso is null) return NotFound(ApiResponse<NivelDetalleCursoReadDto>.Fail("Curso no encontrado"));

            // Evitar duplicados: nivelDetalle + curso
            var exists = await _db.NivelesDetalleCurso.AnyAsync(ndc => ndc.NivelDetalleId == dto.NivelDetalleId && ndc.CursoId == dto.CursoId);
            if (exists) return Conflict(ApiResponse<NivelDetalleCursoReadDto>.Fail("El curso ya está asignado a este nivelDetalle"));

            var ndc = new Domain.Entities.NivelDetalleCurso
            {
                NivelDetalleId = dto.NivelDetalleId,
                CursoId = dto.CursoId,
                Activo = dto.Activo
            };
            _db.NivelesDetalleCurso.Add(ndc);
            await _db.SaveChangesAsync();

            var read = new NivelDetalleCursoReadDto(ndc.Id, ndc.NivelDetalleId, ndc.CursoId, ndc.Activo, ndc.FechaRegistro);
            return CreatedAtAction(nameof(GetNivelDetalleCurso), new { id = ndc.Id }, ApiResponse<NivelDetalleCursoReadDto>.Success(read, "Curso asignado al nivel"));
        }

        [HttpGet("cursos")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<IEnumerable<NivelDetalleCursoReadDto>>>> GetAllCursosNivel()
        {
            var list = await _db.NivelesDetalleCurso.ToListAsync();

            var read = list.Select(ndc =>
                new NivelDetalleCursoReadDto(ndc.Id, ndc.NivelDetalleId, ndc.CursoId, ndc.Activo, ndc.FechaRegistro)
            );

            return Ok(ApiResponse<IEnumerable<NivelDetalleCursoReadDto>>.Success(read));
        }


        /// <summary>
        /// Obtener una asignación NivelDetalleCurso por id
        /// </summary>
        [HttpGet("cursos/{id:int}")]
        public async Task<ActionResult<ApiResponse<NivelDetalleCursoReadDto>>> GetNivelDetalleCurso(int id)
        {
            var ndc = await _db.NivelesDetalleCurso.FindAsync(id);
            if (ndc is null) return NotFound(ApiResponse<NivelDetalleCursoReadDto>.Fail("NivelDetalleCurso no encontrado"));
            var read = new NivelDetalleCursoReadDto(ndc.Id, ndc.NivelDetalleId, ndc.CursoId, ndc.Activo, ndc.FechaRegistro);
            return Ok(ApiResponse<NivelDetalleCursoReadDto>.Success(read));
        }

        /// <summary>
        /// Desasignar (soft-delete) un curso de un nivelDetalle
        /// Roles: Admin, Direccion, Subdireccion, JefeArea
        /// </summary>
        [HttpDelete("cursos/{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<string>>> RemoveNivelDetalleCurso(int id)
        {
            var ndc = await _db.NivelesDetalleCurso.FindAsync(id);
            if (ndc is null) return NotFound(ApiResponse<string>.Fail("Asignación no encontrada"));
            ndc.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Asignación desactivada"));
        }


        // GET: api/niveles/detalle?nivelId=1 (nivelId opcional)
        [HttpGet("detalle")]
        public async Task<ActionResult<ApiResponse<IEnumerable<NivelDetalleResumenDto>>>> GetNivelesDetalle(
            [FromQuery] int? nivelId = null)
        {
            var q = _db.NivelesDetalle
                .AsNoTracking()
                .Include(nd => nd.Nivel)
                .Include(nd => nd.GradoSeccion)
                .Where(nd => nd.Activo);

            if (nivelId.HasValue)
                q = q.Where(nd => nd.NivelId == nivelId.Value);

            var list = await q
                .OrderBy(nd => nd.Nivel.DescripcionNivel)
                .ThenBy(nd => nd.GradoSeccion.DescripcionGrado)
                .ThenBy(nd => nd.GradoSeccion.DescripcionSeccion)
                .Select(nd => new NivelDetalleResumenDto
                {
                    NivelDetalleId = nd.Id,
                    NivelId = nd.NivelId,
                    NivelDescripcion = nd.Nivel.DescripcionNivel,
                    Turno = nd.Nivel.DescripcionTurno,
                    GradoSeccionId = nd.GradoSeccionId,
                    GradoDescripcion = nd.GradoSeccion.DescripcionGrado,
                    SeccionDescripcion = nd.GradoSeccion.DescripcionSeccion
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<NivelDetalleResumenDto>>.Success(list));
        }

        // POST: api/niveles/detalle
        // Crea un registro en nivel_detalle (Nivel + GradoSeccion)
        [HttpPost("detalle")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<NivelDetalleResumenDto>>> CreateNivelDetalle(
            [FromBody] NivelDetalleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<NivelDetalleResumenDto>.Fail(
                    SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));

            // Validar que exista el nivel
            var nivel = await _db.Niveles.FindAsync(dto.NivelId);
            if (nivel is null)
                return NotFound(ApiResponse<NivelDetalleResumenDto>.Fail("Nivel no encontrado"));

            // Validar que exista el grado/sección
            var grado = await _db.GradoSecciones.FindAsync(dto.GradoSeccionId);
            if (grado is null)
                return NotFound(ApiResponse<NivelDetalleResumenDto>.Fail("Grado/Sección no encontrado"));

            // Evitar duplicados (mismo nivel + mismo grado/sección activos)
            var exists = await _db.NivelesDetalle
                .AnyAsync(nd => nd.NivelId == dto.NivelId
                                && nd.GradoSeccionId == dto.GradoSeccionId
                                && nd.Activo);
            if (exists)
                return Conflict(ApiResponse<NivelDetalleResumenDto>.Fail(
                    "Ya existe un NivelDetalle para ese Nivel y Grado/Sección."));

            // Crear entidad
            var nd = new NivelDetalle
            {
                NivelId = dto.NivelId,
                GradoSeccionId = dto.GradoSeccionId,
                TotalVacantes = dto.TotalVacantes,   
                VacantesOcupadas = 0,
                Activo = true
            };

            _db.NivelesDetalle.Add(nd);
            await _db.SaveChangesAsync();

            // Cargar con includes para armar el resumen
            var ndLoaded = await _db.NivelesDetalle
                .AsNoTracking()
                .Include(x => x.Nivel)
                .Include(x => x.GradoSeccion)
                .FirstOrDefaultAsync(x => x.Id == nd.Id);

            if (ndLoaded is null)
                return NotFound(ApiResponse<NivelDetalleResumenDto>.Fail("Error al cargar NivelDetalle creado."));

            var resumen = new NivelDetalleResumenDto
            {
                NivelDetalleId = ndLoaded.Id,
                NivelId = ndLoaded.NivelId,
                NivelDescripcion = ndLoaded.Nivel.DescripcionNivel,
                Turno = ndLoaded.Nivel.DescripcionTurno,
                GradoSeccionId = ndLoaded.GradoSeccionId,
                GradoDescripcion = ndLoaded.GradoSeccion.DescripcionGrado,
                SeccionDescripcion = ndLoaded.GradoSeccion.DescripcionSeccion
            };

            return CreatedAtAction(
                nameof(GetNivelesDetalle),
                new { nivelId = ndLoaded.NivelId },
                ApiResponse<NivelDetalleResumenDto>.Success(resumen, "NivelDetalle creado")
            );
        }




    }
}
