// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// A range of source location.
    /// </summary>
    public struct CppSourceSpan
    {
        /// <summary>
        /// Constructor of a range of source location.
        /// </summary>
        /// <param name="start">Start of the range</param>
        /// <param name="end">End of the range</param>
        public CppSourceSpan(CppSourceLocation start, CppSourceLocation end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets or sets the beginning of the range source
        /// </summary>
        public CppSourceLocation Start;

        /// <summary>
        /// Gets or sets the end of the range source
        /// </summary>
        public CppSourceLocation End;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Start}";
        }
    }
}