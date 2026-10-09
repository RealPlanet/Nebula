#pragma once

#include "GCUser.h"
#include "NotificationListener.h"

#include <map>

namespace nebula
{
	class Frame;
	class GCUser;

	// Enables control of a Frame state (messaging and waiting)
	class FrameScheduler
		: public NotificationListener
	{
	public:
		FrameScheduler(Frame* parent);

		// Freeze the owned thread for X milliseconds
		void Sleep(size_t amount);
		void Kill();

		void WaitForNotification(GCUser* notifier, const std::string& str);

		void EndOnNotification(GCUser* notifier, const std::string& str);

		// Returns true if the frame is sleeping, will also check time passed and clear flag
		bool IsSleeping();
		bool HasBeenKilled() const { return m_Killed; }

		virtual bool OnNotification(GCUser* sender, const size_t notification) override;
	private:
		Frame* m_Parent;
		size_t m_SleepAmount{ 0 };
		bool m_Killed{ false };
		std::map<GCUser*, std::unordered_set<size_t>> m_WaitingHashes;
		std::map<GCUser*, std::unordered_set<size_t>> m_WaitingEndonHashes;

		bool FindAndRemoveWaitingHash(GCUser* sender, const size_t notification);
		bool FindAndRemoveEndonHash(GCUser* sender, const size_t notification);
	};
}

