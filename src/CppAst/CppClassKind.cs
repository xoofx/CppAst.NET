// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Type of a <see cref="CppClass"/> (class, struct or union)
    /// </summary>
    public enum CppClassKind
    {
        /// <summary>
        /// A C++ `class`
        /// </summary>
        Class,
        /// <summary>
        /// A C++ `struct`
        /// </summary>
        Struct,
        /// <summary>
        /// A C++ `union`
        /// </summary>
        Union,
    }
}