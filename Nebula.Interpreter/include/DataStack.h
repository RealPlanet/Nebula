#pragma once

#include <vector>

#include "LanguageTypes.h"

namespace nebula
{
    // The stack of a singular function call
    class DataStack
    {
    public:
        DataStack() = default;
        ~DataStack() = default;
        DataStack(DataStack&&) = default;
        DataStack(const DataStack&) = delete;

        inline DataStackVariant&    Peek()                  { return m_Data.back(); }
        inline DataStackVariant&    Peek(size_t offset)     { return m_Data[m_Data.size() - 1 - offset]; }
        inline void                 Pop()                   { return m_Data.pop_back(); }
        inline size_t               Size() const            { return m_Data.size(); }
        inline void                 Reserve(size_t newCap)  { m_Data.reserve(newCap); }

        inline void Dup()                               { m_Data.emplace_back(m_Data.back()); }
        inline void Push(TInt32 v)                      { m_Data.emplace_back(v); }
        inline void Push(TFloat v)                      { m_Data.emplace_back(v); }
        //inline void Push(TByte v)                       { m_Data.emplace_back(v); }
        inline void Push(const TString& v)              { m_Data.emplace_back(v); }
        inline void Push(const TBundle& v)              { m_Data.emplace_back(v); }
        inline void Push(const DataStackVariant& v)     { m_Data.emplace_back(v); }

        std::vector<DataStackVariant>::iterator begin() { return m_Data.begin(); }
        std::vector<DataStackVariant>::iterator  end() { return m_Data.end(); }
       
    private:
        std::vector<DataStackVariant> m_Data;
    };
}
