#include "DebugServer.h"

using namespace nebula::debugger;

DebugServer* DebugServer::m_pInstance{ nullptr };

DebugServer* DebugServer::Instance()
{
	return m_pInstance;
}

void DebugServer::RegisterDebugServer(DebugServer* Instance)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}

	m_pInstance = Instance;
}
