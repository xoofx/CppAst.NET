// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ reference type (e.g `int&amp;`)
    /// </summary>
    public sealed class CppReferenceType : CppTypeWithElementType
    {
        /// <summary>
        /// Constructor of a reference type.
        /// </summary>
        /// <param name="elementType">The element type referenced to.</param>
        public CppReferenceType(CppType elementType) : base(CppTypeKind.Reference, elementType)
        {
        }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => ElementType.SizeOf;
            set => throw new InvalidOperationException("Cannot override the SizeOf of a Reference type");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ElementType.GetDisplayName()}&";
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            var elementTypeCanonical = ElementType.GetCanonicalType();
            return ReferenceEquals(elementTypeCanonical, ElementType) ? this : new CppReferenceType(elementTypeCanonical);
        }
    }
}