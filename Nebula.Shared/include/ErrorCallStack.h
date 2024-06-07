#include <string>
#include <vector>

#include "Instruction.h"

namespace nebula::shared
{
    class ErrorCallStackLine
    {
    public:
        ErrorCallStackLine(size_t instNumber, const std::string& scriptText);
    
        inline size_t InstructionNumber() const { return m_InstructionNumber; }
        inline const std::string& Text() const { return m_ScriptLine; }
    
    private:
        size_t m_InstructionNumber;
        std::string m_ScriptLine;
    };

    class ErrorCallStack
    {
    public:
        ErrorCallStack() = default;
    
        inline void Append(const ErrorCallStackLine& line) { m_Lines.push_back(line); }
        inline void SetExplanation(InstructionErrorCode errorCode, const std::string& s) { m_ErrorCode = errorCode;  m_preText = s; }
        inline void Clear() { m_Lines.clear(); }
        std::string GetAsText() const;
        InstructionErrorCode GetErrorCode() const { return m_ErrorCode; }
    private:
        std::vector<ErrorCallStackLine> m_Lines;
        std::string m_preText;
        InstructionErrorCode m_ErrorCode;
    };
}