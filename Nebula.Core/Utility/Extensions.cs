using Nebula.Core.Compilation.AST.Tree;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nebula.Core.Utility
{
    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static bool IsNotNull([NotNullWhen(true)] this AbstractConstant? value)
        {
            return value != null && value.Value != null;
        }
    }
}