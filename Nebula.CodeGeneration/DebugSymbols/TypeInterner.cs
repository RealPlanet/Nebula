using Nebula.CodeGeneration.Definitions;
using Nebula.Interop.Enumerators;
using System.Collections.Generic;

namespace Nebula.CodeGeneration.DebugSymbols
{
    /// <summary>
    /// Interns TypeReferences into the id-keyed DebugSymbolsFile.Types table. Two
    /// locals both typed int[][] share one entry; array/object nesting is resolved
    /// by following ElementTypeId / Members[].TypeId chains, not by re-parsing a
    /// type name string.
    /// </summary>
    internal sealed class TypeInterner
    {
        private readonly DebugSymbolsFile _file;
        private readonly Dictionary<string, int> _idsByKey = [];
        private int _nextId;

        public TypeInterner(DebugSymbolsFile file)
        {
            _file = file;
        }

        /// <summary>
        /// Registers a bundle's full member layout up front. Call this for every
        /// bundle before emitting any function bodies, so a later field/local/param
        /// reference to this object type resolves to this same, fully-populated
        /// entry instead of a nameless stub.
        /// </summary>
        public int RegisterClass(ClassDefinition bundle, string @namespace)
        {
            TypeReference bundleType = TypeReference.ObjectOf(@namespace, bundle.Name);
            string key = bundleType.CompiledName;

            if (_idsByKey.TryGetValue(key, out int existingId))
            {
                return existingId;
            }

            int id = _nextId++;
            _idsByKey[key] = id;

            ObjectTypeDebugSymbol symbol = new()
            {
                Id = id,
                Name = bundleType.Name,
                Identifier = TypeIdentifier.Object,
                ObjectNamespace = @namespace,
                ObjectName = bundle.Name,
            };

            // Reserve the id before resolving members, so a field whose type refers
            // back to this same bundle (a self-referential/recursive object type)
            // resolves to this entry instead of recursing forever.
            _file.Types[id] = symbol;

            foreach (ParameterDefinition field in bundle.Fields)
            {
                int fieldTypeId = GetOrCreateTypeId(field.VariableType);
                symbol.Members.Add(new ObjectMemberSymbol { Name = field.Name, TypeId = fieldTypeId });
            }

            return id;
        }

        /// <summary>
        /// Resolves (creating if needed) the id for any TypeReference — primitive,
        /// array (recursively, any depth), or object. An object not previously seen
        /// via RegisterBundle (declared in another module) gets an empty-Members
        /// stub; its own .ndbg carries the full layout.
        /// </summary>
        public int GetOrCreateTypeId(TypeReference type)
        {
            string key = type.CompiledName;

            if (_idsByKey.TryGetValue(key, out int existingId))
            {
                return existingId;
            }

            int id = _nextId++;
            _idsByKey[key] = id;

            BaseTypeDebugSymbol symbol = type.Identifier switch
            {
                TypeIdentifier.Array => new ArrayTypeDebugSymbol
                {
                    Id = id,
                    Name = type.Name,
                    Identifier = TypeIdentifier.Array,
                    ArrayTypeId = GetOrCreateTypeId(type.ElementType!),
                },
                TypeIdentifier.Object => new ObjectTypeDebugSymbol
                {
                    Id = id,
                    Name = type.Name,
                    Identifier = TypeIdentifier.Object,
                    ObjectNamespace = type.SourceNamespace ?? string.Empty,
                    ObjectName = type.SourceTypeName ?? string.Empty,
                },
                _ => new BaseTypeDebugSymbol
                {
                    Id = id,
                    Name = type.Name,
                    Identifier = type.Identifier,
                },
            };

            _file.Types[id] = symbol;
            return id;
        }
    }
}
