using System;

namespace Nebula.Compilation
{
    public sealed class ConsoleCompilerWriter
    {
        public string AppName { get; }

        public ConsoleCompilerWriter(string appName)
        {
            AppName = appName;
        }

        public void WriteLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Write(message, newLine: true);
            Console.ResetColor();
        }

        private void Write(string message, bool newLine)
        {
            message = $"[{AppName}] - {message}";
            if (newLine)
            {
                Console.WriteLine(message);
                return;
            }

            Console.Write(message);
        }
    }
}