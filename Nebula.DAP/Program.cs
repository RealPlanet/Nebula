using Microsoft.Extensions.Logging;
using Nebula.Debugger.Bridge;
using Nebula.Debugger.DAP;
using Nebula.Debugger.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace Nebula.Debugger
{
    //[GeneratedParser]
    //internal partial class ProgramArgs
    //{
    //    [CommandLineArgument("debug", DefaultValue = false, IsRequired = false)]
    //    [Description("If true will launch debugging of DAP")]
    //    public bool Debug { get; set; }
    //
    //    [CommandLineArgument("nodebug", DefaultValue = false, IsRequired = false)]
    //    public bool NoDebug { get; set; }
    //
    //    [CommandLineArgument("server", DefaultValue = 0, IsRequired = false)]
    //    public int ServerPort { get; set; }
    //
    //    [CommandLineArgument("stepOnEnter", DefaultValue = false, IsRequired = false)]
    //    public bool StepOnEnter { get; set; }
    //}

    internal class Program
    {
        public static ILogger AppLogger { get; private set; } = null!;
        public static int Main(string[] args)
        {
            //CommandLineParser parser = new CommandLineParser(typeof(ProgramArgs));
            //ProgramArgs arguments = null;
            //
            //try
            //{
            //    arguments = (ProgramArgs)parser.Parse(args);
            //}
            //catch (CommandLineArgumentException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    parser.WriteUsage();
            //    return -1;
            //}

            System.Diagnostics.Debugger.Launch();

            DebuggerConfiguration configuration = new(Console.OpenStandardInput(), Console.OpenStandardOutput())
            {
                StepOnEntry = true,
            };

            AppLogger = CreateFileLogger();
            AppLogger.LogInformation("Debugger start");

            // Standard mode - run with the adapter connected to the process's stdin and stdout
            NebulaDebuggerAdapter adapter = new(configuration, AppLogger);
            adapter.Protocol.LogMessage += (sender, e) => DiagnosticLog(sender, e.Message);
            adapter.Protocol.DispatcherError += (sender, e) => DiagnosticLog(sender, e.Exception.ToString());
            adapter.Protocol.ResponseTimeThresholdExceeded += (sender, e) => Debug.WriteLine(e.Command.ToString());
            adapter.Run();
            return 0;
        }

        private static void DiagnosticLog(object? sender, string message)
        {
            Debug.WriteLine(message);
            AppLogger.LogError(message);
        }

        private static ILogger CreateFileLogger()
        {
            const string logDir = "logs";
            string logTempDir = Path.Combine(Path.GetTempPath(), logDir);
            string logFullPath = Path.Combine(logTempDir, "dbg_report.log");

            Directory.CreateDirectory(logTempDir);

            // Disposed by logger when closing
            StreamWriter logFileWriter = new(logFullPath, append: true);
            using ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new CustomFileLoggerProvider(logFileWriter));
            });

            //Create an ILogger
            ILogger<Program> logger = factory.CreateLogger<Program>();
            return logger;
        }
    }
}
