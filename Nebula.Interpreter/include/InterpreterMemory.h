#pragma once

#include <unordered_set>

#include "LanguageTypes.h"
#include "Bundle.h"

namespace nebula
{
    class IGCObject;
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

    private:
        Interpreter* m_pParent;
        std::list<AllocableObjectPtr> m_IGCObjects;
        size_t m_iGCThreshold;
    };
}

