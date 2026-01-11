#include "LiteralScriptParser.h"

#include <cassert>
#include <cwctype>
#include <format>

#include "Script.h"
#include "Function.h"

#include "InstructionRegistry.h"

using namespace nebula;
using namespace nebula::parsing;

Script* ParserDebug::ParseScript(const std::string_view& data)
{
    constexpr const char* namespaceKeyword = ReadableScriptSection(ScriptSection::Namespace);
    constexpr const char* bundleKeyword = ReadableScriptSection(ScriptSection::Bundle);
    constexpr const char* funcKeyword = ReadableScriptSection(ScriptSection::Function);

    m_ScriptBuilder = new ScriptBuilder();
    m_CurrentData = data;

    SkipWhitespace();

    // Flase if an error happened
    bool continueParsing = true;
    while (continueParsing && m_CurrentDataIndex < data.size())
    {
        SkipWhitespace();

        char c = Current();
        switch (c)
        {
        case '#':
        {
            SkipLine();
            break;
        }
        case '.':
        {
            size_t start = m_CurrentDataIndex;
            m_CurrentDataIndex++;
            if (MatchIfNext(namespaceKeyword))
            {
                if (m_ScriptBuilder->HasNamespace()) {
                    // ReportNamespaceAlreadySet
                    continueParsing = false;
                    break;
                }

                continueParsing = ParseNamespace();
                break;
            }

            if (MatchIfNext(bundleKeyword))
            {
                continueParsing = ParseBundleDefinitions();
                break;
            }

            if (MatchIfNext(funcKeyword))
            {
                continueParsing = ParseFunction();
                break;
            }

            ReportUnexpectedSection(start);
            continueParsing = false;
            break;
        }

        case 0:
        {
            continueParsing = false;
            break;
        }
        default:
            m_Report.ReportError("Unexpected value! Aborting parse!");
            continueParsing = false;
        }
    }


    if (m_Report.Errors().size() > 0)
    {
        delete m_ScriptBuilder;
        return nullptr;
    }

    Script* script = m_ScriptBuilder->Finalize();
    delete m_ScriptBuilder;

    return script;
}

bool ParserDebug::ParseNamespace()
{
    SkipWhitespace();

    std::string ns;
    if (!ReadString(ns)) {
        ReportCouldNotParseNameSpace(m_CurrentDataIndex);
        return false;
    }
 
    return m_ScriptBuilder->SetNamespace(ns);
}

bool ParserDebug::ParseFunction()
{
    DataStackVariantIndex returnType;
    if (!ParseType(returnType))
        return false;

    std::string funcName;
    if (!ReadLiteralUntil(funcName, '(')) {
        m_Report.ReportError("Could not read function name.");
        return false;
    }

    Function newFunc{ m_ScriptBuilder->Get(), returnType, funcName };

    if (!ParseFunctionParameters(&newFunc))
    {
        m_Report.ReportError("Could not parse function parameters.");
        return false;
    }

    if (!ParseFunctionAttributes(&newFunc))
    {
        m_Report.ReportError("Could not parse function attributes.");
        return false;
    }

    if (!ParseFunctionBody(&newFunc))
    {
        m_Report.ReportError("Could not parse function body.");
        return false;
    }

    return m_ScriptBuilder->AddFunction(std::move(newFunc));
}

bool ParserDebug::ParseType(DataStackVariantIndex& result, char stopAt)
{
    std::string strType;
    if (!ReadLiteralUntil(strType, stopAt, true)) {
        m_Report.ReportError("Could not read type identifier.");
        return false;
    }

    result = StringToStackValue(strType);
    if (result == _UnknownType)
    {
        m_Report.ReportError(std::format("Unknown type '{}'.", strType));
        return false;
    }

    return true;
}

bool ParserDebug::ParseFunctionParameters(Function* newFunc)
{
    if (!MatchWord("("))
    {
        m_Report.ReportError(std::format("Could not find open parameter list marker for function '{}'!", newFunc->Name()));
        return false;
    }

    SkipWhitespace(true);
    while (Current() != ')' && Current() != '\n' && Current() != '\r')
    {
        DataStackVariantIndex type;
        if (!ParseType(type))
            return false;

        std::string paramName;
        ReadLiteral(paramName, true);

        newFunc->AddParameter(type);

        if (Peek(","))
            MatchWord(",");

        SkipWhitespace(true);
    }

    if (!MatchWord(")"))
    {
        m_Report.ReportError(std::format("Could not find open parameter list marker for function '{}'!", newFunc->Name()));
        return false;
    }

    return true;
}

bool ParserDebug::ParseFunctionAttributes(Function* newFunc)
{
    assert(newFunc);

    // One or more attributes present?
    while (MatchIfNext(";"))
    {
        std::string out;
        if (!ReadLiteral(out)) {
            return false;
        }

        VMAttribute attr;
        if (!stoattr(out.data(), attr)) {
            ReportUnknownAttribute(out, m_CurrentDataIndex - out.size());
            return false;
        }

        newFunc->AddAttribute(attr);
    }
    return true;
}

bool ParserDebug::ParseFunctionBody(Function* newFunc)
{
    if (!MatchWord("{")) {
        m_Report.ReportError(std::format("Could not find open function body marker for function '{}'!", newFunc->Name()));
        return false;
    }

    if (!ParseFunctionSpecialData(newFunc))
    {
        m_Report.ReportError(std::format("Could not parse special data for function '{}'!", newFunc->Name()));
        return false;
    }

    while (Current() != '}')
    {
        // Each line is an instruction
        std::string num;
        ReadLiteral(num);

        if (num.starts_with('#'))
        {
            SkipLine();
            continue;
        }

        std::string instName;
        ReadLiteral(instName);

        VMInstruction opcode;
        stoi(instName, opcode);

        InstructionArguments args;
        if (!ParseInstructionArguments(opcode, args))
        {
            m_Report.ReportError(std::format("Could not parse instruction arguments for function '{}'!", newFunc->Name()));
            return false;
        }

        newFunc->AppendInstruction(std::make_pair(opcode, args));

        m_CurrentDataIndex++;
        SkipWhitespace();
    }
    // Read body!

    if (!MatchWord("}"))
    {
        m_Report.ReportError(std::format("Could not find close function body marker for function{}!", newFunc->Name()));
        return false;
    }


    return true;
}

bool ParserDebug::ParseFunctionSpecialData(Function* newFunc)
{
    while (MatchIfNext("."))
    {
        std::string specialDataMarker;
        if (!ReadLiteral(specialDataMarker))
        {
            return false;
        }

        SpecialDataType type = StringToSpecialDataType(specialDataMarker);

        switch (type)
        {
        case SpecialDataType::Locals:
        {
            return ParseFunctionLocals(newFunc);
        }
        default:
            m_Report.ReportError(std::format("Uknown special data type '{}'.", specialDataMarker));
            return false;
        }
    }

    return true;
}

bool ParserDebug::ParseFunctionLocals(Function* newFunc)
{
    if (!MatchWord("["))
    {
        m_Report.ReportError("Missing [ for locals definition for function");
        return false;
    }

    SkipWhitespace(true);
    while (Current() != ']' && Current() != '\n' && Current() != '\r')
    {

        DataStackVariantIndex index;
        if (!ParseType(index, ','))
        {
            return false;
        }

        newFunc->AddLocalVariable(index);

        if (Peek(","))
            MatchWord(",");

        SkipWhitespace(true);
    }

    if (!MatchWord("]"))
    {
        m_Report.ReportError("Missing [ for locals definition for function");
        return false;
    }

    return true;
}

bool ParserDebug::ParseBundleDefinitions()
{
    std::string bundleName;
    if (!ReadLiteralUntil(bundleName, '(', true))
    {
        ReportUnexpectedCharacter(m_CurrentDataIndex, "(");
        return false;
    }

    if (!MatchWord("("))
    {
        ReportUnexpectedCharacter(m_CurrentDataIndex, "(");
        return false;
    }

    SkipWhitespace(true);

    BundleDefinition newBundle(bundleName);
    // Parse all fields
    while (Current() != ')')
    {
        DataStackVariantIndex fieldType;
        std::string fieldName;
        if (!ParseType(fieldType))
        {
            ReportUnexpectedCharacter(m_CurrentDataIndex, "type - " + fieldType);
            return false;
        }

        if (!ReadLiteralUntil(fieldName, ' ', true))
        {
            ReportUnexpectedCharacter(m_CurrentDataIndex, "literal");
            return false;
        }

        BundleFieldDefinition field(fieldName, (DataStackVariantIndex)fieldType);
        newBundle.AddField(field);

        // Peek skips whitespace
        if (Peek(","))
            m_CurrentDataIndex++;
    }

    if (!MatchWord(")")) {
        ReportUnexpectedCharacter(m_CurrentDataIndex, ")");
        return false;
    }

    m_ScriptBuilder->AddBundle(std::move(newBundle));
    return true;
}

bool ParserDebug::ParseInstructionArguments(VMInstruction opcode, InstructionArguments& out)
{
    // Read entire line
    std::vector<std::string> litArguments;
    SkipWhitespace(true);
    while (Current() != '\n' && Current() != '\r')
    {
        std::string lit;
        if (Peek("\""))
        {
            ReadString(lit);
        }
        else
        {
            ReadLiteral(lit, true);
        }


        litArguments.push_back(lit);
    }

    out = GenerateArgumentsForOpcode(opcode, litArguments);
    return true;
}

void ParserDebug::SkipWhitespace(bool stopAtEndOfLine)
{
    while (m_CurrentDataIndex < m_CurrentData.size())
    {
        char c = Current();
        if (!std::iswspace(c))
        {
            break;
        }

        if (stopAtEndOfLine &&
            (c == '\n' || c == '\r'))
        {
            break;
        }

        m_CurrentDataIndex++;
    }
}

void ParserDebug::SkipLine()
{
    while (m_CurrentDataIndex < m_CurrentData.size())
    {
        char c = Current();
        if (c == '\n' || c == '\r')
            break;

        m_CurrentDataIndex++;
    }
}

bool ParserDebug::Peek(const std::string_view& c)
{
    SkipWhitespace();
    return m_CurrentData.find(c.data(), m_CurrentDataIndex, c.size()) == m_CurrentDataIndex;
}

bool ParserDebug::MatchIfNext(const std::string_view& word)
{
    SkipWhitespace();
    if (m_CurrentData.find(word.data(), m_CurrentDataIndex, word.size()) == m_CurrentDataIndex) {
        m_CurrentDataIndex += word.size();
        return true;
    }
    return false;
}

bool ParserDebug::MatchWord(const std::string_view& word)
{
    SkipWhitespace();

    if (m_CurrentData.find(word.data(), m_CurrentDataIndex, word.size()) == m_CurrentDataIndex) {
        m_CurrentDataIndex += word.size();
        return true;
    }

    ReportUnexpectedCharacter(m_CurrentDataIndex, word);
    return false;
}

bool ParserDebug::ReadLiteral(std::string& out, bool stopAtNewline)
{
    SkipWhitespace(stopAtNewline);
    while (!std::iswspace(Current()))
    {
        out += Current();
        m_CurrentDataIndex++;
    }

    return out.size() > 0;
}

bool ParserDebug::ReadLiteralUntil(std::string& out, char c, bool stopAtNewline)
{
    SkipWhitespace(stopAtNewline);
    while (!std::iswspace(Current()) && Current() != c)
    {
        out += Current();
        m_CurrentDataIndex++;
    }

    return out.size() > 0;
}

bool ParserDebug::ReadInt(TInt32& i)
{
    SkipWhitespace();
    if (!std::isdigit(Current())) {
        ReportExpectedNumberAt(m_CurrentDataIndex);
        return false;
    }

    std::string strNum;
    while (std::isdigit(Current())) {
        strNum = Current();
        m_CurrentDataIndex++;
    }

    i = std::stoi(strNum);
    return true;
}

bool ParserDebug::ReadFloat(TFloat&)
{
    return false;
}

