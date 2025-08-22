using Mono.Options;
using Nebula.Commons.Text;
using Nebula.Commons.Text.Printers;
using Nebula.Interop.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Nebula.Compiler
{
    internal class Program
    {
        public static CompileSettings CompilerSettings = null!;
        public static ConsoleCompilerWriter Writer = new("NEBULA");

        #region Arguments
        static void AddCompilationPathToCompiler(string path)
        {
            path = path.Trim();
            if (!CompilerSettings.AddCompilationPath(path))
            {
                Writer.WriteLine($"Path '{path}' is not a valid source file or root directory", ConsoleColor.Red);
            }
        }

        static void AddReferencePathToCompiler(string path)
        {
            path = path.Trim();
            if (!CompilerSettings.AddReferenceFile(path))
            {
                Writer.WriteLine($"Path '{path}' is not a valid source file or root directory", ConsoleColor.Red);
            }
        }

        static void AddOutputFolderToCompiler(string path)
        {
            if (!Directory.Exists(path))
            {
                Writer.WriteLine($"Output folder '{path}' does not exist, one will be created!", ConsoleColor.Yellow);
            }
            else
            {
                FileAttributes attrs = File.GetAttributes(path);
                if (!attrs.HasFlag(FileAttributes.Directory))
                {
                    Writer.WriteLine($"Path '{path}' is not a valid directory", ConsoleColor.Red);
                }
            }

            Writer.WriteLine($"Using directory '{path}' for compilation result", ConsoleColor.Green);
            CompilerSettings.OutputFolder = path;
        }

        static bool ParseArguments(string[] args)
        {
            CompilerSettings = new();

            bool showHelp = false;
            OptionSet options = new()
            {
                { "h|?|help", v => showHelp =true },
                { "f=", "A source file or directory path, if a directory is provided all sub directories will also be scanned for source files" , AddCompilationPathToCompiler },
                { "r=", "A compiled file or directory path, if a directory is provided all sub directories will also be scanned for compiled files" , AddReferencePathToCompiler },
                { "o=|output_folder" , AddOutputFolderToCompiler },
                { "next_to_source", v => CompilerSettings.OutputToSourceLocation = true }
            };

            options.Parse(args);

            if (showHelp)
            {
                Writer.WriteLine("Printing available commands:", ConsoleColor.Gray);
                options.WriteOptionDescriptions(Console.Out);
                CompilerSettings.Clear();
                return false;
            }

            bool noOutputFolder = string.IsNullOrEmpty(CompilerSettings.OutputFolder) && !CompilerSettings.OutputToSourceLocation;
            if (noOutputFolder || CompilerSettings.SourceFiles.Count == 0)
            {
                CompilerSettings.Clear();
                return false;
            }

            return true;
        }

        #endregion

        static void Main(string[] args)
        {
            Writer.WriteLine("Compiler initializing.", ConsoleColor.DarkGreen);
            if (!ParseArguments(args))
            {
                Writer.WriteLine("Compiler exiting.", ConsoleColor.DarkGreen);
                /* We quit if we show help screen or something is not valid */
                return;
            }

            List<SourceCode> sources = new() { Capacity = CompilerSettings.SourceFiles.Count };

            foreach (string sourcePath in CompilerSettings.SourceFiles)
            {
                Writer.WriteLine($"Including: {sourcePath}", ConsoleColor.DarkGreen);
                if (!File.Exists(sourcePath))
                {
                    Writer.WriteLine($"Source at '{sourcePath}' does not exist!", ConsoleColor.Yellow);
                    continue;
                }

                sources.Add(SourceCode.From(sourcePath));
            }

            List<Script> references = new() { Capacity = CompilerSettings.SourceFiles.Count };
            foreach (string referencePath in CompilerSettings.References)
            {
                Writer.WriteLine($"Including: {referencePath}", ConsoleColor.DarkGreen);
                if (!File.Exists(referencePath))
                {
                    Writer.WriteLine($" Reference at '{referencePath}' does not exist!", ConsoleColor.Yellow);
                    continue;
                }

                if (!Script.FromFile(referencePath, OnReportCallback, out Script loadedScript))
                {
                    Writer.WriteLine($"Could not load compiled script at '{referencePath}'", ConsoleColor.Red);
                    continue;
                }

                references.Add(loadedScript);
            }

            Core.Compilation.Compiler.Options options = new();
            options.Sources.AddRange(sources);
            options.References.AddRange(references);
            options.OutputFolder = CompilerSettings.OutputFolder;
            options.OutputToSourceLocation = CompilerSettings.OutputToSourceLocation;

            Stopwatch compileTime = Stopwatch.StartNew();
            bool compileOk = Core.Compilation.Compiler.Compile(options, out Core.Compilation.Compiler.Result? result);
            compileTime.Stop();

            Commons.Reporting.Report compileReport = result.Report;
            if (!compileOk)
            {
                Writer.WriteLine($"Compilation failed for {options.Sources.Count} source codes", ConsoleColor.Red);
                Writer.WriteLine($"First failed compilation is: {result.FailedSourcePath}", ConsoleColor.Red);
                Console.Out.WriteReport(compileReport);
            }
            else
            {
                Writer.WriteLine("Compilation done!", ConsoleColor.DarkGreen);
            }

            Writer.WriteLine($"Compilation took '{compileTime.ElapsedMilliseconds}' ms", ConsoleColor.DarkCyan);
        }

        private static void OnReportCallback(string scriptPath, ReportType type, string message)
        {
            ConsoleColor color = ConsoleColor.White;
            switch (type)
            {
                case ReportType.Error:
                    {
                        color = ConsoleColor.Red;
                        break;
                    }
                case ReportType.Warning:
                    {
                        color = ConsoleColor.Yellow;
                        break;
                    }
            }

            Writer.WriteLine($"[{scriptPath}] - {message}", color);
        }
    }
}