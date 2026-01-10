#pragma once

#include <string>
#include <map>

namespace nebula {
	class Script;

	class FunctionDebugInformation {
	public:
		static const size_t NoLineInfo{ (size_t)-1 };

	private:
		std::string m_Name;
		size_t m_LineNumber{ 0 };
		size_t m_EndLineNumber{ 0 };
		size_t m_InstructionCount{ 0 };
		std::map<std::string, size_t> m_LineStartingOpcodeIndex;

	public:
		FunctionDebugInformation(const std::string& funcName, size_t lineNumber, size_t endLineNumber, size_t instructionCount, std::map<std::string, size_t> lineIndicies);

		size_t GetLineFromOpcode(size_t opcode) const;
		const std::string& GetFunctionName() const { return m_Name; }
	};

	class ScriptDebugInformation {
	private:
		std::string m_OriginalFileName;
		std::map<std::string, FunctionDebugInformation> m_Functions;

	public:
		ScriptDebugInformation() = default;
		~ScriptDebugInformation() = default;

		void SetOriginalFileName(const std::string& name) { m_OriginalFileName = name; }
		const std::string& GetOriginalFileName() const { return m_OriginalFileName; }

		void AddFunctionInformation(const FunctionDebugInformation& info) { m_Functions.insert(std::make_pair(info.GetFunctionName(), info)); }
		FunctionDebugInformation* GetFunctionInformation(const std::string& funcName);
	};

	class DebugServer
	{
	private:
		static DebugServer* m_pInstance;

	public:
		virtual ~DebugServer() = default;

		static DebugServer* Instance();
		static void RegisterDebugServer(DebugServer* instance);

		// A debug server might cache something on a per-script basis, when a Script is being deleted we signal it here
		virtual void NotifyScriptUnloaded(const Script* script) = 0;
		virtual ScriptDebugInformation* GetDebugInformationForScript(const Script* script) = 0;
	};
}



