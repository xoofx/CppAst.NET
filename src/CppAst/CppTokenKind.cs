// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Kind of a <see cref="CppToken"/> used by <see cref="CppMacro"/>
    /// </summary>
    public enum CppTokenKind
    {
        /// <summary>
        /// A punctuation token (e.g `=`)
        /// </summary>
        Punctuation,

        /// <summary>
        /// A keyword token (e.g `for`)
        /// </summary>
        Keyword,

        /// <summary>
        /// An identifier token (e.g `my_variable`)
        /// </summary>
        Identifier,

        /// <summary>
        /// A literal token (e.g `15` or `"my string"`)
        /// </summary>
        Literal,

        /// <summary>
        /// A comment token
        /// </summary>
        Comment,
    }
}