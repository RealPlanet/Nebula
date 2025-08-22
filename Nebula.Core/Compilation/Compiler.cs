using Nebula.Commons.Reporting;
using Nebula.Commons.Text;
using Nebula.Core.Compilation.AST.Binding;
using Nebula.Core.Compilation.AST.Tree;
using Nebula.Core.Compilation.CST.Parsing;
using Nebula.Core.Compilation.Emitting;
using Nebula.Interop.SafeHandles;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nebula.Core.Compilation
{
    /// <summary>Handles the full compilation pipeline of Nebula code</summary>
    public static class Compiler
    {
        /// <summary>The options which can be passed to the compiler</summary>
        public sealed class Options
        {
            public bool EmitProgram { get; set; } = true;
            public string OutputFolder { get; set; } = string.Empty;
            public bool ReadableBytecode { get; set; } = true;
            public bool OutputToSourceLocation { get; set; } = false;
            public List<SourceCode> Sources { get; } = [];
            public List<Script> References { get; } = [];
        }

        /// <summary>Result data of a compilation</summary>
        public sealed class Result
        {
            /// <summary>Contains all generated programs if the compilation was succesful</summary>
            public AbstractProgram[] Programs { get; internal set; } = [];

            /// <summary>Contains any information about the compilation process, such as errors</summary>
            public Report Report { get; set; } = new();

            /// <summary>Contains the source path of the first file to fail the compilation</summary>
            public string FailedSourcePath { get; set; } = string.Empty;
        }

        public static bool Compile(Options options, out Result result)
        {
            result = new();

            if (options.Sources.Count == 0)
            {
                result.Report.PushError("No source to compile provided", default);
                return false;
            }

            if (string.IsNullOrEmpty(options.OutputFolder) && options.EmitProgram)
            {
                result.Report.PushError("No output folder provided", default);
                return false;
            }

            List<CompilationUnit> compileUnits = new(options.Sources.Count);
            foreach (SourceCode source in options.Sources)
            {
                CompilationUnit? unit = Parse(options, source, out Report? parseReport);
                result.Report.Append(parseReport);
                if (unit == null)
                {
                    result.FailedSourcePath = source.FileName;
                    return false;
                }

                compileUnits.Add(unit);
            }

            ICollection<AbstractProgram> programs = Binder.Bind(compileUnits, options.References, out Report bindingReport);
            result.Report.Append(bindingReport);
            if (bindingReport.HasErrors)
            {
                ReportMessage firstError = bindingReport.Errors.First();
                // TODO Fix
                //result.FailedSourcePath = firstError.Location.Text?.FileName ?? throw new NullReferenceException();
                return false;
            }

            result.Programs = [];
            foreach (AbstractProgram program in programs)
            {
                string moduleName = Path.GetFileNameWithoutExtension(program.SourceCode.FileName);
                bool emitOk = Emit(options, moduleName, program, out Report? emitReport);
                result.Report.Append(emitReport);
                if (!emitOk)
                {
                    result.FailedSourcePath = program.SourceCode.FileName;
                    return false;
                }
            }

            result.Programs = programs.ToArray();
            return true;
        }

        private static CompilationUnit? Parse(Options options, SourceCode source, out Report compilationReport)
        {
            CompilationUnit unit = Parser.Parse(source, out compilationReport);
            return compilationReport.HasErrors ? null : unit;
        }

        private static bool Emit(Options options, string moduleName, AbstractProgram program, out Report emitReport)
        {
            if (!options.EmitProgram)
            {
                emitReport = new();
                return true;
            }

            // Emit
            Emitter emitter = new(moduleName, new()
            {
                OutputToSourceLocation = options.OutputToSourceLocation,
                OutputFolder = options.OutputFolder,
                ReadableBytecode = options.ReadableBytecode,
            });

            emitter.Emit(program, out emitReport);
            return !emitReport.HasErrors;
        }
    }
}
