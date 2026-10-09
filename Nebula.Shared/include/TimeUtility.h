#pragma once

#ifndef _H_NEBULA_TIME_UTILITY_
#define _H_NEBULA_TIME_UTILITY_

#include <chrono>

namespace nebula
{
	inline unsigned long long GetCurrentMillis()
	{
		auto milliseconds = std::chrono::duration_cast<std::chrono::milliseconds>(
			std::chrono::high_resolution_clock::now().time_since_epoch())
			.count();
		return milliseconds;
	}
} // namespace nebula

#endif // !_H_NEBULA_TIME_UTILITY_
