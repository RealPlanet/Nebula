#pragma once

#include <unordered_set>

#include "LanguageTypes.h"
#include "Bundle.h"

namespace nebula
{
    class AllocatedObject;
    class Interpreter;

    using AllocableObjectPtr = std::shared_ptr<AllocatedObject>;

    class InterpreterMemory
    {
    public:
        InterpreterMemory(Interpreter* parent);

        TBundle AllocBundle(const BundleDefinition& definition);

        void Collect(bool force = false);
        void Sweep();
        bool Empty() { return m_AllocatedObjects.empty(); }

    private:
        Interpreter* m_pParent;
        std::list<AllocableObjectPtr> m_AllocatedObjects;
        size_t m_iGCThreshold;
    };
}

