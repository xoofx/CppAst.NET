// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ reference type (e.g `int&amp;`)
    /// </summary>
    public sealed class CppReferenceType : CppType
    {
        /// <summary>
        /// Constructor of a reference type.
        /// </summary>
        /// <param name="elementType">The element type referenced to.</param>
        public CppReferenceType(CppType elementType) : base(CppTypeKind.Reference)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        /// <summary>
        /// Gets the element type referenced by this reference type.
        /// </summary>
        public CppType ElementType { get; }

        private bool Equals(CppReferenceType other)
        {
            return base.Equals(other) && ElementType.Equals(other.ElementType);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppReferenceType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ElementType.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{ElementType.GetDisplayName()}&";
        }
    }
}