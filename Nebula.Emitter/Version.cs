namespace Nebula.CodeEmitter
{
    public record Version(int A, int B, int C)
    {
        public override string ToString() => $"{A}.{B}.{C}";
    }
}
