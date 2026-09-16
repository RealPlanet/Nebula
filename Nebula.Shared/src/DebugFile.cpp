#include "DebugFile.h"

using namespace nebula::debugger::symbols;

const DebugFunction* DebugFile::GetFunctionInformation(const std::string& funcName) const
{
	auto it = functions.find(funcName);
	if (it != functions.end())
	{
		return &it->second;
	}

	return nullptr;
}
