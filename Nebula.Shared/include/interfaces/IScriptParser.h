#pragma once

#include <string>
#include <string_view>

#include "DiagnosticReport.h"

namespace nebula
{
	class Script;
	class ScriptBuilder;

	// Base class for all script parsing facilitites
	class IScriptParser
	{
	public:
		virtual ~IScriptParser() = default;
		virtual Script* ParseScript(const std::string_view& data) = 0;
		virtual shared::DiagnosticReport& GetLastParsingReport() = 0;

	protected:
		bool ReadString(std::string& out, size_t maxIndex = 0);

		// Does not skip whitespace, returns next character in data
		char Current();
		// Does not skip whitespace, returns next character in data and moves iterator forward by one
		char Next();
		void ReportErrorWhileParsingBundle(std::string_view);

		void ReportExpectedNumberAt(size_t);
		void ReportUnexpectedSection(size_t);
		void ReportExpectedString(size_t, const std::string&);
		void ReportUnexpectedCharacter(size_t, std::string_view);
		void ReportCouldNotParseNameSpace(size_t);
		void ReportUnknownAttribute(const std::string_view, size_t);
		shared::DiagnosticReport m_Report{};

		std::string_view m_CurrentData;
		size_t m_CurrentDataIndex{ 0 };
		ScriptBuilder* m_ScriptBuilder{ nullptr };
	};
}

