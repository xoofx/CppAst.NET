// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// A C++ declaration that has a name
    /// </summary>
    public interface ICppMember : ICppElement
    {
        /// <summary>
        /// Name of this C++ declaration.
        /// </summary>
        string Name { get; set; }
    }
}