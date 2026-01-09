#pragma once
#ifndef _NEBULA_HEADER_UTILS_H_
#define _NEBULA_HEADER_UTILS_H_

#include <string>
#include <sstream>
#include <iostream>
#include <chrono>

#ifdef SDVM_COMPILE_DLL
#define _NEBULA_API __declspec(dllexport)
#else
#define _NEBULA_API __declspec(dllimport)
#endif // !SDVM_COMPILE_DLL

#ifndef _NEBULA_TAB 
#define _NEBULA_TAB "    "
#endif // !_NEBULA_TAB 

#define NEBULA_UNUSED_PARAM(x) (void*)x

namespace nebula {
    /// <summary>
    /// Calculates the length of a string at compile time
    /// </summary>
    inline constexpr size_t cstrlen(const char* str) {
        return std::char_traits<char>::length(str);
    }

    template<typename T2, typename T1>
    inline T2 lexical_cast(const T1& in) {
        T2 out;
        std::stringstream ss;
        ss << in;
        ss >> out;
        return out;
    }

    template<typename T2, typename T1>
    inline T2 hex_lexical_cast(const T1& in) {
        T2 out;
        std::stringstream ss;
        ss << std::hex << in;
        ss >> out;
        return out;
    }

    inline unsigned long long GetCurrentMillis() {
        auto milliseconds
            = std::chrono::duration_cast<std::chrono::milliseconds>(
                std::chrono::high_resolution_clock::now().time_since_epoch())
            .count();
        return milliseconds;
    }

    inline std::string trim(const std::string& str)
    {
        size_t first = str.find_first_not_of(' ');
        if (std::string::npos == first)
        {
            return str;
        }
        size_t last = str.find_last_not_of(' ');
        return str.substr(first, (last - first + 1));
    }
}

#endif // !_NEBULA_HEADER_UTILS_H_


