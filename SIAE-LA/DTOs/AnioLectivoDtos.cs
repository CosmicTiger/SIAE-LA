#nullable enable
using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public sealed record AnioLectivoReadDto(int Id, int Anio, string? Descripcion, bool Activo, DateTime FechaInicio, DateTime? FechaFin, DateTime FechaRegistro, string? CreadoPor = null, string? ModificadoPor = null, DateTime? FechaModificacion = null, DateTime? FechaIngreso = null);

    public sealed class AnioLectivoCreateDto
    {
        [Required] public int Anio { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public sealed class AnioLectivoUpdateDto
    {
        [Required] public int Anio { get; set; }
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Activo { get; set; } = true;
    }
}
