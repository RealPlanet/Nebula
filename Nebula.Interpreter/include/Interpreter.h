#pragma once

#include <atomic>
#include <chrono>
#include <functional>

#include "ThreadMap.h"
#include "LanguageTypes.h"

#include "Frame.h"
#include "Script.h"
#include "Instruction.h"
#include "ErrorCallStack.h"
#include "InterpreterMemory.h"

namespace nebula
{
	// Function pointer definition to bind Nebula function calls to C++ calls
	using NativeFunctionDelegate = ::std::function<InstructionErrorCode(Interpreter*, Frame*)>;
	using InterpreterExitDelegate = std::function<void()>;

	using NativeFunctionCallbackPtr = InstructionErrorCode(*)(Interpreter*, Frame*);
	using ScriptMap = std::map<const std::string, std::shared_ptr<Script>>;

	class IStreamWrapper;

	// Core of the virtual machine
	class Interpreter
	{
		friend InstructionErrorCode ExecuteInstruction(VMInstruction, Interpreter*, Frame*, const InstructionArguments&);

	public:
		enum State
		{
			Abort,
			Running,
			Paused,
			Exited,
		};

	public:
		Interpreter();
		~Interpreter();
		Interpreter(const Interpreter&) = delete;
		Interpreter(Interpreter&&) = delete;

	public:
		State GetState() const { return m_CurrentState; }
		State Wait() { m_running.wait(true); return GetState(); }
		State InitAndRun(bool startPaused = false) { Init(startPaused); return Run(); }
		State Init(bool startPaused = false);
		State Run();
		State Pause();
		State Stop();
		State Resume();
		void Reset();

		// Native functions are responsible to fetch the data from the parent data stack
		bool BindNativeFunction(const std::string& name, const NativeFunctionDelegate callback);
		bool BindTypeFunction(const std::string& name, DataStackVariantIndex type, const NativeFunctionDelegate callback);
		bool AddScript(std::shared_ptr<Script> script);
		bool SetStandardOutput(IStreamWrapper* stream);
		// Returns the previous standard output and sets the new one
		IStreamWrapper* SwapStandardOutput(IStreamWrapper* stream);
		bool SetExitCallback(InterpreterExitDelegate callbackPtr);
		bool ClearStandardOutput();

		bool Step();

		// Getters
	public:
		const size_t GetCurrentThreadId() const { return m_CurrentThreadIndex; }
		const ThreadMap& GetThreads() const { return m_Threads; }
		CallStack& GetThread(size_t index) { return m_Threads.At(index); }
		const CallStack& GetThread(size_t index) const { return m_Threads.At(index); }

		IStreamWrapper* StandardOutput() { return m_pStandardOutput; }
		shared::ErrorCallStack* GetFatalErrorCallstack() { return m_LastErrorCallstack; }

		const ScriptMap& GetLoadedScripts() const { return m_Scripts; }
	private:
		bool CheckAndSetExitState();

		void SetState(State);
		bool ShouldScheduleNewFrame();
		void SwapExecutingThread();
		void CreateFrameOnStack(const Function*, bool);
		void BuildErrorStack(Frame*);
		std::string BuildGuiltyInstructionLineForCallStack(Frame*);

		const Function* GetFunction(const std::string&, const std::string&) const;
		const BundleDefinition* GetBundleDefinition(const std::string&, const std::string&) const;
		const NativeFunctionDelegate* GetNativeFunction(const std::string&) const;
		const NativeFunctionDelegate* GetTypeFunction(DataStackVariantIndex, const std::string&) const;

		/// <summary>
		/// Thread map can be re-allocated at any time, do not store a ptr to the current callstack
		/// </summary>
		/// <returns></returns>
		CallStack* GetCurrentCallstack();
	private:
		std::map<const std::string, NativeFunctionDelegate> m_NativeFunctions{};
		std::map<const DataStackVariantIndex, std::map < const std::string, NativeFunctionDelegate>> m_TypeNativeFunctions;

		ScriptMap m_Scripts{};
		ThreadMap m_Threads{};
		size_t m_CurrentThreadIndex{ 0 };

		State m_CurrentState{ State::Paused };
		bool m_StartedOnce{ false };
		std::atomic_flag m_running = ATOMIC_FLAG_INIT;
		std::atomic_flag m_paused = ATOMIC_FLAG_INIT;
		shared::ErrorCallStack* m_LastErrorCallstack;

		int m_MaxExecutionTime = 10;// Milliseconds
		std::chrono::steady_clock::time_point m_LastSchedulingUpdate{};

		IStreamWrapper* m_pStandardOutput;
		InterpreterExitDelegate m_fExitCallback;
		InterpreterMemory m_Memory;
	};
}