using System.Text;

namespace Nebula.CodeGeneration
{
    internal static class Extensions
    {
        internal static string Join<T>(this T[] arguments, char separator)
        {
            StringBuilder sb = new();
            foreach (T i in arguments)
            {
                sb.Append(i);
                sb.Append(separator);
            }

            return sb.ToString().Trim(separator);
        }
    }
}
