namespace Nebula.CodeGeneration
{
    public record Version(int A, int B, int C)
    {
        public override string ToString() => $"{A}.{B}.{C}";
    }
}
