#pragma once

#include <memory>

namespace Nebula::Interop
{
    template <class T>
    public ref class SharedPtrW
    {
    public:
        SharedPtrW(std::shared_ptr<T>& t)
        {
            m_Ptr = new std::shared_ptr<T>(t);
        };

        ~SharedPtrW() { SharedPtrW::!SharedPtrW(); }
        !SharedPtrW() { delete m_Ptr; }

        T* operator->() {
            return (*m_Ptr).get();
        }
    private:
        std::shared_ptr<T>* m_Ptr;
    };
}

