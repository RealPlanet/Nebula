using Nebula.Commons.Text;
using Nebula.Core.Compilation;
using Nebula.Tests.Utility;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Nebula.Tests.Executor
{
    [TestClass]
    public class ExecutorTest
    {
        public TestContext TestContext { get; set; } = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext ctx)
        {
            Assert.IsTrue(File.Exists(ExecutorPath));
        }

#if DEBUG
        public static string ExecutorPath => @"..\..\..\..\..\x64\Debug\Nebula.Executor.exe";
#else
        public static string ExecutorPath => @"..\..\..\..\..\x64\Release\Nebula.Executor.exe";
#endif

        public static string SamplesFolder => @"..\..\..\..\..\Samples";

        [TestMethod]
        [DynamicData(nameof(GetSamples))]
        public void AllSamplesRunAsExpected(string path, TestMetadata md)
        {

            string compiledName = Path.GetFileName(Path.ChangeExtension(path, ".neb"));
            string compiledPath = Path.Combine(Directory.GetCurrentDirectory(), compiledName);

            var options = new Compiler.Options
            {
                EmitProgram = true,
                OutputFolder = ".",
            };

            var references = CreateReferences(Path.GetDirectoryName(path)!, md, out var compiledReferencesPath);

            options.Sources.AddRange(references);         
            options.Sources.Add(SourceCode.From(path));         
            var compileOk = Compiler.Compile(options, out var result);

            WriteReport(result.Report);

            Assert.IsTrue(compileOk);
            Assert.IsTrue(File.Exists(compiledPath));
           
            Assert.IsTrue(compileOk);
            Assert.IsFalse(result.Report.HasErrors);
            
            Assert.AreEqual(md.AbortCode, LaunchExecutor(md, compiledPath, compiledReferencesPath));
            File.Delete(compiledPath);
        }

        private void WriteReport(Commons.Reporting.Report report)
        {
            foreach(var r in report)
            {
                TestContext.WriteLine(r.ToString());
            }
        }

        private static List<SourceCode> CreateReferences(string location, TestMetadata metadata, out string[] compiledPaths)
        {
            var result = new List<SourceCode>();
            var compiledPathsList = new List<string>();

            foreach(var p in metadata.Dependencies)
            {
                string fullPath = Path.Combine(location, p);

                compiledPathsList.Add(Path.ChangeExtension(p, "neb"));
                result.Add(SourceCode.From(fullPath));
            }

            compiledPaths = compiledPathsList.ToArray();
            return result;
        }

        private static int LaunchExecutor(TestMetadata md, string scriptFile, string[] dependencies)
        {
            Process p = new();
            p.StartInfo.FileName = ExecutorPath;      
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.OutputDataReceived += P_OutputDataReceived;
            p.StartInfo.ArgumentList.Add(Path.GetFullPath(scriptFile));

            foreach (var d in dependencies)
            {
                p.StartInfo.ArgumentList.Add(Path.GetFullPath(d));
            }


            p.Start();
            p.BeginOutputReadLine();

            bool exitedOk = p.WaitForExit(md.MaxVMExecutionTime);

            if(!exitedOk)
            {
                p.Kill();
            }

            Assert.IsTrue(exitedOk, "VM Timeout");
            return p.ExitCode;
        }

        private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private static IEnumerable<object[]> GetSamples
        {
            get
            {
                Assert.IsTrue(Directory.Exists(SamplesFolder));
                const string mdFileEx = "meta";

                var metadataFiles = Path.Combine(SamplesFolder, "metadata");
                var allMetdata = Directory.GetFiles(metadataFiles, $"*.{mdFileEx}");
                foreach (var file in Directory.GetFiles(SamplesFolder, "*.nebula"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string? metdataYamlPath = GetMetadataFileFullPath(allMetdata, file);

                    if (metdataYamlPath is null)
                        continue;

                    TestMetadata? md = TestMetadata.Read(metdataYamlPath);

                    Assert.IsNotNull(md);

                    yield return new object[] { file, md };
                }
            }
        }

        private static string? GetMetadataFileFullPath(string[] allMetdata, string nebulaScript)
        {
            var scriptName = Path.GetFileNameWithoutExtension(nebulaScript);

            foreach(var md in allMetdata)
            {
                string mdName = Path.GetFileNameWithoutExtension(md);
                if (scriptName == mdName)
                    return md;
            }

            return null;
        }
    }
}
