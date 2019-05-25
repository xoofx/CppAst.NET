// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Defines the type of storage.
    /// </summary>
    public enum CppStorageQualifier
    {
        /// <summary>
        /// No storage defined.
        /// </summary>
        None,
        /// <summary>
        /// Extern storage
        /// </summary>
        Extern,
        /// <summary>
        /// Static storage.
        /// </summary>
        Static,
    }
}