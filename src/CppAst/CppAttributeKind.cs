// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// Attribute kind enum here
    /// </summary>
    public enum AttributeKind
    {
        CxxSystemAttribute,
        ////CxxCustomAttribute,
        AnnotateAttribute,
        CommentAttribute,
        TokenAttribute,         //the attribute is parse from token, and the parser is slow.
    }
}