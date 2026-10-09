#pragma once

#ifndef _H_DEBUG_STATE_
#define _H_DEBUG_STATE_

#include "Interpreter.h"

#include "DebugSymbols.h"
#include "DebuggerEntities.h"

#include <vector>
#include <optional>

namespace nebula::debugger
{
	class DebugState
	{
	public:
		DebugState(Interpreter* interpreter);

		const Source* CreateSource(const symbols::DebugSymbols* information);
		std::optional<Source> RemoveSource(const std::string& namespace_);
		void ClearSources();

		const std::unordered_map<std::string, Source>& GetSources() const;
		const Source* GetSource(const std::string& namespace_) const;
		const Source* GetSource(size_t reference) const;
		const Source* GetSourceByPath(const std::string& path) const;

		const std::vector<Thread>& GetThreads();
		const Thread* GetThread(ThreadId id) const;
		// Fetches the frames of a specific thread in their reversed order
		const std::vector<Frame>& GetFrames(ThreadId id, size_t requestedFrames);
		const Frame* GetFrameById(FrameId frameId) const;

		const std::vector<GenericScope>& GetScopes(const Frame* frame);
		const GlobalScope GetGlobalScope() const;

		std::vector<Value>& GetValues(size_t refence);
		void PopulateChildValues(Value& variable);

		size_t GetNextSourceReference();
		size_t GetNextFrameReference();
		size_t GetNextVariableReference();

		// Remote debugging is defined as debugging through the attach request
		bool IsRemoteDebugging() const;
		void SetRemoteDebugging(bool v);

		void InvalidateState();

	private:
		void DeclareVariable(VariableId reference, nebula::Value& variable, const nebula::debugger::symbols::DebugSymbols* debugSymbols, const symbols::ValueInformation& debugVariable);

		Interpreter* m_interpreter;

		size_t m_sourceReference = 0;
		size_t m_frameReference = 0;
		size_t m_variableReference = 0;

		bool m_isRemoteDebugging{ false };

		// Data that should be reset between steps
		std::vector<debugger::Thread> m_threads;
		std::unordered_map<ThreadId, std::vector<debugger::Frame>> m_frames;
		std::unordered_map<FrameId, debugger::Frame*> m_framesById;

		std::unordered_map<FrameId, std::vector<GenericScope>> m_scopes;
		GlobalScope m_globalScope;
		std::unordered_map<VariableId, std::vector<Value>> m_values;

		std::unordered_map<std::string /* namespace */, Source> m_cachedSources;
	};
} // namespace nebula::debugger

#endif // !_H_DEBUG_EXECUTION_STATE_
