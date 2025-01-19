#pragma once

namespace Nebula::Interop
{
    /// <summary>
    /// Wrapper interface to log interop information without a specific implementation
    /// </summary>
    public interface class LoggerW
    {
    public:
        void LogInformation(System::String^ message);
        void LogWarning(System::String^ message);
        void LogError(System::String^ message);
    };
}

