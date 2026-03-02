#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.DTOs;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Utils;

namespace SIAE_LA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class AlumnosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlumnosController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private static int CalcularEdad(DateTime fechaNacUtc)
        {
            var hoy = DateTime.UtcNow.Date;
            var edad = hoy.Year - fechaNacUtc.Date.Year;
            if (fechaNacUtc.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        // ======================================================================
        // GET: api/alumnos  (paginado + búsqueda avanzada + filtros + ordenamiento)
        //
        // Query parameters supported:
        //  - q.Search (desde QueryParams) --> texto genérico
        //  - name         : búsqueda por nombre del alumno (partial)
        //  - tutor        : búsqueda por nombre del tutor (partial)
        //  - doc          : documento identidad (exact or partial depending)
        //  - email        : email (partial)
        //  - phone        : teléfono (partial)
        //  - gradoId      : int -> filtrar por Grado/Sección (GradoSeccion.Id)
        //  - list         : "ACTIVE" (default) | "ALL" (incluye inactivos)
        //  - sortBy       : "grado" | "created" | "apellido" | "email" | "matricula"  (default: "apellido")
        //  - sortDir      : "asc" | "desc" (default: "asc")
        // ======================================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginationResult<AlumnoReadDto>>>> GetAll(
            [FromQuery] QueryParams q,
            [FromQuery] string? name = null,
            [FromQuery] string? tutor = null,
            [FromQuery] string? doc = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phone = null,
            [FromQuery] int? gradoId = null,
            [FromQuery] string? list = "ACTIVE",
            [FromQuery] string? sortBy = "apellido",
            [FromQuery] string? sortDir = "asc")
        {
            // Normalizar parámetros
            var listMode = (list ?? "ACTIVE").Trim().ToUpperInvariant();
            var sort = (sortBy ?? "apellido").Trim().ToLowerInvariant();
            var dir = (sortDir ?? "asc").Trim().ToLowerInvariant();
            if (dir != "asc" && dir != "desc") dir = "asc";

            // Base query (desde Alumnos con Persona)
            var query = _db.Alumnos
                .AsNoTracking()
                .Include(a => a.Persona)
                .AsQueryable();

            // Filtrar por activos por defecto
            if (listMode != "ALL")
            {
                query = query.Where(a => a.Activo);
            }

            // Aplicar filtros de búsqueda específicos (si se pasan)
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var s = q.Search.Trim();
                // mezcla de criterios: nombre/apellido o documento
                query = query.Where(a =>
                    EF.Functions.Like(a.Persona.Nombres + " " + a.Persona.Apellidos, $"%{s}%") ||
                    EF.Functions.Like(a.Persona.Apellidos + " " + a.Persona.Nombres, $"%{s}%") ||
                    EF.Functions.Like(a.Persona.DocumentoIdentidad ?? "", $"%{s}%") ||
                    EF.Functions.Like(a.Persona.Email ?? "", $"%{s}%"));
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var s = name.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Persona.Nombres + " " + a.Persona.Apellidos, $"%{s}%") ||
                    EF.Functions.Like(a.Persona.Apellidos + " " + a.Persona.Nombres, $"%{s}%"));
            }

            if (!string.IsNullOrWhiteSpace(tutor))
            {
                var s = tutor.Trim();
                // Buscar por cualquiera de las matrículas donde exista apoderado con persona coincidente
                query = query.Where(a =>
                    a.Matriculas.Any(m =>
                        m.Apoderado != null &&
                        EF.Functions.Like(m.Apoderado.Persona.Nombres + " " + m.Apoderado.Persona.Apellidos, $"%{s}%")));
            }

            if (!string.IsNullOrWhiteSpace(doc))
            {
                var s = doc.Trim();
                // Por documento hacemos igualdad si tiene guión largo (formato), si no usamos contains
                if (s.Contains("-"))
                    query = query.Where(a => a.Persona.DocumentoIdentidad == s);
                else
                    query = query.Where(a => EF.Functions.Like(a.Persona.DocumentoIdentidad ?? "", $"%{s}%"));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var s = email.Trim();
                query = query.Where(a => EF.Functions.Like(a.Persona.Email ?? "", $"%{s}%"));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var s = phone.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Persona.NumeroTelefono ?? "", $"%{s}%") ||
                    // también buscar en teléfono de apoderado
                    a.Matriculas.Any(m => m.Apoderado != null && EF.Functions.Like(m.Apoderado.Persona.NumeroTelefono ?? "", $"%{s}%"))
                );
            }

            if (gradoId.HasValue)
            {
                var gid = gradoId.Value;
                // Filtrar si alguna matrícula del alumno corresponde a ese GradoSeccion
                query = query.Where(a => a.Matriculas.Any(m => m.NivelDetalle.GradoSeccionId == gid));
            }

            // Contar total antes de paginar
            var total = await query.CountAsync();

            // Para ordenar por valores relacionados (grado, matrícula), proyectamos LatestMat dentro del query
            var withLatest = query.Select(a => new
            {
                Alumno = a,
                LatestMat = a.Matriculas.OrderByDescending(m => m.FechaRegistro).FirstOrDefault()
            });

            // Aplicar ordenamiento
            switch (sort)
            {
                case "grado":
                    if (dir == "desc")
                        withLatest = withLatest
                            .OrderByDescending(x => x.LatestMat != null ? x.LatestMat.NivelDetalle.Nivel.DescripcionNivel : null)
                            .ThenByDescending(x => x.LatestMat != null ? x.LatestMat.NivelDetalle.GradoSeccion.DescripcionGrado : null)
                            .ThenByDescending(x => x.Alumno.Persona.Apellidos).ThenByDescending(x => x.Alumno.Persona.Nombres);
                    else
                        withLatest = withLatest
                            .OrderBy(x => x.LatestMat != null ? x.LatestMat.NivelDetalle.Nivel.DescripcionNivel : null)
                            .ThenBy(x => x.LatestMat != null ? x.LatestMat.NivelDetalle.GradoSeccion.DescripcionGrado : null)
                            .ThenBy(x => x.Alumno.Persona.Apellidos).ThenBy(x => x.Alumno.Persona.Nombres);
                    break;

                case "created":
                    // Ordenar por fecha de creación del alumno (campo FechaRegistro en ALUMNO)
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.Alumno.FechaRegistro);
                    else
                        withLatest = withLatest.OrderBy(x => x.Alumno.FechaRegistro);
                    break;

                case "email":
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.Alumno.Persona.Email).ThenByDescending(x => x.Alumno.Persona.Apellidos);
                    else
                        withLatest = withLatest.OrderBy(x => x.Alumno.Persona.Email).ThenBy(x => x.Alumno.Persona.Apellidos);
                    break;

                case "matricula":
                    // Ordenar por fecha de matrícula más reciente
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.LatestMat != null ? x.LatestMat.FechaRegistro : DateTime.MinValue)
                                               .ThenByDescending(x => x.Alumno.Persona.Apellidos);
                    else
                        withLatest = withLatest.OrderBy(x => x.LatestMat != null ? x.LatestMat.FechaRegistro : DateTime.MinValue)
                                               .ThenBy(x => x.Alumno.Persona.Apellidos);
                    break;

                case "apellido":
                default:
                    if (dir == "desc")
                        withLatest = withLatest.OrderByDescending(x => x.Alumno.Persona.Apellidos).ThenByDescending(x => x.Alumno.Persona.Nombres);
                    else
                        withLatest = withLatest.OrderBy(x => x.Alumno.Persona.Apellidos).ThenBy(x => x.Alumno.Persona.Nombres);
                    break;
            }

            // Paginación
            var page = Math.Max(1, q.Page);
            var pageSize = Math.Max(1, q.PageSize);
            var skip = (page - 1) * pageSize;

            var listedItems = await withLatest
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new AlumnoReadDto(
                    x.Alumno.Id,
                    x.Alumno.Persona.Nombres,
                    x.Alumno.Persona.Apellidos,
                    /* Codigo */ null,
                    x.Alumno.Persona.DocumentoIdentidad,
                    x.Alumno.Persona.Ciudad,
                    x.Alumno.Persona.Direccion,
                    x.Alumno.Activo
                ))
                .ToListAsync();

            var result = new PaginationResult<AlumnoReadDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalItems = total,
                Items = listedItems
            };

            return Ok(ApiResponse<PaginationResult<AlumnoReadDto>>.Success(result));
        }

        // ======================================================================
        // GET: api/alumnos/{id}
        // ======================================================================
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<AlumnoDetailDto>>> GetOne(int id)
        {
            // Proyección en una sola consulta para evitar N+1
            var item = await _db.Alumnos
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    AlumnoId = a.Id,
                    a.Activo,
                    Persona = new
                    {
                        a.Persona.Nombres,
                        a.Persona.Apellidos,
                        a.Persona.DocumentoIdentidad,
                        // Read the raw database value as DateTime? to avoid Npgsql trying to deserialize
                        // into System.DateOnly when the underlying column is a timestamp with time zone.
                        FechaNacimiento = EF.Property<DateTime?>(a.Persona, "FechaNacimiento"), 
                        a.Persona.Sexo,
                        a.Persona.Ciudad,
                        a.Persona.Direccion,
                        a.Persona.Email,
                        a.Persona.NumeroTelefono
                    },
                    // Matrícula "actual" = la más reciente por FechaRegistro (si existe)
                    Matricula = a.Matriculas
                        .OrderByDescending(m => m.FechaRegistro)
                        .Select(m => new
                        {
                            MatriculaId = m.Id,
                            AnioLectivoId = m.AnioLectivoId,
                            m.Situacion,
                            m.EsRepitente,
                            m.ApoderadoId,
                            m.FechaRegistro,
                            NivelDetalle = new
                            {
                                NivelDetalleId = m.NivelDetalle.Id,
                                NivelId = m.NivelDetalle.NivelId,
                                NivelDescripcion = m.NivelDetalle.Nivel.DescripcionNivel,
                                NivelTurno = m.NivelDetalle.Nivel.DescripcionTurno,
                                GradoSeccionId = m.NivelDetalle.GradoSeccionId,
                                GradoDescripcion = m.NivelDetalle.GradoSeccion.DescripcionGrado,
                                SeccionDescripcion = m.NivelDetalle.GradoSeccion.DescripcionSeccion
                            }
                        })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (item is null)
                return NotFound(ApiResponse<AlumnoDetailDto>.Fail("Alumno no encontrado"));

            // item.Persona.FechaNacimiento now holds a DateTime? (raw DB value). PersonaDto expects DateTime?
            var personaDto = new PersonaDto(
                item.Persona.Nombres,
                item.Persona.Apellidos,
                item.Persona.DocumentoIdentidad,
                item.Persona.FechaNacimiento,
                item.Persona.Sexo,
                item.Persona.Ciudad,
                item.Persona.Direccion,
                item.Persona.Email,
                item.Persona.NumeroTelefono
            );

            MatriculaResumenDto? matriculaDto = null;
            TutorDto? tutorDto = null;

            if (item.Matricula is not null)
            {
                var nd = item.Matricula.NivelDetalle;
                var nivelResumen = new NivelResumenDto(
                    nd.NivelDetalleId,
                    nd.NivelId,
                    nd.NivelDescripcion,
                    nd.NivelTurno,
                    nd.GradoSeccionId,
                    nd.GradoDescripcion,
                    nd.SeccionDescripcion
                );

                matriculaDto = new MatriculaResumenDto(
                    item.Matricula.MatriculaId,
                    nivelResumen,
                    item.Matricula.AnioLectivoId,
                    item.Matricula.Situacion,
                    item.Matricula.EsRepitente,
                    item.Matricula.ApoderadoId,
                    item.Matricula.FechaRegistro
                );
            }

            // Load only the needed fields via projection to avoid materializing
            // the full Persona entity (which uses DateOnly) — this prevents Npgsql
            // attempting to read a timestamp as DateOnly.
            var activeAssignment = await _db.AlumnosApoderados
                .AsNoTracking()
                .Where(x => x.AlumnoId == item.AlumnoId && x.FechaFin == null)
                .Select(x => new
                {
                    ApoderadoId = x.Apoderado.Id,
                    Persona = new
                    {
                        Id = x.Apoderado.Persona.Id,
                        x.Apoderado.Persona.Nombres,
                        x.Apoderado.Persona.Apellidos,
                        x.Apoderado.Persona.DocumentoIdentidad,
                        x.Apoderado.Persona.Email,
                        x.Apoderado.Persona.NumeroTelefono
                    }
                })
                .FirstOrDefaultAsync();

            if (activeAssignment is not null)
            {
                var per = activeAssignment.Persona;
                tutorDto = new TutorDto(
                    activeAssignment.ApoderadoId,
                    per.Id,
                    per.Nombres,
                    per.Apellidos,
                    per.DocumentoIdentidad,
                    per.Email,
                    per.NumeroTelefono
                );
            }
            else if (matriculaDto is not null && matriculaDto.ApoderadoId is not null)
            {
                // Fallback: si no hay asignación activa, usar snapshot de la matrícula (legacy)
                var ap = await _db.Apoderados
                    .AsNoTracking()
                    .Where(x => x.Id == matriculaDto.ApoderadoId.Value)
                    .Select(x => new
                    {
                        x.Id,
                        x.PersonaId,
                        Persona = new
                        {
                            x.Persona.Nombres,
                            x.Persona.Apellidos,
                            x.Persona.DocumentoIdentidad,
                            x.Persona.Email,
                            x.Persona.NumeroTelefono
                        }
                    })
                    .FirstOrDefaultAsync();

                if (ap is not null)
                {
                    tutorDto = new TutorDto(
                        ap.Id,
                        ap.PersonaId,
                        ap.Persona.Nombres,
                        ap.Persona.Apellidos,
                        ap.Persona.DocumentoIdentidad,
                        ap.Persona.Email,
                        ap.Persona.NumeroTelefono
                    );
                }
            }

            var dto = new AlumnoDetailDto(
                item.AlumnoId,
                personaDto,
                matriculaDto,
                tutorDto,
                item.Activo
            );

            return Ok(ApiResponse<AlumnoDetailDto>.Success(dto));
        }

        /// <summary>
        /// Devuelve las notas del alumno que realiza la petición (Estudiante) o del alumno indicado (Admin/Docente/Tutor según permisos).
        /// Roles: Admin, Direccion, Subdireccion, JefeArea, Docente, Estudiante, Tutor
        /// </summary>
        [HttpGet("me/notas")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CalificacionReadDto>>>> MyNotas([FromQuery] int? periodoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            int? alumnoId = null;
            if (User.IsInRole("Estudiante"))
            {
                if (user.PersonaId is null) return Forbid();
                var a = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(x => x.PersonaId == user.PersonaId);
                if (a is null) return NotFound(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail("Alumno no encontrado para el usuario"));
                alumnoId = a.Id;
            }
            else if (User.IsInRole("Tutor"))
            {
                // Para tutor devolvemos notas de todos sus pupilos actuales
                if (user.PersonaId is null) return Forbid();
                var ap = await _db.Apoderados.AsNoTracking().FirstOrDefaultAsync(x => x.PersonaId == user.PersonaId);
                if (ap is null) return Forbid();
                var pupils = await _db.AlumnosApoderados.AsNoTracking().Where(x => x.ApoderadoId == ap.Id && x.FechaFin == null).Select(x => x.AlumnoId).ToListAsync();
                var q = _db.Calificaciones.AsNoTracking().Where(c => pupils.Contains(c.AlumnoId));
                if (periodoId is not null)
                {
                    var pid = periodoId.Value;
                    var periodo = await _db.Periodos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pid);
                    if (periodo is not null && periodo.AnioLectivoId is not null)
                    {
                        var anio = periodo.AnioLectivoId.Value;
                        q = from c in q
                            join cur in _db.Curriculas on c.CurriculaId equals cur.Id
                            join dndc in _db.DocentesNivelDetalleCurso on cur.DocenteNivelDetalleCursoId equals dndc.Id
                            join ndc in _db.NivelesDetalleCurso on dndc.NivelDetalleCursoId equals ndc.Id
                            join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                            join m in _db.Matriculas on new { c.AlumnoId, Id = nd.Id, AnioLectivoId = (int?)anio } equals new { m.AlumnoId, Id = m.NivelDetalleId, AnioLectivoId = m.AnioLectivoId }
                            select c;
                    }
                    else
                    {
                        q = _db.Calificaciones.Where(c => false);
                    }
                }
                var listTutor = await q.OrderByDescending(c => c.FechaRegistro).Select(c => new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo, null, null, null, null)).ToListAsync();
                return Ok(ApiResponse<IEnumerable<CalificacionReadDto>>.Success(listTutor));
            }
            else
            {
                return BadRequest(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail("Endpoint para uso de perfiles autenticados: Estudiante o Tutor. Para administradores use /api/calificaciones/by-alumno/{id}"));
            }

            if (alumnoId is null) return BadRequest(ApiResponse<IEnumerable<CalificacionReadDto>>.Fail("Alumno no identificado"));
            var q2 = _db.Calificaciones.AsNoTracking().Where(c => c.AlumnoId == alumnoId.Value);
            if (periodoId is not null)
            {
                var pid = periodoId.Value;
                var periodo = await _db.Periodos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pid);
                if (periodo is not null && periodo.AnioLectivoId is not null)
                {
                    var anio = periodo.AnioLectivoId.Value;
                    q2 = from c in q2
                         join cur in _db.Curriculas on c.CurriculaId equals cur.Id
                         join dndc in _db.DocentesNivelDetalleCurso on cur.DocenteNivelDetalleCursoId equals dndc.Id
                         join ndc in _db.NivelesDetalleCurso on dndc.NivelDetalleCursoId equals ndc.Id
                         join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                         join m in _db.Matriculas on new { c.AlumnoId, Id = nd.Id, AnioLectivoId = (int?)anio } equals new { m.AlumnoId, Id = m.NivelDetalleId, AnioLectivoId = m.AnioLectivoId }
                         select c;
                }
                else
                {
                    q2 = _db.Calificaciones.Where(c => false);
                }
            }
            var result = await q2.OrderByDescending(c => c.FechaRegistro).Select(c => new CalificacionReadDto(c.Id, c.CurriculaId, c.AlumnoId, c.Nota, c.FechaRegistro, c.Activo, null, null, null, null)).ToListAsync();
            return Ok(ApiResponse<IEnumerable<CalificacionReadDto>>.Success(result));
        }

        /// <summary>
        /// Devuelve el horario del alumno que realiza la petición (Estudiante) o de los pupilos del Tutor.
        /// Roles: Admin, Direccion, Subdireccion, JefeArea, Docente, Estudiante, Tutor
        /// </summary>
        [HttpGet("me/horario")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public async Task<ActionResult<ApiResponse<IEnumerable<HorarioReadDto>>>> MyHorario()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized();

            if (User.IsInRole("Estudiante"))
            {
                if (user.PersonaId is null) return Forbid();
                var alumno = await _db.Alumnos.AsNoTracking().FirstOrDefaultAsync(a => a.PersonaId == user.PersonaId);
                if (alumno is null) return NotFound(ApiResponse<IEnumerable<HorarioReadDto>>.Fail("Alumno no encontrado"));

                var horarios = await (from h in _db.Horarios.AsNoTracking()
                                      join ndc in _db.NivelesDetalleCurso on h.NivelDetalleCursoId equals ndc.Id
                                      join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                                      join m in _db.Matriculas on new { AlumnoId = alumno.Id, NivelDetalleId = nd.Id } equals new { m.AlumnoId, NivelDetalleId = m.NivelDetalleId }
                                      select new HorarioReadDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro))
                                      .ToListAsync();
                return Ok(ApiResponse<IEnumerable<HorarioReadDto>>.Success(horarios));
            }

            if (User.IsInRole("Tutor"))
            {
                if (user.PersonaId is null) return Forbid();
                var ap = await _db.Apoderados.AsNoTracking().FirstOrDefaultAsync(a => a.PersonaId == user.PersonaId);
                if (ap is null) return Forbid();
                var pupils = await _db.AlumnosApoderados.AsNoTracking().Where(x => x.ApoderadoId == ap.Id && x.FechaFin == null).Select(x => x.AlumnoId).ToListAsync();

                var horarios = await (from h in _db.Horarios.AsNoTracking()
                                      join ndc in _db.NivelesDetalleCurso on h.NivelDetalleCursoId equals ndc.Id
                                      join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                                      join m in _db.Matriculas on new { NivelDetalleId = nd.Id } equals new { NivelDetalleId = m.NivelDetalleId }
                                      where pupils.Contains(m.AlumnoId)
                                      select new HorarioReadDto(h.Id, h.NivelDetalleCursoId, h.DiaSemana, h.HoraInicio, h.HoraFin, h.Activo, h.FechaRegistro))
                                      .ToListAsync();
                return Ok(ApiResponse<IEnumerable<HorarioReadDto>>.Success(horarios));
            }

            return BadRequest(ApiResponse<IEnumerable<HorarioReadDto>>.Fail("Endpoint para Estudiante/Tutor. Administradores/Docentes deben usar endpoints de Horarios."));
        }

        // ======================================================================
        // POST avanzado: crea Alumno + Persona + Usuario(Estudiante)
        // Si es menor de 18, también crea Tutor (Persona + Apoderado + Usuario(Tutor))
        // ======================================================================
        [HttpPost]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
        public async Task<ActionResult<ApiResponse<AlumnoCreateResultDto>>> Create([FromBody] AlumnoCreateWithAccountsDto req)
        {
            // Validaciones mínimas
            if (req.AlumnoPersona is null)
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("AlumnoPersona es requerido."));

            if (string.IsNullOrWhiteSpace(req.AlumnoEmail) || string.IsNullOrWhiteSpace(req.AlumnoPassword))
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Credenciales del alumno son requeridas."));

            if (req.AlumnoPersona.FechaNacimiento is null)
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("FechaNacimiento del alumno es obligatoria para validar mayoría de edad."));

            if (await _userManager.FindByEmailAsync(req.AlumnoEmail) is not null)
                return Conflict(ApiResponse<AlumnoCreateResultDto>.Fail("El email del alumno ya está registrado."));

            var edad = CalcularEdad(req.AlumnoPersona.FechaNacimiento.Value);
            var esMenor = edad < 18;

            if (esMenor)
            {
                if (req.Tutor is null)
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("El alumno es menor de edad: la información del Tutor es requerida."));

                if (await _userManager.FindByEmailAsync(req.Tutor.Email) is not null)
                    return Conflict(ApiResponse<AlumnoCreateResultDto>.Fail("El email del tutor ya está registrado."));

                if (string.IsNullOrWhiteSpace(req.Tutor.Password))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("La contraseña del tutor es requerida."));
            }

            // Validar/normalizar doc y teléfonos antes de grabar ↓↓↓
            if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.AlumnoPersona.NumeroTelefono, out var telAlumnoE164))
                return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Número telefónico de alumno inválido para Nicaragua."));

            string? docAlumno = req.AlumnoPersona.DocumentoIdentidad?.Trim();

            if (esMenor)
            {
                if (req.Tutor is null || string.IsNullOrWhiteSpace(req.Tutor.DocumentoIdentidad))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Tutor: DocumentoIdentidad requerido."));

                if (!CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(req.Tutor.DocumentoIdentidad.Trim(), out _))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Tutor: DocumentoIdentidad inválido (cédula NI)."));

                if (!TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.Tutor.NumeroTelefono, out var _))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Tutor: número telefónico inválido para Nicaragua."));

                // Alumno menor debe quedar con TUTOR-<cedTutor>
                docAlumno = $"TUTOR-{req.Tutor.DocumentoIdentidad.Trim()}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(docAlumno) || !CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(docAlumno, out _))
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail("Alumno: DocumentoIdentidad inválido (cédula NI)."));
            }

            if (await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == docAlumno))
                return Conflict(ApiResponse<AlumnoCreateResultDto>.Fail("DocumentoIdentidad ya existe."));
            // ↑↑↑ NEW

            using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                // 1) Persona del alumno
                var pAlumno = new Persona
                {
                    Nombres = req.AlumnoPersona.Nombres,
                    Apellidos = req.AlumnoPersona.Apellidos,
                    DocumentoIdentidad = docAlumno!,                  // ← NEW
                    FechaNacimiento = req.AlumnoPersona.FechaNacimiento.HasValue ? req.AlumnoPersona.FechaNacimiento.Value : (DateTime?)null,
                    Sexo = req.AlumnoPersona.Sexo,
                    Ciudad = req.AlumnoPersona.Ciudad,
                    Direccion = req.AlumnoPersona.Direccion,
                    NumeroTelefono = telAlumnoE164,                  // ← NEW
                    Email = req.AlumnoEmail
                };
                _db.Personas.Add(pAlumno);
                await _db.SaveChangesAsync();

                // 2) Alumno
                var alumno = new Alumno
                {
                    PersonaId = pAlumno.Id,
                    Activo = true
                };
                _db.Alumnos.Add(alumno);
                await _db.SaveChangesAsync();

                // 3) Usuario del alumno (rol Estudiante) — pendiente aprobación
                var userAlumno = new ApplicationUser
                {
                    UserName = req.AlumnoEmail,
                    Email = req.AlumnoEmail,
                    EmailConfirmed = true,
                    FullName = $"{pAlumno.Nombres} {pAlumno.Apellidos}",
                    PersonaId = pAlumno.Id,
                    IsApproved = false
                };
                var createAlumno = await _userManager.CreateAsync(userAlumno, req.AlumnoPassword);
                if (!createAlumno.Succeeded)
                    return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail(string.Join("; ", createAlumno.Errors.Select(e => e.Description))));

                await _userManager.AddToRoleAsync(userAlumno, "Estudiante");

                int? personaTutorId = null;
                int? apoderadoId = null;
                string? tutorEmail = null;

                // 4) Si es menor, crear Tutor (Persona + Apoderado + Usuario con rol Tutor)
                if (esMenor && req.Tutor is not null)
                {
                    // Normaliza teléfono tutor (ya validado arriba)
                    TelefonoNicaraguenseValidadorHelper.TryNormalizePhoneNi(req.Tutor.NumeroTelefono, out var telTutorE164);

                    var pTutor = new Persona
                    {
                        Nombres = req.Tutor.Nombres,
                        Apellidos = req.Tutor.Apellidos,
                        DocumentoIdentidad = req.Tutor.DocumentoIdentidad,
                        FechaNacimiento = req.Tutor.FechaNacimiento.HasValue ? req.Tutor.FechaNacimiento.Value : (DateTime?)null,
                        Sexo = req.Tutor.Sexo,
                        Ciudad = req.Tutor.Ciudad,
                        Direccion = req.Tutor.Direccion,
                        NumeroTelefono = telTutorE164,                // ← NEW
                        Email = req.Tutor.Email
                    };
                    _db.Personas.Add(pTutor);
                    await _db.SaveChangesAsync();
                    personaTutorId = pTutor.Id;

                    var apoderado = new Apoderado
                    {
                        PersonaId = pTutor.Id,
                        TipoParentesco = req.Tutor.TipoParentesco ?? "Tutor",
                        Activo = true
                    };
                    _db.Apoderados.Add(apoderado);
                    await _db.SaveChangesAsync();
                    apoderadoId = apoderado.Id;

                    var userTutor = new ApplicationUser
                    {
                        UserName = req.Tutor.Email,
                        Email = req.Tutor.Email,
                        EmailConfirmed = true,
                        FullName = $"{pTutor.Nombres} {pTutor.Apellidos}",
                        PersonaId = pTutor.Id,
                        IsApproved = false
                    };
                    var createTutor = await _userManager.CreateAsync(userTutor, req.Tutor.Password);
                    if (!createTutor.Succeeded)
                        return BadRequest(ApiResponse<AlumnoCreateResultDto>.Fail(string.Join("; ", createTutor.Errors.Select(e => e.Description))));

                    await _userManager.AddToRoleAsync(userTutor, "Tutor");
                    tutorEmail = req.Tutor.Email;
                }

                await tx.CommitAsync();

                var result = new AlumnoCreateResultDto
                {
                    AlumnoId = alumno.Id,
                    AlumnoPersonaId = pAlumno.Id,
                    AlumnoEmail = req.AlumnoEmail,
                    EsMenorDeEdad = esMenor,
                    TutorPersonaId = personaTutorId,
                    ApoderadoId = apoderadoId,
                    TutorEmail = tutorEmail
                };

                return CreatedAtAction(nameof(GetOne), new { id = alumno.Id }, ApiResponse<AlumnoCreateResultDto>.Success(result, "Alumno creado"));
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // ======================================================================
        // PUT: api/alumnos/{id}  (actualiza datos en Persona + Activo en Alumno)
        // ======================================================================
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<AlumnoReadDto>>> Update(int id, [FromBody] AlumnoUpdateDto dto)
        {
            var a = await _db.Alumnos
                .Include(x => x.Persona)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (a is null)
                return NotFound(ApiResponse<AlumnoReadDto>.Fail("Alumno no encontrado"));

            // ↓↓↓ NEW: Validación básica de documento (cédula NI o TUTOR-<ced>) y unicidad ↓↓↓
            if (!string.IsNullOrWhiteSpace(dto.DocumentoIdentidad))
            {
                var doc = dto.DocumentoIdentidad.Trim();
                var okDoc = CedulaNicaraguenseValidadorHelper.TryParseCedulaNica(doc, out _) || CedulaNicaraguenseValidadorHelper.IsTutorPattern(doc, out _);
                if (!okDoc) return BadRequest(ApiResponse<AlumnoReadDto>.Fail("DocumentoIdentidad inválido."));

                if (!string.Equals(doc, a.Persona.DocumentoIdentidad, StringComparison.OrdinalIgnoreCase) &&
                    await _db.Personas.AnyAsync(p => p.DocumentoIdentidad == doc))
                    return Conflict(ApiResponse<AlumnoReadDto>.Fail("DocumentoIdentidad ya existe."));

                a.Persona.DocumentoIdentidad = doc;
            }
            // ↑↑↑ NEW

            // Actualiza datos en Persona (ya no en Alumno)
            a.Persona.Nombres = dto.Nombres;
            a.Persona.Apellidos = dto.Apellidos;
            a.Persona.Ciudad = dto.Ciudad;
            a.Persona.Direccion = dto.Direccion;
            a.Activo = dto.Activo;

            await _db.SaveChangesAsync();

            var read = new AlumnoReadDto(
                a.Id,
                a.Persona.Nombres,
                a.Persona.Apellidos,
                /* Codigo */ null,
                a.Persona.DocumentoIdentidad,
                a.Persona.Ciudad,
                a.Persona.Direccion,
                a.Activo);

            return Ok(ApiResponse<AlumnoReadDto>.Success(read, "Alumno actualizado"));
        }

        // ======================================================================
        // DELETE: api/alumnos/{id} (soft-delete → Activo=false)
        // ======================================================================
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var a = await _db.Alumnos.FindAsync(id);
            if (a is null) return NotFound(ApiResponse<string>.Fail("Alumno no encontrado"));

            a.Activo = false;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<string>.Success("OK", "Alumno desactivado"));
        }
    }
}
