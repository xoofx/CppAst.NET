// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst;

/// <summary>
/// Represents a header inclusion directive.
/// </summary>
public class CppInclusionDirective : CppElement
{
    /// <summary>
    /// Gets or sets the file name being included.
    /// </summary>
    public string FileName { get; set; }

    /// <inheritdoc />
    public override string ToString() => FileName ?? "<empty>";
}