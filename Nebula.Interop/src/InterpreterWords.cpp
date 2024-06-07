#include "InterpreterWords.h"
#include "EmitTokens.h"
#include "Utility.h"
#include "InstructionDefs.h"
using namespace nebula::shared;
using namespace nebula;

String^ Nebula::CodeEmitter::InterpreterWords::GetScriptSectionName(ScriptSection type, bool withPrefix)
{
    const char* sectionName = ReadableScriptSection((nebula::ScriptSection)type);
    String^ suffix = gcnew String(sectionName);

    if (withPrefix)
    {
        String^ prefix = GetTokenChar(TokenType::MarkerPrefix);
        return prefix + suffix;
    }

    return suffix;
}

String^ Nebula::CodeEmitter::InterpreterWords::GetTokenChar(TokenType type)
{
    switch (type)
    {
    case TokenType::OpenBody:
        return gcnew String(EMIT_OPENBODY_TOKEN);
    case TokenType::CloseBody:
        return gcnew String(EMIT_CLOSEBODY_TOKEN);
    case TokenType::AttributePrefix:
        return gcnew String(ATTRIBUTE_PREFIX);
    case TokenType::OpenParenthesis:
        return gcnew String(OPEN_PARENTHESIS);
    case TokenType::ClosedParenthesis:
        return gcnew String(CLOSE_PARENTHESIS);
    case TokenType::MarkerPrefix:
        return gcnew String(GENERIC_MARKER_PREFIX);
    case TokenType::CompiledComment:
        return gcnew String(COMMENT_PREFIX);
    }

    throw gcnew System::Exception("Unknown marker");
}