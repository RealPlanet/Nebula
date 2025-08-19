using Nebula.Commons.Debugger;
using Nebula.Interop.Structures;
using System;
using System.Collections.Generic;

namespace Nebula.Debugger.Debugger.Data
{
    public sealed class FrameState
    {
        public ThreadState Parent { get; }
        public int FrameId { get; }
        public string FunctionName { get; }
        public string FunctionNamespace { get; }
        public int SourceLine { get; }
        public Frame OriginalFrame { get; }

        public IReadOnlyDictionary<int, ScopeState> Scopes => _scopes;

        private readonly Dictionary<int, ScopeState> _scopes = [];

        public FrameState(ThreadState parent, int frameId, Frame frame)
        {
            Parent = parent;
            FrameId = frameId;
            OriginalFrame = frame;
            FunctionName = frame.FunctionName;
            FunctionNamespace = frame.Namespace;
            SourceLine = parent.GetLineNumber(this);

            DebugFunction? dbgFunc = parent.GetDebugInfo(this);
            if (dbgFunc != null)
            {
                ScopeState localsScope = new(this, "Local variables");
                int locCount = frame.LocalCount;
                for (int i = 0; i < locCount; i++)
                {
                    string varName = dbgFunc.LocalVariables[i].Name;
                    localsScope.Add(varName, frame.GetLocalVariableAt(i));
                }

                _scopes[localsScope.ScopeId] = localsScope;

                ScopeState parametersScope = new(this, "Parameters");
                int paramCount = frame.ParameterCount;
                for (int i = 0; i < paramCount; i++)
                {
                    string varName = dbgFunc.Parameters[i].Name;
                    parametersScope.Add(varName, frame.GetParameterVariableAt(i));
                }

                _scopes[parametersScope.ScopeId] = parametersScope;
            }
        }
    }
}
