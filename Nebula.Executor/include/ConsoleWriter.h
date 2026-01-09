#pragma once

#ifndef _H_CONSOLE_WRITER_
#define _H_CONSOLE_WRITER_

#include <iostream>
#include <string>

namespace nebula::frontend::writer
{

    enum Code {
        FG_RED = 31,
        FG_GREEN = 32,
        FG_BLUE = 34,

        FG_DEFAULT = 39,

        BG_RED = 41,
        BG_GREEN = 42,
        BG_BLUE = 44,
        BG_DEFAULT = 49
    };

    void ConsoleWrite(const char* text, Code color)
    {
#ifdef _WIN32
        std::cout << "\033[" << color << "m" << text << "\033[0m\n";
#else
        std::cout << text << "\n";
#endif
    }

    void ConsoleWrite(std::string msg, Code color) { ConsoleWrite(msg.data(), color); }

}

#endif // !_H_CONSOLE_WRITER_

