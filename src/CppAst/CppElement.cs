// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Base class for all Cpp elements of the AST nodes.
    /// </summary>
    public abstract class CppElement : ICppElement
    {
        /// <summary>
        /// Gets or sets the source span of this element.
        /// </summary>
        public CppSourceSpan Span;

        /// <summary>
        /// Gets or sets the parent container of this element. Might be null.
        /// </summary>
        public ICppContainer Parent { get; internal set; }

        /// <summary>
        /// Gets the source file of this element.
        /// </summary>
        public string SourceFile => string.IsNullOrWhiteSpace(Span.Start.File) ? (Parent as CppElement)?.SourceFile : Span.Start.File;

    }
}