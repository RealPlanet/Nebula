#pragma once

#include <map>
#include <unordered_set>

#include "LanguageTypes.h"
#include "Variable.h"
#include "Bundle.h"

namespace nebula
{
	class IGCObject;
	class Script;
	class Interpreter;

	using AllocableObjectPtr = std::shared_ptr<IGCObject>;

	class InterpreterMemory
	{
	public:
		InterpreterMemory(Interpreter* parent);

		TBundle AllocBundle(const BundleDefinition& definition);
		TArray AllocArray(const DataStackVariantIndex& type);

		void Collect(bool force = false);
		void Sweep();
		bool Empty() { return m_IGCObjects.empty(); }

		void AddGlobals(const Script* script);
		Variable* GetGlobal(const std::string_view& namespaceStr, TInt32 index);

	private:
		Interpreter* m_pParent;
		std::list<AllocableObjectPtr> m_IGCObjects;
		size_t m_iGCThreshold;
		std::map<const std::string_view, std::vector< Variable>> m_ScriptGlobals{};
	};
}

