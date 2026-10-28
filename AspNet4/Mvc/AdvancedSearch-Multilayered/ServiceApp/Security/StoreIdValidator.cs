using System.Text.RegularExpressions;

namespace EqDemo.Security
{
    public static class StoreIdValidator
    {
        private static readonly Regex IdPattern =
            new Regex("^[A-Za-z0-9-]{1,64}$", RegexOptions.CultureInvariant);

        public static bool IsValid(string id)
        {
            return id != null && IdPattern.IsMatch(id);
        }
    }
}
