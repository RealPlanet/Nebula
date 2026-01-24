using Microsoft.Extensions.DependencyInjection;
using Nebula.LSP.Documents;
using Nebula.LSP.Handlers;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.LSP
{
    public class LSPServer
    {
        private readonly Serilog.ILogger _logger;
        private readonly LanguageServer _internalServer;

        public LSPServer(Stream input, Stream output, Serilog.ILogger logger)
        {
            _logger = logger;
            _logger.Information("Initializing LSPServer");

            _internalServer = LanguageServer.Create(o =>
                o
                .WithInput(input)
                .WithOutput(output)
                .ConfigureLogging(x => x.AddSerilog(logger))
                .WithServices(ConfigureServices)
                .WithHandler<TextDocumentHandler>()
                .WithHandler<CompletionHandler>()
                .WithHandler<SymbolHandler>()
                .OnInitialize(InitializeLSPAsync)
                );

        }

        private async Task InitializeLSPAsync(ILanguageServer server, InitializeParams request, CancellationToken cancellationToken)
        {
            try
            {
                var library = _internalServer.GetService<DocumentLibrary>()!;
                var cache = _internalServer.GetService<CompilationCache>()!;

                var wsFolders = request.WorkspaceFolders ?? [];
                foreach (var folder in wsFolders)
                {
                    var folderPath = folder.Uri.ToUri().AbsolutePath;
                    var allFiles = Directory.EnumerateFiles(folderPath, "*.nebula", SearchOption.AllDirectories);

                    foreach (var file in allFiles)
                    {
                        Uri tempUri = new Uri(file);
                        var contents = await File.ReadAllTextAsync(file, cancellationToken);
                        library.AddDocument(new NebulaDocument("nebula", tempUri, 0, contents));
                    }
                }

                cache.CheckCompile();
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Exception while initializing: {0}", ex.ToString());
            }

        }

        public async Task RunAsync(CancellationToken token)
        {
            await _internalServer.Initialize(token).ConfigureAwait(false);
            await _internalServer.WaitForExit.ConfigureAwait(false);
        }

        private void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<DocumentLibrary>();
            collection.AddSingleton<CompilationCache>();
        }
    }
}
