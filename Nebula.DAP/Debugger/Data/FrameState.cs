using Nebula.Commons.Debugger;
using Nebula.Interop.Structures;
using System.Collections.Generic;
using System.Linq;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class FrameState
    {
        public VirtualMachineState Parent { get; }
        public ThreadState Thread { get; }
        public int FrameId { get; }
        public string FunctionName { get; }
        public string FunctionNamespace { get; }
        public int SourceLine { get; }
        public Frame OriginalFrame { get; }

        public IReadOnlyDictionary<int, ScopeState> Scopes => _scopes;

        private readonly Dictionary<int, ScopeState> _scopes = [];

        public FrameState(VirtualMachineState parent, ThreadState thread, int frameId, Frame frame)
        {
            Parent = parent;
            Thread = thread;
            FrameId = frameId;
            OriginalFrame = frame;
            FunctionName = frame.FunctionName;
            FunctionNamespace = frame.Namespace;
            SourceLine = thread.GetLineNumber(this);

            DebugFunction? dbgFunc = thread.GetDebugInfo(this);
            if (dbgFunc != null)
            {
                ScopeState localsScope = new(parent, "Local variables", true);
                int locCount = frame.LocalCount;
                for (int i = 0; i < locCount; i++)
                {
                    localsScope.Add(dbgFunc.LocalVariables[i], frame.GetLocalVariableAt(i));
                }

                _scopes[localsScope.VarReference] = localsScope;

                ScopeState parametersScope = new(parent, "Parameters", true);
                int paramCount = frame.ParameterCount;
                for (int i = 0; i < paramCount; i++)
                {
                    parametersScope.Add(dbgFunc.Parameters[i], frame.GetParameterVariableAt(i));
                }

                _scopes[parametersScope.VarReference] = parametersScope;
            }

            foreach (ScopeState? scope in _scopes.Values.ToList())
            {
                foreach (IScopeNode variable in scope.Children)
                {
                    if (variable.ValueType == Interop.Enumerators.TypeIdentifier.Bundle)
                    {
                        BundleScopeState bState = (BundleScopeState)variable;
                        _scopes.Add(bState.VarReference, bState);
                    }
                }
            }
        }
    }
}
