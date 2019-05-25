// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// Attribute flags attached to a <see cref="CppFunction"/>
    /// </summary>
    [Flags]
    public enum CppAttributeFlags
    {
        /// <summary>
        /// No attributes.
        /// </summary>
        None = 0,

        /// <summary>
        /// DllImport attribute.
        /// </summary>
        DllImport = 1 << 0,

        /// <summary>
        /// DllExport attribute
        /// </summary>
        DllExport = 1 << 0,
    }
}