// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// Base class for a type using an element type.
    /// </summary>
    public abstract class CppTypeWithElementType : CppType
    {
        protected CppTypeWithElementType(CppTypeKind typeKind, CppType elementType) : base(typeKind)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public CppType ElementType { get; }

        /// <inheritdoc />
        public override int SizeOf { get; set; }
    }
}