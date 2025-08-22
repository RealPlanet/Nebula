namespace Nebula.Debugger.DAP
{
    public static class DebuggerConstants
    {
        public const string JustMyCodeStepping = nameof(JustMyCodeStepping);
        public const string DisableJITOptimization = nameof(DisableJITOptimization);
        public const string WarnIfNoUserCodeOnLaunch = nameof(WarnIfNoUserCodeOnLaunch);
        public const string EnableStepFiltering = nameof(EnableStepFiltering);

        // Debugger configurations
        public const string RecompileOnLaunch = nameof(RecompileOnLaunch);
        public const string CompilerPath = nameof(CompilerPath);
        public const string Workspace = nameof(Workspace);
        public const string BindingLookupPath = nameof(BindingLookupPath);
        public const string AdditionalScriptFolders = nameof(AdditionalScriptFolders);
        public const string StepOnEntry = nameof(StepOnEntry);
    }
}
