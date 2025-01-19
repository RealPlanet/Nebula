using Microsoft.Extensions.Logging;
using Nebula.Interop;

namespace Nebula.Debugger.Bridge
{
    internal class InteropLogger
        : LoggerW
    {
        public ILogger Logger { get; }

        public InteropLogger(ILogger logger)
        {
            Logger = logger;
        }

        public void LogInformation(string message) => Logger.LogInformation(message);
        public void LogWarning(string message) => Logger.LogWarning(message);
        public void LogError(string message) => Logger.LogError(message);
    }
}
