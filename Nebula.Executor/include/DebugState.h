#pragma once

#ifndef _H_DEBUG_STATE_
#define _H_DEBUG_STATE_

#include "DebugFile.h"

#include "DebugController.h"
#include "DebugTypes.h"
#include "DAPTypes.h"

#include <vector>
#include <optional>
#include <tuple>

namespace nebula::debugger
{
	class DebugState
	{
	public:
		DebugState(DebugController* interpreter);

		const DebugSource* CreateSource(const symbols::DebugFile* information);
		std::optional<DebugSource> RemoveSource(const std::string& namespace_);
		void ClearSources();

		const std::unordered_map<std::string, DebugSource>& GetSources() const;
		const DebugSource* GetSource(const std::string& namespace_) const;
		const DebugSource* GetSource(size_t reference) const;
		const DebugSource* GetSourceByPath(const std::string& path) const;

		const std::vector<DebugThread>& GetThreads();
		const DebugThread* GetThread(ThreadId id) const;
		// Fetches the frames of a specific thread in their reversed order
		const std::vector<DebugFrame>& GetFramesOfThread(ThreadId id, size_t requestedFrames);
		const DebugFrame* GetFrameById(FrameId frameId) const;

		const std::vector<DebugScope>& GetScopes(const DebugFrame* frame);
		std::vector<DebugVariable>& GetVariables(size_t refence);
		void PopulateChildVariables(DebugVariable& variable);

		size_t GetNextSourceReference();
		size_t GetNextFrameReference();
		size_t GetNextVariableReference();

		// Remote debugging is defined as debugging through the attach request
		bool IsRemoteDebugging() const;
		void SetRemoteDebugging(bool v);

		void InvalidateState();

	private:
		void DeclareVariable(VariableId reference, DebugVariable::Scope scope, nebula::Variable& variable, const symbols::DebugVariable& debugVariable);

		size_t GetLineNumber(const nebula::Frame& frame);

		DebugController* m_controller;
		size_t m_sourceReference = 0;
		size_t m_frameReference = 0;
		size_t m_variableReference = 0;

		bool m_isRemoteDebugging{ false };

		// Data that should be reset between steps

		std::unordered_map<FrameId, DebugFrame*> m_cachedFramesById;

		std::vector<DebugThread> m_threads;
		std::unordered_map<ThreadId, std::vector<DebugFrame>> m_frames;
		std::unordered_map<FrameId, std::vector<DebugScope>> m_scopes;
		std::unordered_map<VariableId, std::vector<DebugVariable>> m_variables;

		std::unordered_map<std::string /* namespace */, DebugSource> m_cachedSources;
	};
} // namespace nebula::debugger

#endif // !_H_DEBUG_EXECUTION_STATE_
