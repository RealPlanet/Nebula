#pragma once

#ifndef _H_DEBUG_BUNDLE_DEFINITION_
#define _H_DEBUG_BUNDLE_DEFINITION_

#include "DebugVariable.h"

#include <string>
#include <vector>

namespace nebula::debugger::symbols
{
	struct DebugBundleDefinition
	{
		std::string name{};
		std::vector<DebugVariable> fields{};
	};
}

#endif // !_H_DEBUG_BUNDLE_DEFINITION_
