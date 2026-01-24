using Mono.Options;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.LSP
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            bool showHelp = false;
            OptionSet options = new()
            {
                { "h|?|help", v => showHelp =true },
            };

            options.Parse(args);

            if (showHelp)
            {
                Console.WriteLine("Printing available commands:");
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("Logging/Nebula.LSP.log", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Verbose()
                .CreateLogger();

            var server = new LSPServer(Console.OpenStandardInput(),
                                       Console.OpenStandardOutput(),
                                       Log.Logger);

            await server.RunAsync(CancellationToken.None);
            return 0;
        }
    }
}
