// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Defines the target CPU used to compile a header file.
    /// </summary>
    public enum CppTargetCpu
    {
        /// <summary>
        /// The x86 CPU family (32bit)
        /// </summary>
        X86,

        /// <summary>
        /// The X86_64 CPU family (64bit)
        /// </summary>
        X86_64,

        /// <summary>
        /// The ARM CPU family (32bit)
        /// </summary>
        ARM,

        /// <summary>
        /// The ARM 64 CPU family (64bit)
        /// </summary>
        ARM64
    }
}