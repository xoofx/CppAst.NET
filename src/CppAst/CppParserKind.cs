// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst;

public enum CppParserKind
{
    /// <summary>
    /// No parser kind defined.
    /// </summary>
    None = 0,
    /// <summary>
    /// Equivalent to -xc++. (Default)
    /// </summary>
    Cpp,
    /// <summary>
    /// Equivalent to -xc.
    /// </summary>
    C,
    /// <summary>
    /// Equivalent to -xobjective-c.
    /// </summary>
    ObjC
}