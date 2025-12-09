using System.ComponentModel.DataAnnotations;

namespace SIAE_LA.DTOs
{
    public class PersonaInputDto
    {
        [Required] public string Nombres { get; set; } = default!;
        [Required] public string Apellidos { get; set; } = default!;
        public string? DocumentoIdentidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        [Required, RegularExpression("^(M|F)$", ErrorMessage = "Sexo debe ser 'M' o 'F'.")]
        public string Sexo { get; set; } = default!;

        public string? Ciudad { get; set; }
        public string? Direccion { get; set; }
        public string? NumeroTelefono { get; set; }   // se normaliza a +505######## si existe
    }

    public sealed record PersonaDto(
            string Nombres,
            string Apellidos,
            string? DocumentoIdentidad,
            DateTime? FechaNacimiento,
            string? Sexo,
            string? Ciudad,
            string? Direccion,
            string? Email,
            string? NumeroTelefono
        );
}
