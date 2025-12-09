using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace SIAE_LA.Utils
{
    public static class ModelStateHelper
    {
        public static string BuildErrors(ModelStateDictionary ms)
        {
            return string.Join("; ", ms.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
