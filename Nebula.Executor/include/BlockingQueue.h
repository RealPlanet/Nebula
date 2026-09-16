#pragma once

#ifndef _H_BLOCKING_QUEUE_
#define _H_BLOCKING_QUEUE_

#include <queue>
#include <mutex>
#include <condition_variable>

namespace nebula::collections
{
	template <typename T> class BlockingQueue
	{
	public:
		void Push(const T& item)
		{
			{
				std::unique_lock<std::mutex> lock(m_sync);
				m_internalQueue.push(item);
			}

			m_cvCanPop.notify_one();
		}

		bool Pop(T& item)
		{
			std::unique_lock<std::mutex> lock(m_sync);
			for (;;)
			{
				if (m_internalQueue.empty())
				{
					if (m_bShutdown)
					{
						return false;
					}
				}
				else
				{
					break;
				}

				m_cvCanPop.wait(lock);
			}
			item = std::move(m_internalQueue.front());
			m_internalQueue.pop();
			return true;
		}

		void RequestShutdown()
		{
			{
				std::unique_lock<std::mutex> lock(m_sync);
				m_bShutdown = true;
			}
			m_cvCanPop.notify_all();
		}

	private:
		std::condition_variable m_cvCanPop;
		std::mutex m_sync;
		std::queue<T> m_internalQueue;
		bool m_bShutdown = false;
	};
} // namespace nebula::collections

#endif // !_H_BLOCKING_QUEUE_
