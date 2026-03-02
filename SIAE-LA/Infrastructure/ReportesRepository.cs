using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.Infrastructure.Persistence;

namespace SIAE_LA.Infrastructure
{
    // Nota: el requisito original pedía SPs, pero el usuario luego indicó "descartemos el uso del SPs".
    // Implementamos consultas parametrizadas con EF Core y transformación a List<Dictionary<string, object>>.
    public class ReportesRepository : IReportesRepository
    {
        private readonly ApplicationDbContext _db;

        public ReportesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async IAsyncEnumerable<Dictionary<string, object>> GetReporteAlumnoAsync(int? periodoId = null, int? nivelDetalleId = null, int? cursoId = null)
        {
            // Query: listar alumnos con matrícula y calificaciones por filtros
            int? periodoAnioId = null;
            if (periodoId is not null)
            {
                var perFilter = await _db.Periodos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == periodoId.Value);
                if (perFilter is null) yield break; // no matching periodo -> empty
                periodoAnioId = perFilter.AnioLectivoId;
                if (periodoAnioId is null) yield break;
            }

            var q = from m in _db.Matriculas.AsNoTracking()
                    join a in _db.Alumnos on m.AlumnoId equals a.Id
                    join p in _db.Personas on a.PersonaId equals p.Id
                    join nd in _db.NivelesDetalle on m.NivelDetalleId equals nd.Id
                    select new { m, a, p, nd };

            if (periodoAnioId is not null) q = q.Where(x => x.m.AnioLectivoId == periodoAnioId.Value);
            if (nivelDetalleId is not null) q = q.Where(x => x.m.NivelDetalleId == nivelDetalleId.Value);

            var queryable = q.OrderBy(x => x.p.Apellidos).ThenBy(x => x.p.Nombres)
                .Select(x => new
                {
                    AlumnoId = x.a.Id,
                    Nombres = x.p.Nombres,
                    Apellidos = x.p.Apellidos,
                    Documento = x.p.DocumentoIdentidad,
                    NivelDetalleId = x.nd.Id,
                    Periodo = _db.Periodos.Where(pp => pp.AnioLectivoId == x.m.AnioLectivoId).OrderBy(pp => pp.Orden).Select(pp => pp.Descripcion).FirstOrDefault(),
                    MatriculaId = x.m.Id,
                    FechaMatricula = x.m.FechaRegistro
                }).AsAsyncEnumerable();

            await foreach (var obj in queryable)
            {
                yield return obj.GetType().GetProperties().ToDictionary(pi => pi.Name, pi => pi.GetValue(obj) ?? (object)DBNull.Value);
            }
        }

        public async IAsyncEnumerable<Dictionary<string, object>> GetReporteDocenteAsync(int? docenteId = null, int? nivelDetalleId = null, int? cursoId = null)
        {
            var q = from d in _db.Docentes.AsNoTracking()
                    join p in _db.Personas on d.PersonaId equals p.Id
                    join asign in _db.DocentesNivelDetalleCurso on d.Id equals asign.DocenteId
                    join ndc in _db.NivelesDetalleCurso on asign.NivelDetalleCursoId equals ndc.Id
                    join nd in _db.NivelesDetalle on ndc.NivelDetalleId equals nd.Id
                    join c in _db.Cursos on ndc.CursoId equals c.Id
                    select new { d, p, asign, ndc, nd, c };

            if (docenteId is not null) q = q.Where(x => x.d.Id == docenteId.Value);
            if (nivelDetalleId is not null) q = q.Where(x => x.nd.Id == nivelDetalleId.Value);
            if (cursoId is not null) q = q.Where(x => x.c.Id == cursoId.Value);

            var queryable = q.Select(x => new
            {
                DocenteId = x.d.Id,
                Nombres = x.p.Nombres,
                Apellidos = x.p.Apellidos,
                Curso = x.c.Descripcion,
                NivelDetalleId = x.nd.Id,
                AsignacionId = x.asign.Id
            }).AsAsyncEnumerable();

            await foreach (var obj in queryable)
            {
                yield return obj.GetType().GetProperties().ToDictionary(pi => pi.Name, pi => pi.GetValue(obj) ?? (object)DBNull.Value);
            }
        }
    }
}
