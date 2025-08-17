using Nebula.Commons.Debugger;
using Nebula.Interop.Structures;
using System.Collections.Generic;

namespace Nebula.Debugger.Bridge.Objects
{
    public class NebulaStackFrame
        : BaseNebulaDebuggerObejct
    {
        /// <summary> Unique ID across all threads </summary>
        public int FrameId { get; }
        public Frame NativeFrame { get; }
        public IList<NebulaScope> Scopes { get; }

        public NebulaStackFrame(StateInformation parent, Frame nativeFrame)
            : base(parent)
        {
            FrameId = parent.FrameIdCounter++;
            NativeFrame = nativeFrame;
            Scopes = ReloadScopes();
        }

        private IList<NebulaScope> ReloadScopes()
        {
            // Currently there are only locas and parameters, a scope in nebula is a function view but here it needs to be separated

            NebulaDebugger dbg = Parent.Parent;

            DebugFunction? currentFrameData = dbg.GetDebugInfo(this);

            NebulaScope localsScope = new(Parent, "Local variables", this);
            for (int i = 0; i < NativeFrame.LocalCount; i++)
            {
                string varName = currentFrameData.LocalVariables[i].Name;
                localsScope.AddVariable(varName, NativeFrame.GetLocalVariableAt(i));
            }


            NebulaScope parametersScope = new(Parent, "Parameters", this);
            for (int i = 0; i < NativeFrame.ParameterCount; i++)
            {
                string varName = currentFrameData.Parameters[i].Name;
                parametersScope.AddVariable(varName, NativeFrame.GetParameterVariableAt(i));
            }

            return [localsScope, parametersScope];
        }
    }
}
