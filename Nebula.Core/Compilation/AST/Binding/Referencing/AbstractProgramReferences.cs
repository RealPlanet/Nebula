using Nebula.Core.Binding.Symbols;
using Nebula.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Nebula.Core.Binding
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
        private Dictionary<string, CompiledScript> AllReferences { get; } = new();

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

        public void AddScriptReference(CompiledScript script)
        {
            AllReferences.Add(script.Namespace, script);
        }

        public bool TryGetBundle(string @namespace, string bundleName, out BundleSymbol? bundle)
        {
            if (AllPrograms.TryGetValue(@namespace, out AbstractProgram? program) && program.Bundles.TryGetValue(bundleName, out bundle))
            {
                return true;
            }

            if (AllReferences.TryGetValue(@namespace, out CompiledScript? script) && script.Bundles.TryGetValue(bundleName, out BundleW? scriptBundle))
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

            if (AllReferences.TryGetValue(@namespace, out CompiledScript? script) && script.Functions.TryGetValue(functionName, out FunctionW? scriptFunction))
            {
                function = CreateFromCompiled(scriptFunction);
                return function != null;
            }

            function = null;
            return false;
        }

        private FunctionSymbol CreateFromCompiled(FunctionW compiledFunction)
        {
            if (_cachedCompiledFunctions.TryGetValue(compiledFunction.Name, out FunctionSymbol? func))
                return func;

            func = new FunctionSymbol(compiledFunction.Name,
                CreateParametersFromCompiled(compiledFunction),
                CreateAttributesFromCompiled(compiledFunction),
                TypeSymbol.TypeFromEnum(compiledFunction.ReturnType),
                null);
            _cachedCompiledFunctions.Add(func.Name, func);
            return func;
        }

        private static ImmutableArray<AttributeSymbol> CreateAttributesFromCompiled(FunctionW compiledFunction)
        {
            ImmutableArray<AttributeSymbol>.Builder builder = ImmutableArray.CreateBuilder<AttributeSymbol>();
            foreach (FunctionAttributeW attr in compiledFunction.Attributes)
            {
                AttributeSymbol attrSymbol = AttributeSymbol.FromName(attr.RawName);
                builder.Add(attrSymbol);
            }

            return builder.ToImmutableArray();

        }

        private static ImmutableArray<ParameterSymbol> CreateParametersFromCompiled(FunctionW compiledFunction)
        {
            ImmutableArray<ParameterSymbol>.Builder builder = ImmutableArray.CreateBuilder<ParameterSymbol>();
            foreach (FunctionParameterW? param in compiledFunction.Parameters)
            {
                string paramName = $"{builder.Count}_{param.Type}";
                ParameterSymbol symbol = new(paramName, TypeSymbol.TypeFromEnum(param.Type), builder.Count);
                builder.Add(symbol);
            }

            return builder.ToImmutableArray();
        }

        private BundleSymbol CreateFromCompiled(BundleW compiledBundle)
        {
            if (_cachedCompiledBundles.TryGetValue(compiledBundle.Name, out BundleSymbol? bundle))
                return bundle;

            bundle = new BundleSymbol(compiledBundle.Name, null!, CreateFieldsFromCompiled(compiledBundle));
            _cachedCompiledBundles.Add(bundle.Name, bundle);
            return bundle;
        }

        private static ImmutableArray<AbstractBundleField> CreateFieldsFromCompiled(BundleW bundle)
        {
            ImmutableArray<AbstractBundleField>.Builder builder = ImmutableArray.CreateBuilder<AbstractBundleField>();

            foreach (BundleFieldW? f in bundle.Fields)
            {
                builder.Add(new AbstractBundleField(TypeSymbol.TypeFromEnum(f.Type), f.Name, builder.Count));
            }

            if (builder.Count != bundle.Fields.Count)
                throw new Exception("Mismatch of field counts");

            return builder.ToImmutable();
        }
    }
}
