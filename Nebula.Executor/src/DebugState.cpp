#include "DebugState.h"
#include "DebugController.h"
#include "DebugServer.h"
#include "DebugTypes.h"
#include "ExecutorDebugServer.h"
#include "Frame.h"
#include "Interpreter.h"
#include "DebugFile.h"
#include "DebugFunction.h"
#include "DebugVariable.h"
#include "VariantArray.h"

#include <algorithm>
#include <cassert>
#include <format>
#include <optional>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

using namespace nebula;
using namespace nebula::debugger;

DebugState::DebugState(DebugController* controller) : m_controller{ controller }
{
	assert(controller);
}

const DebugSource* DebugState::CreateSource(const symbols::DebugFile* information)
{
	assert(information);

	auto& filename = information->originalFileName;
	auto& fullpath = information->originalFileFullName;
	auto& ns = information->namespace_;

	auto [iterator, interted] = m_cachedSources.insert(std::make_pair(ns, DebugSource{
																			  .name = filename,
																			  .reference = GetNextSourceReference(),
																			  .namespace_ = ns,
																			  .path = fullpath,
																			  .origin = "Loaded scripts",
		}));

	assert(interted);
	return &(*iterator).second;
}

std::optional<DebugSource> DebugState::RemoveSource(const std::string& namespace_)
{
	auto it = m_cachedSources.find(namespace_);
	if (it == m_cachedSources.end())
	{
		return std::nullopt;
	}

	DebugSource source = it->second;
	m_cachedSources.erase(it);
	return source;
}

void DebugState::ClearSources()
{
	m_cachedSources.clear();
	m_sourceReference = 0;
}

const std::unordered_map<std::string, DebugSource>& DebugState::GetSources() const
{
	return m_cachedSources;
}

const DebugSource* DebugState::GetSource(const std::string& namespace_) const
{
	auto it = m_cachedSources.find(namespace_);
	if (it == m_cachedSources.end())
	{
		return nullptr;
	}

	return &it->second;
}

const DebugSource* DebugState::GetSourceByPath(const std::string& path) const
{
	if (path == "")
	{
		return nullptr;
	}

	for (auto& [ns, source] : m_cachedSources)
	{
		if (source.path == path)
		{
			return &source;
		}
	}

	return nullptr;
}

const DebugSource* DebugState::GetSource(size_t reference) const
{
	if (reference == 0)
	{
		return nullptr;
	}

	for (auto& [ns, source] : m_cachedSources)
	{
		if (source.reference == reference)
		{
			return &source;
		}
	}

	return nullptr;
}

const std::vector<DebugThread>& DebugState::GetThreads()
{
	assert(m_controller->IsDebugging());
	assert(m_controller->IsPaused());

	// If the threads are empty we need to cache them OR the vm has exited
	// either way attempt to cache them
	if (m_threads.empty())
	{
		auto& interpreter = m_controller->GetInterpreter();
		auto& threads = interpreter.GetThreads();
		ThreadId threadId = 0;
		for (const auto& thread : threads)
		{
			m_threads.emplace_back(DebugThread{
				.id = threadId,
				.name = std::format("Thread {}", threadId + 1),
				.frameCount = thread.size(),
				});

			threadId++;
		}
	}

	return m_threads;
}

const DebugThread* DebugState::GetThread(ThreadId id) const
{
	assert(m_controller->IsDebugging());
	assert(m_controller->IsPaused());
	assert(m_threads.size() != 0);

	for (auto& thread : m_threads)
	{
		if (thread.id == id)
		{
			return &thread;
		}
	}

	return nullptr;
}

const std::vector<DebugFrame>& DebugState::GetFramesOfThread(ThreadId id, size_t requestedFrames)
{
	auto it = m_frames.find(id);
	if (it == m_frames.end())
	{
		it = m_frames.emplace(id, std::vector<DebugFrame>{}).first;
	}

	auto& thread = m_controller->GetInterpreter().GetThreads().At(id);
	const size_t totalFrames = thread.size();

	// A levels value of 0 means "all frames".
	if (requestedFrames == 0)
	{
		requestedFrames = totalFrames;
	}
	else
	{
		requestedFrames = std::min(requestedFrames, totalFrames);
	}

	auto& cachedFrames = it->second;

	if (cachedFrames.size() < requestedFrames)
	{
		const size_t alreadyCached = cachedFrames.size();
		const size_t framesToCache = requestedFrames - alreadyCached;

		cachedFrames.reserve(requestedFrames);

		for (size_t i = 0; i < framesToCache; ++i)
		{
			const size_t frameIndex = totalFrames - alreadyCached - i - 1;
			auto& actualFrame = thread.at(frameIndex);
			FrameId frameId = GetNextFrameReference();
			cachedFrames.push_back(DebugFrame{
				.id = frameId,
				.functionNamespace = actualFrame->Namespace(),
				.functionName = actualFrame->FunctionName(),
				.line = GetLineNumber(*actualFrame),
				.column = 0,
				.originalFrame = actualFrame,
				});

			m_cachedFramesById.emplace(std::make_pair(frameId, &cachedFrames[cachedFrames.size() - 1]));
		}
	}

	return cachedFrames;
}

const DebugFrame* DebugState::GetFrameById(FrameId frameId) const
{
	auto it = m_cachedFramesById.find(frameId);
	if (it == m_cachedFramesById.end())
	{
		return nullptr;
	}

	return it->second;
}

const std::vector<DebugScope>& DebugState::GetScopes(const DebugFrame* frame)
{
	assert(frame);
	if (!frame)
	{
		return m_scopes[0];
	}

	assert(frame->originalFrame);
	auto it = m_scopes.find(frame->id);
	if (it != m_scopes.end())
	{
		return it->second;
	}

	ExecutorDebugServer* debugServer = static_cast<ExecutorDebugServer*>(DebugServer::Instance());
	assert(debugServer);
	auto debugFunction = debugServer->GetFunction(frame->functionNamespace, frame->functionName);
	assert(debugFunction);

	auto insertIt = m_scopes.insert(it, std::make_pair(frame->id, std::vector<DebugScope>{}));

	// Create all scopes
	auto& frameMemory = frame->originalFrame->Memory();
	if (frameMemory.ParamCount() > 0)
	{
		DebugScope argScope =
		{
			.frame = frame,
			.name = "Arguments",
			.reference = GetNextVariableReference(),
			.type = ScopeType::Arguments,
		};

		insertIt->second.push_back(argScope);

		for (size_t i{ 0 }; i < frameMemory.ParamCount(); i++)
		{
			auto& debugParameter = debugFunction->parameters[i];
			DeclareVariable(argScope.reference, DebugVariable::Scope::Global, frameMemory.ParamAt(i), debugParameter);
		}
	}

	if (frameMemory.LocalCount() > 0)
	{
		DebugScope localScope =
		{
			.frame = frame,
			.name = "Locals",
			.reference = GetNextVariableReference(),
			.type = ScopeType::Locals,
		};

		insertIt->second.push_back(localScope);

		for (size_t i{ 0 }; i < frameMemory.LocalCount(); i++)
		{
			auto& debugLocal = debugFunction->locals[i];
			DeclareVariable(localScope.reference, DebugVariable::Scope::Local, frameMemory.LocalAt(i), debugLocal);
		}
	}

	return insertIt->second;
}

std::vector<DebugVariable>& DebugState::GetVariables(size_t reference)
{
	return m_variables[reference];
}

void DebugState::PopulateChildVariables(DebugVariable& variable)
{
	assert(variable.debugInformation);
	assert(variable.originalVariable);
	assert(variable.debugInformation->internalType == "bundle" 
		|| variable.debugInformation->internalType == "array");

	ExecutorDebugServer* debugServer = static_cast<ExecutorDebugServer*>(DebugServer::Instance());
	assert(debugServer);

	bool isObjectInitialized = variable.originalVariable->ContainsGCObject();
	if (!isObjectInitialized)
	{
		assert(false);
		return;
	}

	if (variable.debugInformation->internalType == "bundle")
	{
		assert(variable.debugInformation);
		assert(variable.debugInformation->sourceNamespace != "");
		assert(variable.debugInformation->sourceType != "");

		auto object = debugServer->GetBundle(variable.debugInformation->sourceNamespace, variable.debugInformation->sourceType);
		assert(object);

		if (!object)
		{
			// TODO HANDLE SYMBOLS NOT FOUND
			assert(false);
			return;
		}

		for (size_t i = 0; i < object->fields.size(); i++)
		{
			auto& internalObject = *(nebula::Bundle*)variable.originalVariable->AsGCObject().get();
			auto& internalVariable = internalObject.GetVariable((int)i);
			auto& dbgField = object->fields[i];

			DeclareVariable(variable.reference,
				DebugVariable::Scope::Member,
				internalVariable,
				dbgField);
		}

		return;
	}

	if (variable.debugInformation->internalType == "array")
	{
		assert(variable.debugInformation);
		assert(variable.debugInformation->sourceType != "");

		auto& internalArray = *(nebula::VariantArray*)variable.originalVariable->AsGCObject().get();
		for (size_t i{ 0 }; i < internalArray.Size(); i++)
		{
			// TODO
			//DeclareVariable(variable.reference,
			//	DebugVariable::Scope::Member,
			//	internalVariable,
			//	nullptr);
		}

		return;
	}
}

size_t DebugState::GetNextSourceReference()
{
	if (!IsRemoteDebugging())
	{
		// Client can receive it directly
		return 0;
	}

	return ++m_sourceReference;
}

size_t DebugState::GetNextFrameReference()
{
	return ++m_frameReference;
}

size_t DebugState::GetNextVariableReference()
{
	return ++m_variableReference;
}

// Remote debugging is defined as debugging through the attach request
bool DebugState::IsRemoteDebugging() const
{
	return m_isRemoteDebugging;
}

void DebugState::SetRemoteDebugging(bool v)
{
	m_isRemoteDebugging = v;
}

void DebugState::InvalidateState()
{
	m_threads.clear();
	m_frames.clear();
	m_cachedFramesById.clear();
	m_scopes.clear();
	m_variables.clear();
}

void DebugState::DeclareVariable(VariableId reference, DebugVariable::Scope scope,
	nebula::Value& variable,
	const symbols::DebugVariable& debugVariable)
{

	DebugVariable wrapDebugVarriable
	{
		.reference = 0,
		.scope = scope,
		.debugInformation = &debugVariable,
		.canChangeValueByDebugger = true,
		.originalVariable = &variable,
	};

	if (debugVariable.internalType == "bundle" ||
		debugVariable.internalType == "array")
	{
		wrapDebugVarriable.canChangeValueByDebugger = false;
		if (variable.ContainsGCObject())
		{
			wrapDebugVarriable.reference = GetNextVariableReference();
		}
	}

	auto& currentVariables = m_variables[reference];
	currentVariables.emplace_back(std::move(wrapDebugVarriable));
}

size_t DebugState::GetLineNumber(const nebula::Frame& frame)
{
	assert(DebugServer::Instance());

	ExecutorDebugServer* debugServer = static_cast<ExecutorDebugServer*>(DebugServer::Instance());
	assert(debugServer);

	const symbols::DebugFunction* dbgInfo = debugServer->GetFunction(frame.Namespace(), frame.FunctionName());
	if (!dbgInfo)
	{
		return 0;
	}

	auto nextOpcode = frame.NextInstructionIndex();
	if (nextOpcode == 0)
	{
		// Function hasn't executed yet
		return dbgInfo->GetLineFromOpcode(0);
	}

	// Return current opcode
	return dbgInfo->GetLineFromOpcode(nextOpcode);
}
