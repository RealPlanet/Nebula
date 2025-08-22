namespace Nebula.Compiler.Tests.Utility
{
    public sealed class TestMetadata
    {
        public int AbortCode { get; set; }

        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public int MaxVMExecutionTime { get; set; }

        public static TestMetadata? Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();

            TestMetadata tm = deserializer.Deserialize<TestMetadata>(File.ReadAllText(filePath));

            tm.Dependencies ??= Array.Empty<string>();
            if (tm.MaxVMExecutionTime <= 1000)
            {
                tm.MaxVMExecutionTime = 1000;
            }

            return tm;
        }
    }
}
