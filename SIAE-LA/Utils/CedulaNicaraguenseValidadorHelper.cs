using System.Text.RegularExpressions;

namespace SIAE_LA.Utils
{
    public class CedulaNicaraguenseValidadorHelper
    {
        // MMM-DDMMAA-SSSSL con DD/ MM válidos y letra A-Z
        private static readonly Regex CedulaRegex = new(
            @"^(?<mun>\d{3})-(?<dd>0[1-9]|[12]\d|3[01])(?<mm>0[1-9]|1[0-2])(?<yy>\d{2})-(?<seq>\d{4})(?<ver>[A-Z])$",
            RegexOptions.Compiled);

        private static readonly Regex TutorRegex = new(
            @"^TUTOR-(?<inner>\d{3}-(?:0[1-9]|[12]\d|3[01])(?:0[1-9]|1[0-2])\d{2}-\d{4}[A-Z])$",
            RegexOptions.Compiled);

        public static bool TryParseCedulaNica(string cedula, out DateTime fechaNac)
        {
            fechaNac = default;
            var m = CedulaRegex.Match(cedula);
            if (!m.Success) return false;

            int dd = int.Parse(m.Groups["dd"].Value);
            int mm = int.Parse(m.Groups["mm"].Value);
            int yy = int.Parse(m.Groups["yy"].Value);

            // Pivot: si yy > añoActual(2d) => 1900+yy, si no => 2000+yy
            var nowYY = DateTime.UtcNow.Year % 100;
            int year = yy > nowYY ? 1900 + yy : 2000 + yy;

            try { fechaNac = new DateTime(year, mm, dd); return true; }
            catch { return false; }
        }

        public static bool IsTutorPattern(string s, out string innerCedula)
        {
            innerCedula = "";
            var m = TutorRegex.Match(s);
            if (!m.Success) return false;
            innerCedula = m.Groups["inner"].Value;
            return TryParseCedulaNica(innerCedula, out _);
        }

        public static string BuildCedula(string municipio3, DateTime fechaNac, int secuencial4, char verificador)
        {
            string dd = fechaNac.Day.ToString("00");
            string mm = fechaNac.Month.ToString("00");
            string yy = (fechaNac.Year % 100).ToString("00");
            return $"{municipio3}-{dd}{mm}{yy}-{secuencial4:0000}{char.ToUpper(verificador)}";
        }
    }
}
