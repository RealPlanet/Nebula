#include "InterpreterMemory.h"
#include "Bundle.h"
#include "Interpreter.h"

using namespace nebula;

constexpr size_t g_MinGCThreshold = 128;

static void GetGarbageCollectionRoots(const ThreadMap& threads,
    std::unordered_set<AllocableObjectPtr>& allocatedObjects,
    std::unordered_set<AllocableObjectPtr>& out)
{
    // All the reachable allocated objects in the callstacks are considered roots
    // and stack memory
    for (int callStackIndex = 0; callStackIndex < threads.Count(); callStackIndex++)
    {
        const CallStack& cs = threads.At(callStackIndex);
        if (cs.size() == 0)
            continue;

        for (int frameIndex{ 0 }; frameIndex < cs.size(); frameIndex++)
        {
            Frame* frame = cs.at(frameIndex);
            FrameMemory& mem = frame->Memory();

            for (int varIndex = 0; varIndex < mem.LocalCount(); varIndex++)
            {

                FrameVariable& fVar = mem.LocalAt(varIndex);
                DataStackVariant& variant = fVar.Value();

                if (TBundle* pBundle = std::get_if<TBundle>(&variant))
                {
                    AllocableObjectPtr* basePtr = (AllocableObjectPtr*)(pBundle);
                    out.insert(*basePtr);
                }
            }

            for (auto& it : frame->Stack())
            {
                if (TBundle* pBundle = std::get_if<TBundle>(&it))
                {
                    AllocableObjectPtr* basePtr = (AllocableObjectPtr*)(pBundle);
                    out.insert(*basePtr);
                }
            }
        }
    }

    // Now remove roots which are pointeed at by other bundles (roots must have no parents)
    // Cyclic dependencies will be removed from roots and be freed later
    for (auto it = allocatedObjects.begin(); it != allocatedObjects.end(); it++)
    {
        AllocableObjectPtr ptr = *it;

        if (TBundle bundle = dynamic_pointer_cast<Bundle>(ptr))
        {
            for (int i = 0; i < bundle->FieldCount(); i++)
            {
                auto& fieldValue = bundle->Get(i);
                if (TBundle* pBundle = std::get_if<TBundle>(&fieldValue))
                {
                    AllocableObjectPtr* basePtr = (AllocableObjectPtr*)(pBundle);
                    out.erase(*basePtr);
                }
            }
        }

        if (it == allocatedObjects.end())
            break;
    }
}

static void TraverseTree(AllocableObjectPtr& ptr, std::unordered_set<AllocableObjectPtr>& reachable) {
    if (reachable.find(ptr) != reachable.end()) {
        return; // Already visited
    }

    reachable.insert(ptr);
    if (TBundle bundle = dynamic_pointer_cast<Bundle>(ptr))
    {
        for (int i = 0; i < bundle->FieldCount(); i++)
        {
            auto& fieldValue = bundle->Get(i);
            if (TBundle* pBundle = std::get_if<TBundle>(&fieldValue))
            {
                AllocableObjectPtr* basePtr = (AllocableObjectPtr*)(pBundle);
                TraverseTree(*basePtr, reachable);
            }
        }
    }
}

InterpreterMemory::InterpreterMemory(Interpreter* parent)
    : m_pParent{ parent }, m_AllocatedObjects{}, m_iGCThreshold{ g_MinGCThreshold }
{
}

TBundle InterpreterMemory::AllocBundle(const BundleDefinition& definition)
{
    // This shared pointer is passed around function frames
    TBundle ptr = Bundle::FromDefinition(definition);
    // Keep track of the allocated objects
    m_AllocatedObjects.insert(dynamic_pointer_cast<AllocatedObject>(ptr));
    return ptr;
}

void InterpreterMemory::Collect(bool force)
{
    if (m_AllocatedObjects.empty())
        return;
    size_t startingSize = m_AllocatedObjects.size();

    if (force || startingSize >= m_iGCThreshold)
    {

#if _DEBUG
        m_pParent->StandardOutput()->WriteLine("******* GC COLLECTION *******");
        size_t time = GetCurrentMillis();
#endif

        ReleaseUnreferencedObjects();

        std::unordered_set<AllocableObjectPtr> roots;
        const ThreadMap& threads = m_pParent->GetThreadMap();

        // Fetch all allocated objects in the callstacks
        GetGarbageCollectionRoots(threads, m_AllocatedObjects, roots);

        if (m_AllocatedObjects.size() == 0)
            return;

        // We now have many trees for each root where each node is another allocable object
        // We visit each node and take note of them.

        std::unordered_set<AllocableObjectPtr> reachableNodes;
        for (AllocableObjectPtr ptr : roots)
        {
            TraverseTree(ptr, reachableNodes);
        }

        // Now we calculate which nodes have not been reached by the tree navigation
        // And release them from memory
        std::unordered_set<AllocableObjectPtr> unreachableObjects;
        std::unordered_set<AllocableObjectPtr> unreachableChildren;
        for (auto it = m_AllocatedObjects.begin(); it != m_AllocatedObjects.end(); it++)
        {
            AllocableObjectPtr ptr = *it;
            if (reachableNodes.find(ptr) != reachableNodes.end())
                continue;

            if (unreachableChildren.find(ptr) != unreachableChildren.end())
                continue;

            unreachableObjects.insert(ptr);

            std::unordered_set<AllocableObjectPtr> children;
            TraverseTree(ptr, children);

            for (AllocableObjectPtr child : children)
            {
                // it's a circular loop so one must be the root we picked!
                if (child == ptr)
                    continue;

                unreachableChildren.insert(child);
                // We are about to de-allocate this so remove it from our pool
                m_AllocatedObjects.erase(child);
            }
        }

        // By breaking one link in each circular dependency we trigger the de-allocation of the entire ring
        for (AllocableObjectPtr ptr : unreachableObjects)
        {
            // We are deallocating this one so remove it from the ppol
            m_AllocatedObjects.erase(ptr);
            if (TBundle bundle = dynamic_pointer_cast<Bundle>(ptr))
            {
                bundle->ClearFields();
            }
        }

        size_t reductionAmount = startingSize - m_AllocatedObjects.size();
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

#if _DEBUG
        roots.clear();
        reachableNodes.clear();
        unreachableObjects.clear();
        unreachableChildren.clear();

        size_t endTime = GetCurrentMillis();
        m_pParent->StandardOutput()->WriteLine("******* GC COLLECTION END *******");
        m_pParent->StandardOutput()->WriteLine(std::format("Took: {} ms", endTime - time));
        m_pParent->StandardOutput()->WriteLine("********************************");
#endif        
    }
}

void InterpreterMemory::ReleaseUnreferencedObjects()
{
    for (auto it = m_AllocatedObjects.begin(); it != m_AllocatedObjects.end(); it++)
    {
        const AllocableObjectPtr& ptr = *it;
        if (ptr.use_count() == 1)
        {
            // The GC is holding this in memory, release it
            it = m_AllocatedObjects.erase(it);
        }

        if (it == m_AllocatedObjects.end())
            break;
    }
}
