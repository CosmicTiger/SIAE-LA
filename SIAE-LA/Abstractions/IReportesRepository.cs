using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIAE_LA.Abstractions
{
    public interface IReportesRepository
    {
        IAsyncEnumerable<Dictionary<string, object>> GetReporteAlumnoAsync(int? periodoId = null, int? nivelDetalleId = null, int? cursoId = null);
        IAsyncEnumerable<Dictionary<string, object>> GetReporteDocenteAsync(int? docenteId = null, int? nivelDetalleId = null, int? cursoId = null);
    }
}
