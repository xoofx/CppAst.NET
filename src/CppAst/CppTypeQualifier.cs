// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Qualifiers for a <see cref="CppQualifiedType"/>
    /// </summary>
    public enum CppTypeQualifier
    {
        /// <summary>
        /// The type is `const`
        /// </summary>
        Const,

        /// <summary>
        /// The type is `volatile`
        /// </summary>
        Volatile,
    }
}