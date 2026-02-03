using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NivelesController> _logger;

        public NivelesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILogger<NivelesController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

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
                    select new GradoSeccionDto(gs.Id, gs.DescripcionGrado, gs.DescripcionSeccion, gs.Activo, gs.FechaRegistro);

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
        public async Task<ActionResult<ApiResponse<IEnumerable<NivelDetalleDto>>>> GetNivelesDetalle(
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
                .Select(nd => new NivelDetalleDto
                {
                    NivelDetalleId = nd.Id,
                    NivelId = nd.NivelId,
                    NivelDescripcion = nd.Nivel.DescripcionNivel,
                    Turno = nd.Nivel.Horario ?? "N/A",
                    GradoSeccionId = nd.GradoSeccionId,
                    GradoDescripcion = nd.GradoSeccion.DescripcionGrado,
                    SeccionDescripcion = nd.GradoSeccion.DescripcionSeccion,
                    TotalVacantes = nd.TotalVacantes,
                    VacantesOcupadas = nd.VacantesOcupadas,
                    FechaRegistro = nd.FechaRegistro,
                    activo = nd.Activo
                })
                .ToListAsync();

            return Ok(ApiResponse<IEnumerable<NivelDetalleDto>>.Success(list));
        }

        // POST: api/niveles/detalle
        // Crea un registro en nivel_detalle (Nivel + GradoSeccion)
        [HttpPost("detalle")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<NivelDetalleDto>>> CreateNivelDetalle(
            [FromBody] NivelDetalleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<NivelDetalleDto>.Fail(
                    SIAE_LA.Utils.ModelStateHelper.BuildErrors(ModelState)));

            // Validar que exista el nivel
            var nivel = await _db.Niveles.FindAsync(dto.NivelId);
            if (nivel is null)
                return NotFound(ApiResponse<NivelDetalleDto>.Fail("Nivel no encontrado"));

            // Validar que exista el grado/sección
            var grado = await _db.GradoSecciones.FindAsync(dto.GradoSeccionId);
            if (grado is null)
                return NotFound(ApiResponse<NivelDetalleDto>.Fail("Grado/Sección no encontrado"));

            // Evitar duplicados (mismo nivel + mismo grado/sección activos)
            var exists = await _db.NivelesDetalle
                .AnyAsync(nd => nd.NivelId == dto.NivelId
                                && nd.GradoSeccionId == dto.GradoSeccionId
                                && nd.Activo);
            if (exists)
                return Conflict(ApiResponse<NivelDetalleDto>.Fail(
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
                return NotFound(ApiResponse<NivelDetalleDto>.Fail("Error al cargar NivelDetalle creado."));

            var resumen = new NivelDetalleDto
            {
                NivelDetalleId = ndLoaded.Id,
                NivelId = ndLoaded.NivelId,
                NivelDescripcion = ndLoaded.Nivel.DescripcionNivel,
                Turno = ndLoaded.Nivel.Horario ?? "N/A",
                GradoSeccionId = ndLoaded.GradoSeccionId,
                GradoDescripcion = ndLoaded.GradoSeccion.DescripcionGrado,
                SeccionDescripcion = ndLoaded.GradoSeccion.DescripcionSeccion,
                TotalVacantes = ndLoaded.TotalVacantes,
                VacantesOcupadas = ndLoaded.VacantesOcupadas,
                FechaRegistro = ndLoaded.FechaRegistro,
                activo = ndLoaded.Activo
            };

            return CreatedAtAction(
                nameof(GetNivelesDetalle),
                new { nivelId = ndLoaded.NivelId },
                ApiResponse<NivelDetalleDto>.Success(resumen, "NivelDetalle creado")
            );
        }

        /// <summary>
        /// Actualiza vacantes y estado (activo) de un NivelDetalle
        /// Roles: Admin, Direccion, Subdireccion, JefeArea
        /// </summary>
        [HttpPatch("detalle/{nivelDetalleId:int}/vacantes")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<NivelDetalleDto>>> UpdateVacantes(int nivelDetalleId, [FromBody] NivelDetalleVacantesUpdateDto dto)
        {
            var nd = await _db.NivelesDetalle.FindAsync(nivelDetalleId);
            if (nd is null) return NotFound(ApiResponse<NivelDetalleDto>.Fail("NivelDetalle no encontrado"));

            // Guardar valores antiguos para auditoría
            var oldTotal = nd.TotalVacantes;
            var oldOcupadas = nd.VacantesOcupadas;
            var oldActivo = nd.Activo;

            if (dto.TotalVacantes.HasValue)
                nd.TotalVacantes = dto.TotalVacantes.Value;

            if (dto.VacantesOcupadas.HasValue)
                nd.VacantesOcupadas = dto.VacantesOcupadas.Value;

            if (dto.Activo.HasValue)
                nd.Activo = dto.Activo.Value;

            // Validación: vacantes ocupadas no puede exceder total (si ambos están presentes)
            if (nd.TotalVacantes.HasValue && nd.VacantesOcupadas.HasValue && nd.VacantesOcupadas > nd.TotalVacantes)
            {
                return BadRequest(ApiResponse<NivelDetalleDto>.Fail("Vacantes ocupadas no puede ser mayor que el total de vacantes."));
            }

            await _db.SaveChangesAsync();

            // Auditoría: registrar el cambio en logs y preparar DTO de auditoría
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id ?? "-";
                var audit = new NivelDetalleVacantesAuditDto
                {
                    NivelDetalleId = nd.Id,
                    OldTotalVacantes = oldTotal,
                    NewTotalVacantes = nd.TotalVacantes,
                    OldVacantesOcupadas = oldOcupadas,
                    NewVacantesOcupadas = nd.VacantesOcupadas,
                    OldActivo = oldActivo,
                    NewActivo = nd.Activo,
                    ChangedByUserId = userId,
                    ChangedAt = DateTime.UtcNow
                };

                _logger.LogInformation("NivelDetalleVacantes updated: {@audit}", audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar auditoría de NivelDetalleVacantes para id {Id}", nd.Id);
            }

            // Retornar el DTO actualizado
            var loaded = await _db.NivelesDetalle.AsNoTracking().Include(x => x.Nivel).Include(x => x.GradoSeccion).FirstOrDefaultAsync(x => x.Id == nd.Id);
            if (loaded is null) return NotFound(ApiResponse<NivelDetalleDto>.Fail("Error al cargar NivelDetalle actualizado"));

            var resumen = new NivelDetalleDto
            {
                NivelDetalleId = loaded.Id,
                NivelId = loaded.NivelId,
                NivelDescripcion = loaded.Nivel.DescripcionNivel,
                Turno = loaded.Nivel.Horario ?? "N/A",
                GradoSeccionId = loaded.GradoSeccionId,
                GradoDescripcion = loaded.GradoSeccion.DescripcionGrado,
                SeccionDescripcion = loaded.GradoSeccion.DescripcionSeccion,
                TotalVacantes = loaded.TotalVacantes,
                VacantesOcupadas = loaded.VacantesOcupadas,
                FechaRegistro = loaded.FechaRegistro,
                activo = loaded.Activo
            };

            return Ok(ApiResponse<NivelDetalleDto>.Success(resumen, "NivelDetalle actualizado"));
        }




    }
}
