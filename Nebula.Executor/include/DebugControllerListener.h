#pragma once

#ifndef _H_DEBUG_CONTROLLER_LISTENER_
#define _H_DEBUG_CONTROLLER_LISTENER_

#include "ErrorCallStack.h"
#include <string>

namespace nebula::debugger
{
	enum class PauseReason
	{
		Entry,
		Step,
		Stopped,
	};

	class DebugControllerListener
	{
	public:
		virtual void OnInterpreterPaused(ThreadId threadId, PauseReason reason) = 0;
		virtual void OnInterpreterResumed(ThreadId threadId) = 0;
		virtual void OnInterpreterTerminated() = 0;
		virtual void OnBreakpointHit(DebugBreakpoint& breakpoint) = 0;
		virtual void OnInterpreterFatalError(const nebula::shared::ErrorCallStack* error) = 0;
		virtual void OnOutput(const std::string& output) = 0;
	};
}

#endif // !_H_DEBUG_CONTROLLER_LISTENER_
