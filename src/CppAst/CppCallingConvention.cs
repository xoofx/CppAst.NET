// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// The calling function of a <see cref="CppFunction"/> or <see cref="CppFunctionType"/>
    /// </summary>
    public enum CppCallingConvention
    {
        Default,
        C,
        X86StdCall,
        X86FastCall,
        X86ThisCall,
        X86Pascal,
        AAPCS,
        AAPCS_VFP,
        X86RegCall,
        IntelOclBicc,
        Win64,
        X86_64SysV,
        X86VectorCall,
        Swift,
        PreserveMost,
        PreserveAll,
        AArch64VectorCall,
        Invalid,
        Unexposed,
    }
}