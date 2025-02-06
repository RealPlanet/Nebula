#include "interfaces\IScriptParser.h"

#include <cwctype>
#include <string>
#include <format>

using namespace nebula;

bool IScriptParser::ReadString(std::string& out, size_t maxIndex)
{
    if (Current() != '\"')
    {
        return false;
    }
    m_CurrentDataIndex++;

    // TODO error if string contains unescaped new line
    while (Current() != '\"')
    {
        out += Current();
        m_CurrentDataIndex++;
        if (maxIndex > 0 && m_CurrentDataIndex >= maxIndex)
            return false;// Terminator not found
    }

    if (Current() != '\"')
    {
        return false;
    }

    m_CurrentDataIndex++;
    return true;
}

char IScriptParser::Current()
{
    if (m_CurrentDataIndex >= m_CurrentData.size())
        return 0;

    return m_CurrentData[m_CurrentDataIndex];
}

char IScriptParser::Next()
{
    char c = Current();
    m_CurrentDataIndex++;
    return c;
}

void IScriptParser::ReportErrorWhileParsingBundle(std::string_view bundleName)
{
    m_Report.ReportError(std::format("Error while parsing bundle '{}', aborting!", bundleName));
}

void IScriptParser::ReportExpectedNumberAt(size_t charIndex)
{
    m_Report.ReportError(std::format("Expected digit at '{}' but found '{}'", charIndex, m_CurrentData[charIndex]));
}

void IScriptParser::ReportUnexpectedSection(size_t charIndex)
{
    std::string weirdName;
    // So we dont move the actual iterator
    size_t offset = m_CurrentDataIndex;
    while (offset < m_CurrentData.size() && !std::iswspace(m_CurrentData[offset]))
    {
        weirdName += m_CurrentData[offset];
        offset++;
    }

    m_Report.ReportError(std::format("Unexptected script section at ({}) named '{}'.", charIndex, weirdName));
}

void IScriptParser::ReportExpectedString(size_t charIndex, const std::string& reason)
{
    m_Report.ReportError(std::format("Expected \" at '{}' because: {}.", charIndex, reason));
}

void IScriptParser::ReportCouldNotParseNameSpace(size_t charIndex)
{
    m_Report.ReportError(std::format("Could not parse namespace definition at '{}'.", charIndex));
}

void IScriptParser::ReportUnknownAttribute(const std::string_view parsedAttribute, size_t charIndex)
{
    m_Report.ReportError(std::format("Literal at '{}' is not a valid attribute, parsed: '{}'.", charIndex, parsedAttribute));
}

void IScriptParser::ReportUnexpectedCharacter(size_t charIndex, std::string_view expected)
{
    m_Report.ReportError(std::format("Expected '{}' at '{}' but found '{}'", expected.data(), charIndex, m_CurrentData[charIndex]));
}