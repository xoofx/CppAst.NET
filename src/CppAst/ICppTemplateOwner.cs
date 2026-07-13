// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// Base interface of a type/method declared with template parameters.
    /// </summary>
    public interface ICppTemplateOwner
    {
        /// <summary>
        /// Gets the list of template parameters.
        /// </summary>
        /// <remarks>
        /// The list may either own declared template parameters or reference template argument types owned elsewhere.
        /// </remarks>
        IList<CppType> TemplateParameters { get; }
    }
}
