// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// Flags attached to a <see cref="CppFunction"/>
    /// </summary>
    [Flags]
    public enum CppFunctionFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,

        /// <summary>
        /// The function is `const`
        /// </summary>
        Const = 1 << 0,

        /// <summary>
        /// The method is defaulted.
        /// </summary>
        Defaulted = 1 << 1,
        
        /// <summary>
        /// The method is pure (`= 0`)
        /// </summary>
        Pure = 1 << 2,

        /// <summary>
        /// The method is declared `virtual`.
        /// </summary>
        Virtual = 1 << 3,
    }
}