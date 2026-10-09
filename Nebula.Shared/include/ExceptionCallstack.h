#pragma once

#ifndef _H_NEBULA_EXCEPTION_CALLSTACK_
#define _H_NEBULA_EXCEPTION_CALLSTACK_

#include <string>
#include <vector>

#include "Instruction.h"

namespace nebula::shared
{
	class ExceptionCallstackLine
	{
	public:
		ExceptionCallstackLine(const std::string& scriptSource,
								const std::string& namespace_,
								const std::string& funcName,
								size_t instNumber,
								const std::string& scriptText);

		inline size_t GetInstructionNumber() const
		{
			return m_InstructionNumber;
		}

		inline const std::string& GetScriptSource() const
		{
			return m_ScriptSource;
		}

		inline const std::string& GetFunctionName() const
		{
			return m_FunctionName;
		}

		inline const std::string& GetFunctionNamespace() const
		{
			return m_Namespace;
		}

		inline const std::string& GetText() const
		{
			return m_ScriptLine;
		}

	private:
		const std::string m_ScriptSource;
		const std::string m_FunctionName;
		const std::string m_Namespace;
		size_t m_InstructionNumber;
		// Readable script line
		std::string m_ScriptLine;
	};

	class ExceptionCallstack
	{
	public:
		ExceptionCallstack() = default;

		inline void Append(const ExceptionCallstackLine& line)
		{
			m_Lines.push_back(line);
		}
		inline void SetExplanation(InstructionErrorCode errorCode, const std::string& s)
		{
			m_ErrorCode = errorCode;
			m_preText = s;
		}

		std::string GetReadableError() const;
		std::string GetAsText() const;
		std::string GetBytecodeErrorString(size_t lineIndex) const;

		inline InstructionErrorCode GetErrorCode() const
		{
			return m_ErrorCode;
		}

		inline size_t GetLineCount() const
		{
			return m_Lines.size();
		}

		inline ExceptionCallstackLine& LineAt(size_t index)
		{
			return m_Lines.at(index);
		}

		inline const ExceptionCallstackLine& LineAt(size_t index) const
		{
			return m_Lines.at(index);
		}

		inline void Clear()
		{
			m_Lines.clear();
		}

	private:
		std::vector<ExceptionCallstackLine> m_Lines;
		std::string m_preText;
		InstructionErrorCode m_ErrorCode{ InstructionErrorCode::Unknown };

		bool GetExceptionCallstackLineFromDbgFile(const ExceptionCallstackLine& line, std::string* outTextLine) const;
	};
} // namespace nebula::shared

#endif // !_H_NEBULA_EXCEPTION_CALLSTACK_
