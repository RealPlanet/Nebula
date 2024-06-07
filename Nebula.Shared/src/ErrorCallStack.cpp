#include "ErrorCallStack.h"
#include <sstream>
#include "Utility.h"

using namespace nebula::shared;

ErrorCallStackLine::ErrorCallStackLine(size_t instNumber, const std::string& scriptText)
    : m_InstructionNumber{ instNumber }, m_ScriptLine{ scriptText }
{
}

std::string ErrorCallStack::GetAsText() const
{
    std::stringstream ss;

    ss << "~~~~~~ Interpreter fatal error report: ~~~~~~\n";

    if (m_preText != "")
    {
        ss << "Error description: ";
        ss << m_preText << "\n";
    }

    const char* rootPrefix = "Root: ";
    size_t rootPrefixCount = cstrlen(rootPrefix);

    ss << "See callstack for error location: \n";
    for (size_t i{0}; i < m_Lines.size(); i++)
    {
        auto& line = m_Lines[i];

        std::string labelCountPrefix = std::format("+ @{}", line.InstructionNumber());

        if (i == m_Lines.size() - 1)
        {
            // Root of call stack
            ss << rootPrefix << labelCountPrefix << "::" << line.Text() << "" << "\n";
            continue;
        }

        std::string strLine = std::format("{}::{}\n", labelCountPrefix, line.Text());
        strLine.insert(0, rootPrefixCount, ' ');
        ss << strLine;
    }
    ss << "~~~~~~ End of Error ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n";

    return ss.str();
}
