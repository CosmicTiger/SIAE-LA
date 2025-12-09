using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using SIAE_LA.Abstractions;

namespace SIAE_LA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public sealed class ReportesController : ControllerBase
    {
        private readonly IReportesRepository _repo;
        public ReportesController(IReportesRepository repo) => _repo = repo;

        /// <summary>
        /// Reporte de alumnos. Devuelve stream de objetos (cada objeto es un diccionario columna?valor).
        /// </summary>
        [HttpGet("alumno")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente,Estudiante,Tutor")]
        public IAsyncEnumerable<Dictionary<string, object>> ReporteAlumno([FromQuery] int? periodoId = null, [FromQuery] int? nivelDetalleId = null, [FromQuery] int? cursoId = null)
        {
            return _repo.GetReporteAlumnoAsync(periodoId, nivelDetalleId, cursoId);
        }

        /// <summary>
        /// Reporte de docentes. Devuelve stream de objetos (cada objeto es un diccionario columna?valor).
        /// </summary>
        [HttpGet("docente")]
        [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea,Docente")]
        public IAsyncEnumerable<Dictionary<string, object>> ReporteDocente([FromQuery] int? docenteId = null, [FromQuery] int? nivelDetalleId = null, [FromQuery] int? cursoId = null)
        {
            return _repo.GetReporteDocenteAsync(docenteId, nivelDetalleId, cursoId);
        }
    }
}
