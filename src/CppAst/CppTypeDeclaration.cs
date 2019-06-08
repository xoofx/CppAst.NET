// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace CppAst
{
    /// <summary>
    /// Base class for a type declaration (<see cref="CppClass"/>, <see cref="CppEnum"/>, <see cref="CppFunctionType"/> or <see cref="CppTypedef"/>)
    /// </summary>
    public abstract class CppTypeDeclaration : CppType, ICppDeclaration, ICppContainer
    {
        protected CppTypeDeclaration(CppTypeKind typeKind) : base(typeKind)
        {
        }

        /// <inheritdoc />
        public CppComment Comment { get; set; }

        /// <inheritdoc />
        public virtual IEnumerable<ICppDeclaration> Children()
        {
            return Enumerable.Empty<ICppDeclaration>();
        }
    }
}