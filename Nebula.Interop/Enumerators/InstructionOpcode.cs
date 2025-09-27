namespace Nebula.Interop.Enumerators
{
    public enum InstructionOpcode
    {
        /// <summary></summary>
        Nop,		// Empty operation

        /// <summary></summary>
        Pop,		// Pop the value on top of the stack
        /// <summary></summary>
        Dup,		// Duplicates value on top of stack

        /// <summary></summary>
        Call,		// Invoke function
        /// <summary></summary>
        CallVirt,	// Invoke instance function

        // Conversion

        /// <summary></summary>
        ConvType,	// Convert the top of the stack value from to [type]

        // Control flow 

        /// <summary></summary>
        Ret,		// Return control flow
        /// <summary></summary>
        Br,			// Jump always
        /// <summary> Jump if true </summary>
        BrTrue,
        /// <summary> Jump if false </summary>
        BrFalse,
        /// <summary> Compares two values on the stack and pushes 1 if they're equal otherwise 0 </summary>
        Ceq,
        /// <summary> Negates the value on top of the stack </summary>
        Neg,
        /// <summary></summary>
        Not,
        /// <summary></summary>
        And,
        /// <summary></summary>
        Or,
        /// <summary></summary>
        Xor,
        /// <summary></summary>
        Clt,
        /// <summary></summary>
        Cgt,

        // Threads
        Call_t,
        Wait,
        Wait_n,
        Notify,

        // Math
        Add,
        Sub,
        Mul,
        Div,
        /// <summary> Pushes the remainer of a division onto the stack </summary>
        Rem,

        // String
        AddStr,

        // Load
        Ldc_i4_0,	// Load constant i32 0
        Ldc_i4_1,	// Load constant i32 1
        Ldc_i4,		// Load constant i32
        Ldc_r4,		// Load constant f32
        Ldc_s,		// Load string constant
        Ld_b,       // Load bundle [ObjectNamespace] ObjectType
        NewArr,     // Load array DataStackIndex [ObjectNamespace] ObjectType

        // Load variables
        Ldarg,
        LdBarg,     // Read data in parameter bundle and put it on top of stack
        Ldloc,
        LdBloc,     // Read data in local bundle and put it on top of stack
        /// <summary>Loads the element at a specified array index onto the top of the evaluation stack as the type specified in the instruction.</summary>
        Ldelem,

        // Store
        Stloc,		// Store local variable
        StBloc,     // Store data in local bundle
        StArg,
        StBArg,
        /// <summary> Replaces the array element at a given index with the value on the evaluation stack </summary>
        StElem,

        LastInstruction
    }
}
