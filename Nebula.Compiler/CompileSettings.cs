using Nebula.Core.Utility;
using System.Collections.Generic;
using System.IO;

namespace Nebula.Compilation
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CompileSettings
    {
        /// <summary> All the paths to include during compilation </summary>
        public IReadOnlyList<string> SourceFiles => _sourceFiles;

        public IReadOnlyList<string> References => _references;

        /// <summary> Where to put the compiled files </summary>
        public string OutputFolder { get; set; } = string.Empty;

        private readonly List<string> _sourceFiles = [];
        private readonly List<string> _references = [];

        internal bool AddReferenceFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // We support both single file path and complete directories
            if (TryGetFiles(path, Constants.CompiledExtension, out string[] files))
            {
                _references.AddRange(files);
                return true;

            }

            return false;
        }

        internal bool AddCompilationPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // We support both single file path and complete directories
            if (TryGetFiles(path, Constants.SourceFileExtension, out string[] files))
            {
                _sourceFiles.AddRange(files);
                return true;

            }

            return false;
        }

        internal void Clear()
        {
            _sourceFiles.Clear();
            _references.Clear();
            OutputFolder = string.Empty;
        }

        private static bool TryGetFiles(string where, string extension, out string[] foundFiles)
        {
            foundFiles = [];
            if (IsDirectory(where, extension, out foundFiles))
            {
                if (extension.Length == 0)
                {
                    return false;
                }

                return true;
            }

            string ext = Path.GetExtension(where);
            if (ext != extension || !File.Exists(where))
            {
                return false;
            }

            foundFiles = new string[] { where };
            return true;
        }

        private static bool IsDirectory(string path, string extFile, out string[] allFiles)
        {
            allFiles = [];

            FileAttributes attrs = File.GetAttributes(path);
            if (attrs.HasFlag(FileAttributes.Directory))
            {
                allFiles = Directory.GetFiles(path, $"*{extFile}", SearchOption.AllDirectories);
                return true;
            }

            return false;
        }
    }
}