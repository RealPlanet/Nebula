namespace Nebula.Debugger.Debugger.Data
{
    public static class StateExtensions
    {
        public static int GetNextFrameId(this ThreadState t)
        {
            return t.Parent.GetNextFrameId();
        }

        public static int GetNextScopeId(this FrameState t)
        {
            return t.Parent.Parent.GetNextScopeId();
        }
    }
}
