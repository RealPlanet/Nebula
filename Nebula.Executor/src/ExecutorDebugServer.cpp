#include "ExecutorDebugServer.h"
#include "DAPEvents.h"
#include "DAPServer.h"
#include "DebugFile.h"
#include "DebugFunction.h"
#include "DebugServer.h"
#include "DefaultDebugServer.h"
#include "Interpreter.h"
#include "Utility.h"

#include <cassert>
#include <filesystem>
#include <memory>

using namespace nebula::debugger;

#define FETCH_IMPL_SPECIFIC_FLAG(args, destination, name, default)                                                     \
    if (args.contains(#name))                                                                                          \
    {                                                                                                                  \
        auto &value = settings.at(#name);                                                                              \
        if (value.has_value())                                                                                         \
        {                                                                                                              \
            std::string stringValue = std::any_cast<std::string>(value);                                               \
            destination = stringValue == "true" || stringValue == "1";                                                 \
        }                                                                                                              \
    }                                                                                                                  \
    else                                                                                                               \
    {                                                                                                                  \
        destination = default;                                                                                         \
    }

ExecutorDebugServer::ExecutorDebugServer(nebula::Interpreter* virtualMachine) : m_pInterpreter(virtualMachine)
{
	// Should not exist self file but remove it to be sure
	nebula::utility::CleanupDebuggerFiles(true);

	// TODO This shouldnt kill the vm
	m_dapServer = std::make_unique<dap::DAPServer>(this, 0);
	nebula::utility::WriteDebuggerFile(m_dapServer->GetServerPort());
}

ExecutorDebugServer::~ExecutorDebugServer()
{
	DetachDebugger(false);
}

void ExecutorDebugServer::UnregisterScript(const Script* script)
{
	assert(m_debugController);
	if (m_debugController->IsDebugging())
	{
		assert(m_debugState);
		auto removedSource = m_debugState->RemoveSource(script->Namespace());
		if (removedSource.has_value())
		{
			::dap::LoadedSourceEvent event;
			event.reason = dap::LoadedSourceEvent::ReasonValues::Removed;
			utility::assign(event.source, removedSource.value());
			m_dapServer->SendEvent(&event);
		}
	}

	DefaultDebugServer::UnregisterScript(script);
}

DebugFilePtr ExecutorDebugServer::GetScript(const std::string& namespace_)
{
	const symbols::DebugFile* scriptInformation = DefaultDebugServer::GetScript(namespace_);
	if (m_debugController && m_debugController->IsDebugging() && scriptInformation)
	{
		assert(m_debugState);

		auto& ns = scriptInformation->namespace_;
		if (!m_debugState->GetSource(ns))
		{
			// Add source to cache and notify source added
			const DebugSource* newSource = m_debugState->CreateSource(scriptInformation);
			::dap::LoadedSourceEvent event;
			event.reason = dap::LoadedSourceEvent::ReasonValues::New;
			utility::assign(event.source, *newSource);
			m_dapServer->SendEvent(&event);
		}
	}

	return scriptInformation;
}

void ExecutorDebugServer::UnloadAll()
{
	DefaultDebugServer::UnloadAll();
	if (m_debugState)
	{
		m_debugState->ClearSources();
	}
}

const symbols::DebugFunction* ExecutorDebugServer::GetFunctionAtLine(const std::string& namespace_, size_t line)
{
	auto script = GetScript(namespace_);
	if (script == nullptr)
	{
		return nullptr;
	}

	for (auto& kvp : script->functions)
	{
		auto& func = kvp.second;
		if (line >= func.lineNumber && line < func.endLineNumber)
		{
			return &func;
		}
	}

	return nullptr;
}

bool ExecutorDebugServer::IsDebugging() const
{
	return m_isDebugging;
}

void ExecutorDebugServer::AttachDebugger()
{
	m_isDebugging = true;
	// Allocate debugging facilities
	m_debugController = std::make_unique<DebugController>(m_pInterpreter, this);
	m_debugState = std::make_unique<DebugState>(m_debugController.get());

	// Temporarly stops executor while configuration is happening
	m_debugController->StartDebugger();
}

void ExecutorDebugServer::DetachDebugger(bool resumeInterpreter)
{
	if (!m_isDebugging)
	{
		return;
	}

	// Notify immediately we are terminating
	if (m_dapServer)
	{
		m_dapServer->SendEvent<dap::TerminatedEvent>();
	}

	if (m_debugController)
	{
		// Explicit call to avoid restarting the interpreter thread for any reason
		// ( example: Termination of the entire process )
		// Calling this immediately unlocks the original main execution thread
		m_debugController->StopDebugger(resumeInterpreter);
		m_debugState.reset();
	}

	if (m_debugState)
	{
		m_debugState.reset();
	}

	nebula::utility::CleanupDebuggerFiles(true);

	// Save memory, drop all cache
	UnloadAll();

	m_isDebugging = false;
}

void ExecutorDebugServer::CacheAlreadyLoadedSources()
{
	for (auto& [ns, script] : m_pInterpreter->GetLoadedScripts())
	{
		// Just in case it was not registered on creation
		RegisterScript(script.get());

		auto scriptSymbols = GetScript(ns);
		if (scriptSymbols == nullptr)
		{
			// TODO Report if null! Debugging symbols could not be found
			continue;
		}
	}
}

void ExecutorDebugServer::NotifyInterpreterStopped(ThreadId threadId, const dap::StoppedEvent::Reason& reason)
{
	auto stoppedEvent = dap::Create<dap::StoppedEvent>();
	stoppedEvent->reason = reason;
	stoppedEvent->threadId = threadId;
	stoppedEvent->allThreadsStopped = true;
	m_dapServer->QueueEventAfterResponse(std::move(stoppedEvent));
}

std::tuple<bool, size_t, std::string, size_t> ExecutorDebugServer::IsLineDebuggable(const std::string& namespace_,
	size_t line)
{
	const symbols::DebugFunction* function = GetFunctionAtLine(namespace_, line);
	if (!function)
	{
		return { false, 0, "", 0 };
	}

	auto closestOpcode = function->GetOpcodeFromLine(line);
	auto actualLine = function->GetLineFromOpcode(closestOpcode);
	return { true, closestOpcode, function->name, actualLine };
}

#pragma region

size_t ExecutorDebugServer::NormalizeLineNumber(size_t line) const
{
	if (m_dapLineStartsAtOne)
	{
		return line - 1;
	}

	return line;
}

size_t ExecutorDebugServer::DeNormalizeLineNumber(size_t line) const
{
	if (m_dapLineStartsAtOne)
	{
		return line + 1;
	}

	return line;
}

void ExecutorDebugServer::OnInvalidMessageReceived(const std::string&)
{
	assert(false);
}

bool ExecutorDebugServer::OnInitializeRequest(const dap::InitializeRequest& request, dap::InitializeResponse& response)
{
	AttachDebugger();

	response.supportsSingleThreadExecutionRequests = false;
	response.supportTerminateDebuggee = false;
	response.supportSuspendDebuggee = false;

	response.supportsLoadedSourcesRequest = true;
	response.supportsSetVariable = true;
	response.supportsTerminateRequest = true;
	response.supportsTerminateThreadsRequest = true;
	response.supportsFunctionBreakpoints = true;
	response.supportsConfigurationDoneRequest = true;
	response.supportsSetExpression = true;

	m_dapLineStartsAtOne = request.linesStartAt1.value();
	m_dapColumnStartsAtOne = request.columnsStartAt1.value();

	m_dapServer->QueueEventAfterResponse(dap::Create<dap::InitializedEvent>());

	return true;
}

bool ExecutorDebugServer::OnLoadedSourcesRequest(const dap::LoadedSourcesRequest&,
	dap::LoadedSourcesResponse& response)
{
	assert(m_debugController);
	assert(m_debugState);

	auto& sources = m_debugState->GetSources();
	response.sources.reserve(sources.size());
	for (auto& [name, source] : sources)
	{
		::dap::Source dapSource;
		utility::assign(dapSource, source);
		response.sources.push_back(dapSource);
	}

	return true;
}

bool ExecutorDebugServer::OnSourceRequest(const dap::SourceRequest& request, dap::SourceResponse& response)
{
	std::filesystem::path pathToLoad;
	if (request.source.has_value())
	{
		auto& source = request.source.value();
		pathToLoad = source.path.value_or("");

		if (source.sourceReference.has_value())
		{
			const DebugSource* src = m_debugState->GetSource(source.sourceReference.value());
			pathToLoad = src->path;
		}
	}
	else
	{
		// Use retrocompatibility value
		const DebugSource* src = m_debugState->GetSource(request.sourceReference);
		pathToLoad = src->path;
	}

	if (pathToLoad.empty() || !std::filesystem::exists(pathToLoad))
	{
		// TODO LOG
		response.SetFailed("Could not find path of source to load contents");
		return true;
	}
	else
	{
		try
		{
			response.content = utility::ReadAllText(pathToLoad);
		}
		catch (std::exception& e)
		{
			// TODO LOG
			response.SetFailed(e.what());
		}
	}

	return true;
}

bool ExecutorDebugServer::OnAttachRequest(const dap::AttachRequest&, dap::AttachResponse&)
{
	m_startRequestType = DebugStartRequestType::Attach;
	m_debugState->SetRemoteDebugging(true);
	CacheAlreadyLoadedSources();
	return true;
}

bool ExecutorDebugServer::OnLaunchRequest(const dap::LaunchRequest& request, dap::LaunchResponse&)
{
	m_startRequestType = DebugStartRequestType::Launch;
	// We wont need to provide the sources ourselves, we are on the same machine
	m_debugState->SetRemoteDebugging(false);
	CacheAlreadyLoadedSources();

	if (request.implementationSpecificArguments.has_value())
	{
		auto& settings = request.implementationSpecificArguments.value();
		FETCH_IMPL_SPECIFIC_FLAG(settings, m_stepOnEntry, stepOnEntry, false);
	}

	return true;
}

bool ExecutorDebugServer::OnTerminateRequest(const dap::TerminateRequest&, dap::TerminateResponse&)
{
	DetachDebugger(false);
	m_pInterpreter->Stop();
	return true;
}

bool ExecutorDebugServer::OnDisconnectRequest(const dap::DisconnectRequest&, dap::DisconnectResponse&)
{
	// Stop immediately whatever we are doing and drop the debugging
	DetachDebugger(false);

	if (m_startRequestType == DebugStartRequestType::Attach)
	{
		return m_pInterpreter->Resume() == Interpreter::State::Running;
	}

	return m_pInterpreter->Stop() == Interpreter::State::Exited;
}

bool ExecutorDebugServer::OnSetExceptionBreakpointsRequest(const dap::SetExceptionBreakpointsRequest&,
	dap::SetExceptionBreakpointsResponse&)
{
	return false;
}

bool ExecutorDebugServer::OnSetFunctionBreakpointsRequest(const dap::SetFunctionBreakpointsRequest& request,
	dap::SetFunctionBreakpointsResponse& response)
{
	auto& breakpointManager = m_debugController->GetBreakpointManager();
	breakpointManager.ClearFunctionBreakpoints();
	for (auto& breakpoint : request.breakpoints)
	{
		dap::Breakpoint bp = {
			.verified = false,
			.source = std::nullopt,
			.line = -1,
			.reason = dap::Breakpoint::ReasonValues::Failed,
		};

		auto& funcName = breakpoint.name;
		size_t separatorIndex = funcName.find("::");
		if (separatorIndex == std::string::npos)
		{
			bp.message = "Breakpoint function name is not valid, expected <namespace>::<function name>";
			response.breakpoints.push_back(bp);
			continue;
		}

		std::string bpNamespace = funcName.substr(0, separatorIndex);
		std::string bpFuncName = funcName.substr(separatorIndex + 2);

		const symbols::DebugFunction* dbgInfo = GetFunction(bpNamespace, bpFuncName);
		if (dbgInfo == nullptr)
		{
			bp.message = std::format("Could not find function '{}' in namespace '{}'", bpFuncName, bpNamespace);
			response.breakpoints.push_back(bp);
			continue;
		}

		bp.verified = true;
		bp.line = DeNormalizeLineNumber(dbgInfo->lineNumber);
		bp.reason = std::nullopt;
		response.breakpoints.push_back(bp);

		breakpointManager.AddFunctionBreakpoint(
			BreakpointInformation(bpNamespace, bpFuncName, BreakpointInformation::NO_OPCODE));
	}

	return true;
}

bool ExecutorDebugServer::OnSetBreakpointsRequest(const dap::SetBreakpointsRequest& request,
	dap::SetBreakpointsResponse& response)
{
	if (request.sourceModified.value_or(false))
	{
		assert(false);
		response.SetFailed("Source was modified, not supported");
		return true;
	}

	if (!request.breakpoints.has_value())
	{
		// TODO Report, dont even bother processing the req
		return true;
	}

	const DebugSource* source{ nullptr };
	if (request.source.sourceReference.has_value())
	{
		source = m_debugState->GetSource(request.source.sourceReference.value());
	}

	if (!source || request.source.path.has_value())
	{
		source = m_debugState->GetSourceByPath(request.source.path.value());
	}

	auto& bpToSet = request.breakpoints.value();
	response.breakpoints.reserve(bpToSet.size());
	if (!source)
	{
		// TODO Report
		for (auto& reqBp : bpToSet)
		{
			response.breakpoints.push_back({
				.verified = false,
				.message = "Source of this breakpoint could not be located",
				.line = reqBp.line,
				.column = reqBp.column,
				.reason = ::dap::Breakpoint::ReasonValues::Failed,
				});
		}
		return true;
	}

	auto& breakpointManager = m_debugController->GetBreakpointManager();
	breakpointManager.ClearBreakpoints(source->namespace_);
	for (auto& reqBp : bpToSet)
	{
		::dap::Breakpoint bp{
			.verified = false,
			.source = ::dap::Source{},
			.line = reqBp.line,
			.column = reqBp.column,
		};

		utility::assign(bp.source.value(), *source);

		auto normalizedLine = NormalizeLineNumber(reqBp.line);
		auto [isLineDebuggable, instructionIndex, functionName, actualLineNumber] =
			IsLineDebuggable(source->namespace_, normalizedLine);
		if (!isLineDebuggable)
		{
			bp.message = std::format("Line '{}' is not debuggable", reqBp.line);
			response.breakpoints.push_back(bp);
			continue;
		}

		bp.verified = true;
		bp.reason = std::nullopt;
		bp.line = DeNormalizeLineNumber(actualLineNumber);
		response.breakpoints.push_back(bp);
		breakpointManager.AddBreakpoint(BreakpointInformation(source->namespace_, functionName, instructionIndex));
	}

	return true;
}

bool ExecutorDebugServer::OnConfigurationDoneRequest(const dap::ConfigurationDoneRequest&,
	dap::ConfigurationDoneResponse&)
{
	if (m_stepOnEntry)
	{
		NotifyInterpreterStopped(NO_THREAD_ID, dap::StoppedEvent::ReasonValues::Entry);
	}

	if (m_startRequestType == DebugStartRequestType::Attach || !m_stepOnEntry)
	{
		// Now that we setup everything we can resume execution
		m_debugController->Continue(NO_THREAD_ID);
	}

	return true;
}

bool ExecutorDebugServer::OnSetVariableRequest(const dap::SetVariableRequest& request,
	dap::SetVariableResponse& response)
{
	assert(m_debugController);
	assert(m_debugState);

	if (!m_debugController->IsPaused())
	{
		response.message = "Debugger is not paused.";
		return false;
	}

	auto& scopeVariables = m_debugState->GetVariables(request.variablesReference);
	if (scopeVariables.empty())
	{
		response.message = "No variables found for given reference";
		return false;
	}

	DebugVariable* varToChange{ nullptr };
	for (auto& var : scopeVariables)
	{
		if (var.debugInformation->name == request.name)
		{
			varToChange = &var;
			break;
		}
	}

	if (varToChange == nullptr)
	{
		response.SetFailed(std::format("Could not find variable with name '{}' in scope {}", request.name,
			request.variablesReference));
		return false;
	}

	if (!varToChange->canChangeValueByDebugger)
	{
		response.SetFailed("Variable cannot have it's value changed by a debugger");
		return false;
	}

	assert(varToChange->originalVariable);
	std::string reason;
	bool changedValue = varToChange->OverrideValue(request.value, reason);
	if (!changedValue)
	{
		if (reason == "")
		{
			response.SetFailed(std::format("Variable '{}' cannot have it's value changed to '{}'", request.name, request.value));
		}
		else
		{
			response.SetFailed(reason);
		}

		return false;
	}

	response.value = varToChange->GetDisplayValue();
	response.variablesReference = varToChange->reference;
	return true;
}

bool ExecutorDebugServer::OnThreadsRequest(const dap::ThreadsRequest&, dap::ThreadsResponse& response)
{
	if (!IsDebugging())
	{
		return false;
	}

	assert(m_debugController);
	if (!m_debugController->IsPaused())
	{
		// If we are stepping we return a dummy thread
		// Once we notify we are stopping we'll generate the actual threads!
		response.SetFailed("The debugger has not been paused");
		return true;
	}

	auto& threads = m_debugState->GetThreads();
	response.threads.reserve(threads.size());
	for (auto& thread : threads)
	{
		::dap::Thread dapThread;
		dapThread.id = thread.id;
		dapThread.name = thread.name;
		response.threads.push_back(dapThread);
	}
	return true;
}

bool ExecutorDebugServer::OnStackTraceRequest(const dap::StackTraceRequest& request, dap::StackTraceResponse& response)
{
	assert(m_debugController->IsDebugging());
	assert(m_debugState);

	if (!m_debugController->IsPaused())
	{
		return false;
	}

	const size_t startFrame = request.startFrame.value_or(0);
	const size_t levels = request.levels.value_or(0);

	// Cache enough frames to satisfy this request.
	const size_t framesToCache = levels == 0 ? 0 : startFrame + levels;
	auto thread = m_debugState->GetThread(request.threadId);
	if (thread == nullptr)
	{
		return false;
	}
	auto& frames = m_debugState->GetFramesOfThread(request.threadId, framesToCache);

	response.totalFrames = thread->frameCount;
	if (startFrame >= frames.size())
	{
		return true;
	}

	const size_t endFrame = levels == 0 ? frames.size() : std::min(startFrame + levels, frames.size());

	response.stackFrames.reserve(endFrame - startFrame);
	for (size_t i = startFrame; i < endFrame; ++i)
	{
		const DebugFrame& frame = frames.at(frames.size() - 1 - i);
		const DebugSource* source = m_debugState->GetSource(frame.functionNamespace);

		::dap::StackFrame stackFrame;
		stackFrame.id = frame.id;
		stackFrame.line = DeNormalizeLineNumber(frame.line);
		stackFrame.column = 0;

		if (source != nullptr)
		{
			stackFrame.name = frame.functionName;
			stackFrame.source = ::dap::Source{};
			utility::assign(stackFrame.source.value(), *source);
		}
		else
		{
			stackFrame.name = std::format("{} - NO SOURCE", frame.functionName);
		}

		response.stackFrames.push_back(std::move(stackFrame));
	}

	return true;
}

bool ExecutorDebugServer::OnScopesRequest(const dap::ScopesRequest& request, dap::ScopesResponse& response)
{
	if (!m_debugController->IsPaused())
	{
		// TODO REPORT
		return false;
	}

	const DebugFrame* frame = m_debugState->GetFrameById(request.frameId);
	if (frame == nullptr)
	{
		response.SetFailed(std::format("Could not find frame with id {}", request.frameId));
		return false;
	}

	const std::vector<DebugScope>& scopes = m_debugState->GetScopes(frame);
	response.scopes.reserve(scopes.size());
	for (const DebugScope& scope : scopes)
	{
		::dap::Scope dapScope;
		utility::assign(dapScope, scope);
		response.scopes.push_back(dapScope);
	}

	return true;
}

bool ExecutorDebugServer::OnVariablesRequest(const dap::VariablesRequest& request, dap::VariablesResponse& response)
{
	assert(m_debugController);
	assert(m_debugState);

	if (!m_debugController->IsPaused())
	{
		// TODO REPORT
		return false;
	}

	const size_t reference = request.variablesReference;
	auto& variables = m_debugState->GetVariables(reference);
	response.variables.reserve(variables.size());
	for (auto& var : variables)
	{
		if (var.reference > 0)
		{
			// We need to allocate the child variables so the next request can fetch them
			assert(var.debugInformation->internalType == "bundle" || var.debugInformation->internalType == "array");
			m_debugState->PopulateChildVariables(var);
		}

		::dap::Variable variable;
		utility::assign(variable, var);
		response.variables.push_back(variable);
	}

	return true;
}

bool ExecutorDebugServer::OnPauseRequest(const dap::PauseRequest&, dap::PauseResponse&)
{
	if (m_debugController->IsPaused())
	{
		return true;
	}

	m_debugController->StopCurrentOperation();
	return true;
}

bool ExecutorDebugServer::OnNextRequest(const dap::NextRequest& request, dap::NextResponse&)
{
	if (!m_debugController->IsPaused())
	{
		// TODO REPORT
		return false;
	}

	assert(!request.singleThread);
	m_debugController->Step(request.threadId);
	return true;
}

bool ExecutorDebugServer::OnStepInRequest(const dap::StepInRequest& request, dap::StepInResponse&)
{
	if (!m_debugController->IsPaused())
	{
		// TODO REPORT
		return false;
	}

	assert(!request.singleThread);
	m_debugController->StepIn(request.threadId);
	return true;
}

bool ExecutorDebugServer::OnContinueRequest(const dap::ContinueRequest& request, dap::ContinueResponse&)
{
	if (!m_debugController->IsPaused())
	{
		// TODO REPORT
		return false;
	}

	assert(!request.singleThread);
	m_debugController->Continue(request.threadId);
	return true;
}

#pragma endregion DAP Listener

#pragma region

#pragma endregion Debug Controller Listener

void ExecutorDebugServer::OnInterpreterPaused(ThreadId currentThread, PauseReason reason)
{
	dap::StoppedEvent::Reason dapReason;
	switch (reason)
	{
	case PauseReason::Entry:
		dapReason = dap::StoppedEvent::ReasonValues::Entry;
		break;
	case PauseReason::Stopped:
		dapReason = dap::StoppedEvent::ReasonValues::Pause;
		break;
	case PauseReason::Step:
		dapReason = dap::StoppedEvent::ReasonValues::Step;
		break;
	default:
		assert(false);
		dapReason = dap::StoppedEvent::ReasonValues::Pause;
	}

	NotifyInterpreterStopped(currentThread, dapReason);
}

void ExecutorDebugServer::OnInterpreterResumed(ThreadId thread)
{
	assert(m_debugState);
	m_debugState->InvalidateState();

	::dap::ContinuedEvent event;
	event.threadId = thread;
	event.allThreadsContinued = true;
	m_dapServer->SendEvent(&event);
}

void ExecutorDebugServer::OnInterpreterTerminated()
{
	m_dapServer->SendEvent<dap::TerminatedEvent>();
}

void ExecutorDebugServer::OnBreakpointHit(DebugBreakpoint& breakpoint)
{
	dap::StoppedEvent::Reason reason = dap::StoppedEvent::ReasonValues::Breakpoint;
	if (breakpoint.isFunctionBreakpoint)
	{
		reason = dap::StoppedEvent::ReasonValues::FunctionBreakpoint;
	}

	NotifyInterpreterStopped(breakpoint.threadId, reason);
}

void ExecutorDebugServer::OnOutput(const std::string&)
{
	assert(false);
}

void ExecutorDebugServer::OnInterpreterFatalError(const nebula::shared::ErrorCallStack* error)
{
	assert(error);

	::dap::OutputEvent outputEvent;
	outputEvent.category = ::dap::OutputEvent::CategoryValues::StdErr;
	outputEvent.group = ::dap::OutputEvent::GroupValues::End;
	outputEvent.output = std::format("~~~~~~ Interpreter fatal error ~~~~~~\n{} - {}", (int)error->GetErrorCode(),
		error->GetReadableError());

	if (error == nullptr || error->GetLineCount() == 0)
	{
		outputEvent.output += "\n No additional information available.\n";
		m_dapServer->SendEvent(&outputEvent);
	}
	else
	{
		m_dapServer->SendEvent(&outputEvent);
		for (auto& event : utility::build_fatal_error_outputs(error))
		{
			::dap::OutputEvent dapEvent;
			outputEvent.category = ::dap::OutputEvent::CategoryValues::StdErr;
			outputEvent.group = ::dap::OutputEvent::GroupValues::End;
			outputEvent.output = event.output;

			auto source = m_debugState->GetSource(event.functionNamespace);
			if (source)
			{
				outputEvent.source = ::dap::Source();
				utility::assign(outputEvent.source.value(), *source);

				if (event.line != nebula::debugger::symbols::DebugFunction::NoLineInfo)
				{
					outputEvent.line = DeNormalizeLineNumber(event.line);
					outputEvent.output += std::format(" Line {}", outputEvent.line.value());
				}
			}

			m_dapServer->SendEvent(&outputEvent);
		}
	}

	DetachDebugger(false);
	m_pInterpreter->Stop();
}
