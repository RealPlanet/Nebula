using Microsoft.Extensions.Logging;
using Nebula.Debugger.Bridge;
using Nebula.Debugger.DAP;
using Nebula.Debugger.Logging;
using Ookii.CommandLine;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;

namespace Nebula.Debugger
{
    [GeneratedParser]
    internal partial class ProgramArgs
    {
        [CommandLineArgument("debug_dap", DefaultValue = false, IsRequired = false)]
        [Description("If true will launch debugging of DAP")]
        public bool DebugDAP { get; set; }

        [CommandLineArgument("stopOnEntry", DefaultValue = false, IsRequired = false)]
        [Description("If true immediately halt execution of code")]
        public bool StepOnEntry { get; set; }

        [CommandLineArgument("server_port", DefaultValue = 0, IsRequired = false)]
        [Description("Server port")]
        public int ServerPort { get; set; }
    }

    internal class Program
    {
        public static ILogger AppLogger { get; private set; } = null!;
        public static int Main(string[] args)
        {

            CommandLineParser parser = new CommandLineParser(typeof(ProgramArgs));
            ProgramArgs arguments = null!;
            try
            {
                arguments = (ProgramArgs?)parser.Parse(args) ?? new();
            }
            catch (CommandLineArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                parser.WriteUsage();
                return -1;
            }

            if (arguments.DebugDAP)
                System.Diagnostics.Debugger.Launch();

            DebuggerConfiguration configuration = new(Console.OpenStandardInput(), Console.OpenStandardOutput())
            {
                StepOnEntry = arguments.StepOnEntry,
            };

            AppLogger = CreateFileLogger();
            AppLogger.LogInformation("Debugger start");

            if (arguments.ServerPort != 0)
            {
                RunServer(arguments, configuration);
                return 0;
            }

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

        private static void RunServer(ProgramArgs args, DebuggerConfiguration configuration)
        {
            Console.WriteLine(FormattableString.Invariant($"Waiting for connections on port {args.ServerPort}..."));
            NebulaDebuggerAdapter adapter = null;

            Thread listenThread = new Thread(() =>
            {
                TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), args.ServerPort);
                listener.Start();

                while (true)
                {
                    Socket clientSocket = listener.AcceptSocket();
                    Thread clientThread = new(() =>
                    {
                        Console.WriteLine("Accepted connection");

                        using (Stream stream = new NetworkStream(clientSocket))
                        {
                            adapter = new NebulaDebuggerAdapter(configuration, AppLogger);
                            adapter.Protocol.LogMessage += (sender, e) => Console.WriteLine(e.Message);
                            adapter.Protocol.DispatcherError += (sender, e) =>
                            {
                                Console.Error.WriteLine(e.Exception.Message);
                            };
                            adapter.Run();
                            adapter.Protocol.WaitForReader();

                            adapter = null;
                        }

                        Console.WriteLine("Connection closed");
                    })
                    {
                        Name = "DebugServer connection thread"
                    };
                    clientThread.Start();
                }
            });

            listenThread.Name = "DebugServer listener thread";
            listenThread.Start();
            listenThread.Join();
        }
    }
}
