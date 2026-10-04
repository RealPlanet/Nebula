using Nebula.Interop.Enumerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nebula.CodeGeneration.DebugSymbols
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "Kind")]
    [JsonDerivedType(typeof(BaseTypeDebugSymbol), typeDiscriminator: "Primitive")]
    [JsonDerivedType(typeof(ArrayTypeDebugSymbol), typeDiscriminator: "Array")]
    [JsonDerivedType(typeof(ObjectTypeDebugSymbol), typeDiscriminator: "Object")]
    public class BaseTypeDebugSymbol
    {
        /// <summary>The id other symbols reference this type by — the key into DebugSymbolsFile.Types.</summary>
        [JsonInclude]
        public int Id { get; init; }

        /// <summary> Display name of this symbol </summary>
        [JsonInclude]
        public string Name { get; init; } = string.Empty;

        [JsonInclude]
        public TypeIdentifier Identifier { get; init; }
    }
}
