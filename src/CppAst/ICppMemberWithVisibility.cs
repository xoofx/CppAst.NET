// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Interface of a <see cref="ICppMember"/> with a <see cref="CppVisibility"/>.
    /// </summary>
    public interface ICppMemberWithVisibility : ICppMember
    {
        /// <summary>
        /// Gets or sets the visibility of this element.
        /// </summary>
        CppVisibility Visibility { get; set; }
    }
}