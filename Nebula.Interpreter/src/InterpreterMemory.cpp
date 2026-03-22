#include "InterpreterMemory.h"
#include "Bundle.h"
#include "VariantArray.h"
#include "Interpreter.h"

#include <stack>

using namespace nebula;

constexpr size_t g_MinGCThreshold = 128;

static void GatherStackRoots(Interpreter* vm, std::vector<AllocableObjectPtr>& foundRoots)
{
    const ThreadMap& tm = vm->GetThreadMap();
    size_t threadCount = tm.Count();
    for (int i = 0; i < threadCount; i++)
    {
        const CallStack& t = tm.At(i);


        size_t frameCount = t.size();

        [[unlikely]]
        if (frameCount == 0)
            continue;

        for (int f{ 0 }; f < frameCount; f++)
        {
            const Frame* rootFrame = t[f];
            const FrameMemory& memory = rootFrame->Memory();

            size_t localCount = memory.LocalCount();
            for (int j{ 0 }; j < localCount; j++)
            {
                const Variable& fv = memory.LocalAt(j);
                if (const TGCObject* obj = std::get_if<TGCObject>(&fv.Value()))
                {
                    foundRoots.push_back(*obj);
                }
            }

            const DataStack& stack = rootFrame->Stack();
            auto it = stack.begin();
            while (it != stack.end())
            {
                const DataStackVariant& variant = *it;
                if (const TGCObject* obj = std::get_if<TGCObject>(&variant))
                {
                    foundRoots.push_back(*obj);
                }

                it++;
            }
        }

    }
}

InterpreterMemory::InterpreterMemory(Interpreter* parent)
    : m_pParent{ parent }, m_IGCObjects{}, m_iGCThreshold{ g_MinGCThreshold }
{
}

TBundle InterpreterMemory::AllocBundle(const BundleDefinition& definition)
{
    // Attempt to free memory at each allocation
    Collect();
    // This shared pointer is passed around function frames
    TBundle ptr = Bundle::FromDefinition(definition);
    // Keep track of the allocated objectsw
    m_IGCObjects.push_back(dynamic_pointer_cast<IGCObject>(ptr));

    return ptr;
}

TArray InterpreterMemory::AllocArray(const DataStackVariantIndex& type)
{
    // Attempt to free memory at each allocation
    Collect();
    TArray ptr = std::make_shared<VariantArray>(type);
    m_IGCObjects.push_back(dynamic_pointer_cast<IGCObject>(ptr));

    return ptr;
}

void InterpreterMemory::Collect(bool force)
{
    Interpreter* vm = m_pParent;
    size_t startingSize = m_IGCObjects.size();

    if (force || startingSize >= m_iGCThreshold)
    {
        std::vector<AllocableObjectPtr> roots;
        GatherStackRoots(vm, roots);

        std::stack<AllocableObjectPtr> grayObjects;

        for (int i{ 0 }; i < roots.size(); i++)
        {
            roots[i]->m_bIsMarked = true;
            grayObjects.push(roots[i]);
        }

        while (!grayObjects.empty())
        {
            AllocableObjectPtr ptr = grayObjects.top();
            grayObjects.pop();

            if (TBundle bundle = std::dynamic_pointer_cast<Bundle>(ptr))
            {
                for (int f{ 0 }; f < bundle->FieldCount(); f++)
                {
                    auto& variant = bundle->Get(f);
                    if (variant.index() == DataStackVariantIndex::_TypeObject)
                    {
                        AllocableObjectPtr childPtr = std::get<TGCObject>(variant);
                        if (childPtr->m_bIsMarked)
                            continue;

                        childPtr->m_bIsMarked = true;
                        grayObjects.push(childPtr);
                    }
                }
            }
        }

        Sweep();

        size_t reductionAmount = startingSize - m_IGCObjects.size();
        size_t quarter = startingSize / 4;
        if (reductionAmount < quarter)
        {
            m_iGCThreshold *= 2;
        }
        else if (reductionAmount > quarter * 3)
        {
            m_iGCThreshold /= 2;
        }

        if (m_iGCThreshold < g_MinGCThreshold)
            m_iGCThreshold = g_MinGCThreshold;
    }
}

void InterpreterMemory::Sweep()
{
    auto it = m_IGCObjects.begin();
    while (it != m_IGCObjects.end())
    {
        AllocableObjectPtr obj = *it;
        // We didn't reach it, so release it
        if (!obj->m_bIsMarked)
        {
            if (TBundle bundle = std::dynamic_pointer_cast<Bundle>(obj))
            {
                bundle->ClearFields();
            }

            it = m_IGCObjects.erase(it);
        }
        else
        {
            // reset for next gc run
            obj->m_bIsMarked = false;
            it++;
        }
    }
}

void InterpreterMemory::AddGlobals(const Script* script)
{
    std::vector<Variable> variables{};
    variables.reserve(script->Globals().size());

    for (auto& globalType : script->Globals())
    {
        variables.emplace_back(globalType);
    }
    m_ScriptGlobals[script->Namespace()] = variables;
}

Variable* InterpreterMemory::GetGlobal(const std::string_view& script, TInt32 index)
{
    auto it = m_ScriptGlobals.find(script);
    if (it == m_ScriptGlobals.end())
    {
        return nullptr;
    }

    return &it->second.at(index);
}
