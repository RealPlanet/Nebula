#pragma once

#include "InstructionDefs.h"

namespace Nebula::CodeEmitter
{
    public enum class NativeAttribute
    {
        Uknown = nebula::VMAttribute::uknown,
        AutoExec = nebula::VMAttribute::autoexec,
        ExecAfter = nebula::VMAttribute::execAfter,
        AutoGenerated = nebula::VMAttribute::autogenerated,
    };
}