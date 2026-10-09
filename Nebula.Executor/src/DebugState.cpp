#include "DebugState.h"
#include "DebugController.h"
#include "ExecutorDebugServer.h"

#include <cassert>
#include <optional>
#include <string>
#include <unordered_map>
#include <utility>

// Internal objects
#include "Interpreter.h"
#include "Callstack.h"
#include "Frame.h"
#include "VariantArray.h"

nebula::debugger::DebugState::DebugState(Interpreter* interpreter)
	: m_interpreter{ interpreter }
{
	assert(interpreter);
}

const nebula::debugger::Source* nebula::debugger::DebugState::CreateSource(const symbols::DebugSymbols* information)
{
	assert(information);

	auto& fullpath = information->sourceFilePath;
	auto& ns = information->namespace_;
	auto [iterator, inserted] = m_cachedSources.insert(
		std::make_pair(ns, Source{
			.name = ns,
			.reference = GetNextSourceReference(),
			.namespace_ = ns,
			.path = fullpath,
			.origin = "Loaded scripts",
			.debugSymbols = information
			}));

	assert(inserted);
	return &(*iterator).second;
}

std::optional<nebula::debugger::Source> nebula::debugger::DebugState::RemoveSource(const std::string& namespace_)
{
	auto it = m_cachedSources.find(namespace_);
	if (it == m_cachedSources.end())
	{
		return std::nullopt;
	}

	Source source = it->second;
	m_cachedSources.erase(it);
	return source;
}

void nebula::debugger::DebugState::ClearSources()
{
	m_cachedSources.clear();
	m_sourceReference = 0;
}

const std::unordered_map<std::string, nebula::debugger::Source>& nebula::debugger::DebugState::GetSources() const
{
	return m_cachedSources;
}

const nebula::debugger::Source* nebula::debugger::DebugState::GetSource(const std::string& namespace_) const
{
	auto it = m_cachedSources.find(namespace_);
	if (it == m_cachedSources.end())
	{
		return nullptr;
	}

	return &it->second;
}

const nebula::debugger::Source* nebula::debugger::DebugState::GetSourceByPath(const std::string& path) const
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

const nebula::debugger::Source* nebula::debugger::DebugState::GetSource(size_t reference) const
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

const std::vector<nebula::debugger::Thread>& nebula::debugger::DebugState::GetThreads()
{
	// If the threads are empty we need to cache them OR the vm has exited
	// either way attempt to cache them
	if (m_threads.empty())
	{
		auto& threads = m_interpreter->GetThreads();
		ThreadId threadId = 0;
		for (const auto& thread : threads)
		{
			Thread debugThread;
			debugThread.id = threadId++;
			debugThread.name = std::format("Thread {}", threadId);
			debugThread.internalThread = &thread;

			m_threads.push_back(std::move(debugThread));
		}
	}

	return m_threads;
}

const nebula::debugger::Thread* nebula::debugger::DebugState::GetThread(nebula::debugger::ThreadId id) const
{
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

const std::vector<nebula::debugger::Frame>& nebula::debugger::DebugState::GetFrames(nebula::debugger::ThreadId id, size_t requestedFrames)
{
	auto it = m_frames.find(id);
	if (it == m_frames.end())
	{
		it = m_frames.emplace(id, std::vector<nebula::debugger::Frame>{}).first;
	}

	auto& thread = m_interpreter->GetThreads().At(id);
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

		cachedFrames.reserve(totalFrames);
		for (size_t i = 0; i < framesToCache; ++i)
		{
			const size_t frameIndex = totalFrames - alreadyCached - i - 1;
			auto& actualFrame = thread.at(frameIndex);
			FrameId frameId = GetNextFrameReference();

			nebula::debugger::Frame frame;
			frame.id = frameId;
			frame.internalFrame = actualFrame;
			frame.source = GetSource(actualFrame->GetFunction()->Namespace());
			if (frame.source != nullptr &&
				frame.source->debugSymbols != nullptr)
			{
				const std::string& fName = actualFrame->FunctionName();
				frame.functionSymbols = frame.source->debugSymbols->GetFunction(fName);
				if (frame.functionSymbols)
				{
					frame.currentLine = frame.functionSymbols->GetLineFromOpcode(actualFrame->NextInstructionIndex());
				}
			}

			cachedFrames.push_back(std::move(frame));
			m_framesById.emplace(std::make_pair(frameId, &cachedFrames[cachedFrames.size() - 1]));
		}
	}

	return cachedFrames;
}

const nebula::debugger::Frame* nebula::debugger::DebugState::GetFrameById(nebula::debugger::FrameId frameId) const
{
	auto it = m_framesById.find(frameId);
	if (it == m_framesById.end())
	{
		return nullptr;
	}

	return it->second;
}

const std::vector<nebula::debugger::GenericScope>& nebula::debugger::DebugState::GetScopes(const nebula::debugger::Frame* frame)
{
	assert(frame);
	assert(frame->internalFrame);
	auto it = m_scopes.find(frame->id);
	if (it != m_scopes.end())
	{
		return it->second;
	}

	auto insertIt = m_scopes.insert(it, std::make_pair(frame->id, std::vector<nebula::debugger::GenericScope>{}));
	if (!frame->source || !frame->source->debugSymbols)
	{
		return insertIt->second;
	}

	const nebula::debugger::symbols::FunctionInformation* debugInformation = frame->functionSymbols;
	if (!debugInformation)
	{
		return insertIt->second;
	}

	// Create all scopes
	auto& frameMemory = frame->internalFrame->Memory();
	if (frameMemory.ParamCount() > 0)
	{
		nebula::debugger::GenericScope argScope =
		{
			.id = GetNextVariableReference(),
			.name = "Arguments",
			.type = nebula::debugger::GenericScope::eType::Arguments,
			.internalFrame = frame->internalFrame,
		};

		insertIt->second.push_back(argScope);
		for (size_t i{ 0 }; i < frameMemory.ParamCount() && i < debugInformation->parameters.size(); i++)
		{
			auto& debugParameter = debugInformation->parameters[i];
			DeclareVariable(argScope.id, frameMemory.ParamAt(i), frame->source->debugSymbols, debugParameter);
		}
	}

	if (frameMemory.LocalCount() > 0)
	{
		nebula::debugger::GenericScope localScope =
		{
			.id = GetNextVariableReference(),
			.name = "Locals",
			.type = nebula::debugger::GenericScope::eType::Locals,
			.internalFrame = frame->internalFrame,
		};

		insertIt->second.push_back(localScope);

		for (size_t i{ 0 }; i < frameMemory.LocalCount() && i < debugInformation->locals.size(); i++)
		{
			auto& debugLocal = debugInformation->locals[i];
			DeclareVariable(localScope.id, frameMemory.LocalAt(i), frame->source->debugSymbols, debugLocal);
		}
	}

	return insertIt->second;
}

std::vector<nebula::debugger::Value>& nebula::debugger::DebugState::GetValues(size_t reference)
{
	return m_values[reference];
}

void nebula::debugger::DebugState::PopulateChildValues(Value& variable)
{
	if (m_values.find(variable.id) != m_values.end())
	{
		// Already populated
		return;
	}

	assert(variable.typeInformation);
	assert(variable.internalValue);
	assert(variable.debugSymbols);

	assert(variable.typeInformation->kind == symbols::TypeInformation::eKind::Object ||
		variable.typeInformation->kind == symbols::TypeInformation::eKind::Array);

	bool isObjectInitialized = variable.internalValue->ContainsGCObject();
	if (!isObjectInitialized)
	{
		assert(false);
		return;
	}

	if (variable.typeInformation->kind == symbols::TypeInformation::eKind::Object)
	{
		// If the object is defined in a different namespace we need to find
		// the source that contains the type information for this object
		// because the one we have does not expose all information
		auto& internalObject = *(nebula::Bundle*)variable.internalValue->AsGCObject().get();

		const symbols::DebugSymbols* objectScriptDefinitionDebugSymbols = GetSource(variable.typeInformation->objectNamespace)->debugSymbols;
		const symbols::TypeInformation* objectTypeInformation = objectScriptDefinitionDebugSymbols->GetType(variable.typeInformation->objectName);

		for (size_t i = 0; i < objectTypeInformation->members.size(); i++)
		{
			auto& internalVariable = internalObject.GetVariable((int)i);
			DeclareVariable(variable.id, internalVariable, objectScriptDefinitionDebugSymbols, objectTypeInformation->members[i]);
		}
		return;
	}

	if (variable.typeInformation->kind == symbols::TypeInformation::eKind::Array)
	{
		assert(variable.internalValue->GetValueType() == DataStackVariantIndex::_TypeObject);
		assert(!variable.internalValue->ContainsGCObject() || variable.internalValue->AsGCObject()->GetType() == ObjectType::Array);

		// If the object is defined in a different namespace we need to find
		// the source that contains the type information for this object
		// because the one we have does not expose all information
		auto& internalObject = *(nebula::VariantArray*)variable.internalValue->AsGCObject().get();

		const symbols::DebugSymbols* arrayTypeDebugSymbols = variable.debugSymbols;
		const symbols::TypeInformation* elementType = arrayTypeDebugSymbols->GetType(variable.typeInformation->arrayTypeId);

		for (size_t i{ 0 }; i < internalObject.Size(); i++)
		{
			auto& internalVariable = internalObject.At((int)i);
			DeclareVariable(variable.id, internalVariable, arrayTypeDebugSymbols, symbols::ValueInformation
				{
					.name = "[" + std::to_string(i) + "]",
					.typeId = elementType->Id,
				});
		}
	}
}

size_t nebula::debugger::DebugState::GetNextSourceReference()
{
	if (!IsRemoteDebugging())
	{
		// Client can receive it directly
		return 0;
	}

	return ++m_sourceReference;
}

size_t nebula::debugger::DebugState::GetNextFrameReference()
{
	return ++m_frameReference;
}

size_t nebula::debugger::DebugState::GetNextVariableReference()
{
	return ++m_variableReference;
}

bool nebula::debugger::DebugState::IsRemoteDebugging() const
{
	return m_isRemoteDebugging;
}

void nebula::debugger::DebugState::SetRemoteDebugging(bool v)
{
	m_isRemoteDebugging = v;
}

void nebula::debugger::DebugState::InvalidateState()
{
	m_threads.clear();
	m_frames.clear();
	m_framesById.clear();
	m_scopes.clear();
	m_values.clear();
}

void nebula::debugger::DebugState::DeclareVariable(nebula::debugger::VariableId reference,
	nebula::Value& internalValue,
	const nebula::debugger::symbols::DebugSymbols* debugSymbols,
	const nebula::debugger::symbols::ValueInformation& valueInformation)
{
	const nebula::debugger::symbols::TypeInformation* typeInformation = debugSymbols->GetType(valueInformation.typeId);
	nebula::debugger::Value wrapValue
	{
		.id = 0,
		.debugSymbols = debugSymbols,
		.typeInformation = typeInformation,
		.valueInformation = valueInformation,
		.internalValue = &internalValue,
	};

	if (typeInformation->kind == nebula::debugger::symbols::TypeInformation::eKind::Object ||
		typeInformation->kind == nebula::debugger::symbols::TypeInformation::eKind::Array)
	{
		if (wrapValue.internalValue->ContainsGCObject())
		{
			wrapValue.id = GetNextVariableReference();
		}
	}

	auto& currentVariables = m_values[reference];
	currentVariables.emplace_back(std::move(wrapValue));
}
