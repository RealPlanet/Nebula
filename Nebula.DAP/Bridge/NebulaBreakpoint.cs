namespace Nebula.Debugger.Bridge
{
    public class NebulaBreakpoint
    {

        public string Namespace { get; }
        public string FuncName { get; }
        public int Line { get; }

        public NebulaBreakpoint(string @namespace, string funcName, int line)
        {
            Namespace = @namespace;
            FuncName = funcName;
            Line = line;
        }
    }
}
