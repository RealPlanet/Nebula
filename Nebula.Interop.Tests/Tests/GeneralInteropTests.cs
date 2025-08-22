using Nebula.Interop.SafeHandles;

namespace Nebula.Interop.Tests.Tests
{
    [TestClass]
    public class GeneralInteropTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public void ScriptLoadsCorrectly()
        {
            bool loadOk = Script.FromFile("hello_world.neb", OnReportMessage, out Script script);
            Assert.IsTrue(loadOk);
            Assert.AreEqual(script.Namespace, "hello_world");
            script.Dispose();
        }

        [TestMethod]
        public void VirtualMachineInteropTest()
        {
            VirtualMachine vm = new();
            bool wroteHellWorld = false;
            void onVmWriteLine(string message)
            {
                TestContext.WriteLine(message);
                wroteHellWorld = message.Trim() == "Hello, World!";
            }

            void onVmWrite(string message)
            {
                TestContext.Write(message);
            }

            try
            {
                vm.LoadNativesFromDll("Nebula.StandardLib.dll");
                vm.RedirectStdOutput(onVmWrite, onVmWriteLine);
                bool addOk = vm.AddScripts(["hello_world.neb"], OnReportMessage);
                Assert.IsTrue(addOk);

                vm.Initialize(false);
                vm.Run();

                Assert.IsTrue(wroteHellWorld);
            }
            finally
            {
                vm.Dispose();
            }
        }

        private void OnReportMessage(string scriptPath, ReportType type, string message)
        {
            TestContext.WriteLine($"[{type}] - {message}");
        }
    }
}
