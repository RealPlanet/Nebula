#pragma once
#ifndef _NEBULA_HEADER_UTILS_H_
#define _NEBULA_HEADER_UTILS_H_

#include <sstream>
#include <iostream>
#include <cassert>
#include <memory>
#include <chrono>

#include <vector>

#include <functional>

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

    //inline size_t Read7BitEncodedInt64(std::string_view& view, size_t& offset)
    //{
    //    size_t result = 0;
    //    char byteReadJustNow;
    //
    //    // Read the integer 7 bits at a time. The high bit
    //    // of the byte when on means to continue reading more bytes.
    //    //
    //    // There are two failure cases: we've read more than 10 bytes,
    //    // or the tenth byte is about to cause integer overflow.
    //    // This means that we can read the first 9 bytes without
    //    // worrying about integer overflow.
    //
    //    const int MaxBytesWithoutOverflow = 9;
    //    for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
    //    {
    //        // ReadByte handles end of stream cases for us.
    //        byteReadJustNow = view[offset++];
    //        result |= (byteReadJustNow & 0x7Ful) << shift;
    //
    //        if (byteReadJustNow <= 0x7Fu)
    //        {
    //            return (long)result; // early exit
    //        }
    //    }
    //
    //    // Read the 10th byte. Since we already read 63 bits,
    //    // the value of this byte must fit within 1 bit (64 - 63),
    //    // and it must not have the high bit set.
    //
    //    byteReadJustNow = view[offset++];
    //    if (byteReadJustNow > 0)
    //    {
    //        throw std::exception();
    //    }
    //
    //    result |= (size_t)byteReadJustNow << MaxBytesWithoutOverflow * 7;
    //    return result;
    //}
}

#endif // !_NEBULA_HEADER_UTILS_H_


