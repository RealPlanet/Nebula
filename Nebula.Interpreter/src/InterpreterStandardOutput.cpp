#include "InterpreterStandardOutput.h"

#include <iostream>

using namespace nebula;

void InterpreterStandardOutput::WriteLine(const std::string& c)
{
    std::cout << c << "\n";
}

void InterpreterStandardOutput::Write(const std::string& c)
{
    std::cout << c;
}
