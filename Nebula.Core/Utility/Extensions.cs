using Nebula.Core.Compilation.AST.Symbols;
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

        public static void GetTypeInformation(this TypeSymbol type, out string typeNamespace, out string typeName)
        {
            if (type is ObjectTypeSymbol objSymbol)
            {
                typeNamespace = objSymbol.Namespace;
                typeName = objSymbol.Name;
            }
            else if(type is ArrayTypeSymbol arrSymbol)
            {
                arrSymbol.ValueType.GetTypeInformation(out typeNamespace, out typeName);
            }
            else
            {
                typeNamespace = string.Empty;
                typeName = type.Name;
            }
        }
    }
}