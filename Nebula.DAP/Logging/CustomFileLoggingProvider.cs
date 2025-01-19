using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Nebula.Debugger.Logging
{
    // Customized ILoggerProvider, writes logs to text files
    public class CustomFileLoggerProvider
        : ILoggerProvider
    {
        private readonly StreamWriter _logFileWriter;

        public CustomFileLoggerProvider(StreamWriter logFileWriter)
        {
            _logFileWriter = logFileWriter ?? throw new ArgumentNullException(nameof(logFileWriter));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(categoryName, _logFileWriter);
        }

        public void Dispose()
        {
            _logFileWriter.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
