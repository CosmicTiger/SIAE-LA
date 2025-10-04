using System.Text.RegularExpressions;

namespace SIAE_LA.Utils
{
    public static class TelefonoNicaraguenseValidadorHelper
    {
        // Teléfono NI: opcional +505 / 00505 / (505) y 8 dígitos. Prefijos válidos: 2 (fijo), 5/7/8 (móvil).
        private static readonly Regex PhoneNiRegex = new(
            @"^(?:\+?505|00505|\(505\))?[-.\s]?(?<num>(?:2|5|7|8)\d{7})$",
            RegexOptions.Compiled);

        // ───────── Teléfono Nicaragua ─────────
        public static bool TryNormalizePhoneNi(string? input, out string? e164)
        {
            e164 = null;
            if (string.IsNullOrWhiteSpace(input)) return true; // lo tratamos como "no informado"

            var m = PhoneNiRegex.Match(input.Trim());
            if (!m.Success) return false;

            var eight = m.Groups["num"].Value; // 8 dígitos, ya validado prefijo 2/5/7/8
            e164 = $"+505{eight}";
            return true;
        }
    }
}
