// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ qualified type (e.g `const int`)
    /// </summary>
    public sealed class CppQualifiedType : CppType
    {
        /// <summary>
        /// Constructor for a C++ qualified type.
        /// </summary>
        /// <param name="qualifier">The C++ qualified (e.g `const`)</param>
        /// <param name="elementType">The element type (e.g `int`)</param>
        public CppQualifiedType(CppTypeQualifier qualifier, CppType elementType) : base(CppTypeKind.Qualified)
        {
            Qualifier = qualifier;
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        /// <summary>
        /// Gets the qualifier
        /// </summary>
        public CppTypeQualifier Qualifier { get; }

        /// <summary>
        /// Gets the element type.
        /// </summary>
        public CppType ElementType { get; }

        public override string ToString()
        {
            return $"{Qualifier.ToString().ToLowerInvariant()} {ElementType.GetDisplayName()}";
        }

        private bool Equals(CppQualifiedType other)
        {
            return base.Equals(other) && Qualifier == other.Qualifier && ElementType.Equals(other.ElementType);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppQualifiedType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Qualifier;
                hashCode = (hashCode * 397) ^ ElementType.GetHashCode();
                return hashCode;
            }
        }
    }
}