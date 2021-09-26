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

        /// <summary>
        /// This is a C++ method
        /// </summary>
        Method = 1 << 4,

        /// <summary>
        /// This is a C++ function or method with inline attribute
        /// </summary>
        Inline = 1 << 5,

        /// <summary>
        /// This is a C++ constructor
        /// </summary>
        Constructor = 1 << 6,

        /// <summary>
        /// This is a C++ destructor
        /// </summary>
        Destructor = 1 << 7,

        /// <summary>
        /// This is a variadic function (has `...` parameter)
        /// </summary>
        Variadic = 1 << 8,
    }
}