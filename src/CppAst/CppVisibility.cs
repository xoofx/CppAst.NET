// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Gets the visibility of a C++ element.
    /// </summary>
    public enum CppVisibility
    {
        /// <summary>
        /// Default visibility is undefined or not relevant.
        /// </summary>
        Default,

        /// <summary>
        /// `public` visibility
        /// </summary>
        Public,

        /// <summary>
        /// `protected` visibility
        /// </summary>
        Protected,

        /// <summary>
        /// `private` visibility
        /// </summary>
        Private,
    }
}