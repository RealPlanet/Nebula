#pragma once

#include "InstructionDefs.h"

namespace Nebula::CodeEmitter
{
    public enum class InstructionOpcode
    {
        Nop = nebula::VMInstruction::Nop,

        Pop = nebula::VMInstruction::Pop,
        Dup = nebula::VMInstruction::Dup,

        Call = nebula::VMInstruction::Call,
        // Conversion
        ConvType = nebula::VMInstruction::ConvType,

        // Control flow
        Ret = nebula::VMInstruction::Ret,
        Br = nebula::VMInstruction::Br,
        Brtrue = nebula::VMInstruction::BrTrue,
        Brfalse = nebula::VMInstruction::BrFalse,
        Ceq = nebula::VMInstruction::Ceq,

        // Boolean
        Neg = nebula::VMInstruction::Neg,
        Not = nebula::VMInstruction::Not,
        And = nebula::VMInstruction::And,
        Or = nebula::VMInstruction::Or,
        Xor = nebula::VMInstruction::Xor,
        Clt = nebula::VMInstruction::Clt,
        Cgt = nebula::VMInstruction::Cgt,

        // Threads
        Call_t = nebula::VMInstruction::Call_t,
        Wait = nebula::VMInstruction::Wait,
        Wait_n = nebula::VMInstruction::Wait_n,
        Notify = nebula::VMInstruction::Notify,

        // Math
        Add = nebula::VMInstruction::Add,
        Sub = nebula::VMInstruction::Sub,
        Mul = nebula::VMInstruction::Mul,
        Div = nebula::VMInstruction::Div,
        Rem = nebula::VMInstruction::Rem,
        
        // String
        AddStr = nebula::VMInstruction::AddStr,

        // Load
        Ldc_i4_0    = nebula::VMInstruction::Ldc_i4_0,
        Ldc_i4_1    = nebula::VMInstruction::Ldc_i4_1,
        Ldc_i4      = nebula::VMInstruction::Ldc_i4,
        Ldc_r4      = nebula::VMInstruction::Ldc_r4,
        Ldc_s       = nebula::VMInstruction::Ldc_s,
        Ld_b        = nebula::VMInstruction::Ld_b,

        // Load variables
        Ldarg       = nebula::VMInstruction::Ldarg,
        LdBarg      = nebula::VMInstruction::LdBarg,
        Ldloc       = nebula::VMInstruction::Ldloc,
        LdBloc      = nebula::VMInstruction::LdBloc,

        // Store
        Stloc       = nebula::VMInstruction::Stloc,
        StBloc      = nebula::VMInstruction::StBloc,
        StArg       = nebula::VMInstruction::StArg,
        StBArg      = nebula::VMInstruction::StBArg,
    };
}