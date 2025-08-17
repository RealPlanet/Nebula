namespace Nebula.Compiler.Tests.Utility
{
    public static class Extensions
    {
        public static string ToStringEx<T>(this ICollection<T> c)
        {
            string joined = string.Join(", ", c.Select(c => c?.ToString()));
            return joined;
        }
    }
}
