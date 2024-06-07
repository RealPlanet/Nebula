#pragma once
#include "InstructionDefs.h"

using namespace System;

namespace Nebula::CodeEmitter
{
    public enum class TokenType
    {
        MarkerPrefix,
        AttributePrefix,
        OpenBody,
        CloseBody,
        OpenParenthesis,
        ClosedParenthesis,
        CompiledComment,
    };

    public enum class ScriptSection
    {
        Namespace = nebula::ScriptSection::Namespace,
        Function = nebula::ScriptSection::Function,
        Bundle = nebula::ScriptSection::Bundle,
        Locals = nebula::ScriptSection::Locals,
    };

    public ref class InterpreterWords
    {
    public:
        static String^ GetTokenChar(TokenType type);
        static String^ GetScriptSectionName(ScriptSection type, bool withPrefix);
    };
}