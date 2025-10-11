using Nebula.Core.Compilation.AST.Bundle;
using Nebula.Core.Compilation.AST.Symbols;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Interop.SafeHandles;
using Nebula.Interop.Structures;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Nebula.Core.Compilation.AST.Binding.Referencing
{
    /// <summary>
    /// Holds all the necessary data to bind references between compilation units and precompiled scripts
    /// Compilation units references have an higher priority if a precompiled script is also provided to ensure the latest
    /// code is always used.
    /// </summary>
    public sealed class AbstractProgramReferences
    {
        public AbstractProgram Program { get; }

        private Dictionary<string, AbstractProgram> AllPrograms { get; } = new();
        private Dictionary<string, Script> AllReferences { get; } = new();

        private readonly Dictionary<string, BundleSymbol> _cachedCompiledBundles = new();
        private readonly Dictionary<string, FunctionSymbol> _cachedCompiledFunctions = new();

        public AbstractProgramReferences(AbstractProgram owner)
        {
            AllPrograms.Add(owner.Namespace.Text, owner);
            Program = owner;
        }

        public void AddAbstractProgramReference(AbstractProgram unit)
        {
            AllPrograms.Add(unit.Namespace.Text, unit);
        }

        public void AddScriptReference(Script script)
        {
            AllReferences.Add(script.Namespace, script);
        }

        public bool TryGetBundle(string @namespace, string bundleName, out BundleSymbol? bundle)
        {
            if (AllPrograms.TryGetValue(@namespace, out AbstractProgram? program) && program.Bundles.TryGetValue(bundleName, out bundle))
            {
                return true;
            }

            if (AllReferences.TryGetValue(@namespace, out Script? script) &&
                script.Bundles.TryGetValue(bundleName, out BundleDefinition? scriptBundle))
            {
                bundle = CreateFromCompiled(scriptBundle);
                return bundle != null;
            }

            bundle = null;
            return false;
        }

        public bool TryGetFunction(string @namespace, string functionName, out FunctionSymbol? function)
        {
            if (AllPrograms.TryGetValue(@namespace, out AbstractProgram? program))
            {
                function = program.Functions.Keys.FirstOrDefault(f => f.Name == functionName);
                return function != null;
            }

            if (AllReferences.TryGetValue(@namespace, out Script? script) &&
                script.Functions.TryGetValue(functionName, out Function? scriptFunction))
            {
                function = CreateFromCompiled(scriptFunction);
                return function != null;
            }

            function = null;
            return false;
        }

        private FunctionSymbol CreateFromCompiled(Function compiledFunction)
        {
            if (_cachedCompiledFunctions.TryGetValue(compiledFunction.Name, out FunctionSymbol? func))
            {
                return func;
            }

            func = new FunctionSymbol(compiledFunction.Name,
                CreateParametersFromCompiled(compiledFunction),
                CreateAttributesFromCompiled(compiledFunction),
                TypeSymbol.TypeFromEnum(compiledFunction.ReturnType),
                null);
            _cachedCompiledFunctions.Add(func.Name, func);
            return func;
        }

        private static ImmutableArray<AttributeSymbol> CreateAttributesFromCompiled(Function compiledFunction)
        {
            ImmutableArray<AttributeSymbol>.Builder builder = ImmutableArray.CreateBuilder<AttributeSymbol>();
            foreach (FunctionAttribute attr in compiledFunction.Attributes)
            {
                AttributeSymbol attrSymbol = AttributeSymbol.FromName(attr.RawName);
                builder.Add(attrSymbol);
            }

            return builder.ToImmutableArray();

        }

        private static ImmutableArray<ParameterSymbol> CreateParametersFromCompiled(Function compiledFunction)
        {
            ImmutableArray<ParameterSymbol>.Builder builder = ImmutableArray.CreateBuilder<ParameterSymbol>();
            foreach (FunctionParameter param in compiledFunction.Parameters)
            {
                string paramName = $"{builder.Count}_{param.Type}";
                ParameterSymbol symbol = new(paramName, TypeSymbol.TypeFromEnum(param.Type), builder.Count);
                builder.Add(symbol);
            }

            return builder.ToImmutableArray();
        }

        private BundleSymbol CreateFromCompiled(BundleDefinition compiledBundle)
        {
            if (_cachedCompiledBundles.TryGetValue(compiledBundle.Name, out BundleSymbol? bundle))
            {
                return bundle;
            }

            bundle = new BundleSymbol(compiledBundle.Name, null!, CreateFieldsFromCompiled(compiledBundle));
            _cachedCompiledBundles.Add(bundle.Name, bundle);
            return bundle;
        }

        private static ImmutableArray<AbstractBundleField> CreateFieldsFromCompiled(BundleDefinition bundle)
        {
            ImmutableArray<AbstractBundleField>.Builder builder = ImmutableArray.CreateBuilder<AbstractBundleField>();

            foreach (BundleField? f in bundle.Fields)
            {
                builder.Add(new AbstractBundleField(TypeSymbol.TypeFromEnum(f.Type), f.Name, builder.Count));
            }

            if (builder.Count != bundle.Fields.Count)
            {
                throw new Exception("Mismatch of field counts");
            }

            return builder.ToImmutable();
        }
    }
}
