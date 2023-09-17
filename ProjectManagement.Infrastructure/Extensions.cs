using System.Text.RegularExpressions;

namespace ProjectManagement.Infrastructure
{
    public static class Extensions
    {
        public static string Standard(this object? obj)
        {
            // We use Regex to select only starting numbers
            return obj != null ? Regex.Match(obj.ToString(), @"\d+").Value : "";
        }
    }
}
