#pragma once

#include "IScriptParser.h"

#include "DiagnosticReport.h"
#include "LanguageTypes.h"
#include "InstructionDefs.h"


namespace nebula
{
    class Function;
    class Script;
}


namespace nebula::parsing
{

    // Loads plain text bytecode into a usable script
    class ParserDebug
        : public IScriptParser
    {
    public:
        virtual std::shared_ptr<Script> ParseScript(const std::string_view& data) override;
        virtual shared::DiagnosticReport& GetLastParsingReport() override { return m_Report; }

    private:
        bool ParseNamespace();
        bool ParseFunction();
        bool ParseType(DataStackVariantIndex& result, char stopAt = ' ');
        bool ParseFunctionParameters(Function*);
        bool ParseFunctionAttributes(Function*);
        bool ParseFunctionBody(Function*);
        bool ParseFunctionSpecialData(Function*);
        bool ParseFunctionLocals(Function*);

        bool ParseBundleDefinitions();

        bool ParseInstructionArguments(VMInstruction, std::vector<DataStackVariant>&);

        void SkipWhitespace(bool stopAtEndOfLine = false);
        void SkipLine();
        bool Peek(const std::string_view&);
        bool MatchIfNext(const std::string_view&);
        bool MatchWord(const std::string_view&);

        bool ReadLiteral(std::string&, bool = false);
        bool ReadLiteralUntil(std::string&, char c, bool = false);
        bool ReadInt(TInt32& i);
        bool ReadFloat(TFloat& f);
    };
}

