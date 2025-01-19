namespace Nebula.Debugger.Bridge.Objects
{
    public abstract class BaseNebulaDebuggerObejct
        : INebulaDebuggerObject
    {
        public StateInformation Parent { get; }

        public BaseNebulaDebuggerObejct(StateInformation parent)
        {
            Parent = parent;
        }
    }
}
