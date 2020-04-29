// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// C++ primitive kinds used by <see cref="CppPrimitiveType"/>
    /// </summary>
    public enum CppPrimitiveKind
    {
        /// <summary>
        /// C++ `void`
        /// </summary>
        Void,

        /// <summary>
        /// C++ `bool`
        /// </summary>
        Bool,

        /// <summary>
        /// C++ `wchar`
        /// </summary>
        WChar,

        /// <summary>
        /// C++ `char`
        /// </summary>
        Char,

        /// <summary>
        /// C++ `short`
        /// </summary>
        Short,

        /// <summary>
        /// C++ `int`
        /// </summary>
        Int,

        /// <summary>
        /// C++ `long long` (64bits)
        /// </summary>
        LongLong,

        /// <summary>
        /// C++ `unsigned char`
        /// </summary>
        UnsignedChar,

        /// <summary>
        /// C++ `unsigned short`
        /// </summary>
        UnsignedShort,

        /// <summary>
        /// C++ `unsigned int`
        /// </summary>
        UnsignedInt,

        /// <summary>
        /// C++ `unsigned long long` (64 bits)
        /// </summary>
        UnsignedLongLong,

        /// <summary>
        /// C++ `float`
        /// </summary>
        Float,

        /// <summary>
        /// C++ `double`
        /// </summary>
        Double,

        /// <summary>
        /// C++ `long double`
        /// </summary>
        LongDouble,
    }
}