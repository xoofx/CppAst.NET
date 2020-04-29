// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// A C++ qualified type (e.g `const int`)
    /// </summary>
    public sealed class CppQualifiedType : CppTypeWithElementType
    {
        /// <summary>
        /// Constructor for a C++ qualified type.
        /// </summary>
        /// <param name="qualifier">The C++ qualified (e.g `const`)</param>
        /// <param name="elementType">The element type (e.g `int`)</param>
        public CppQualifiedType(CppTypeQualifier qualifier, CppType elementType) : base(CppTypeKind.Qualified, elementType)
        {
            Qualifier = qualifier;
            SizeOf = elementType.SizeOf;
        }

        /// <summary>
        /// Gets the qualifier
        /// </summary>
        public CppTypeQualifier Qualifier { get; }

        private bool Equals(CppQualifiedType other)
        {
            return base.Equals(other) && Qualifier == other.Qualifier;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppQualifiedType other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int)Qualifier;
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            var elementTypeCanonical = ElementType.GetCanonicalType();
            return ReferenceEquals(elementTypeCanonical, ElementType) ? this : new CppQualifiedType(Qualifier, elementTypeCanonical);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Qualifier.ToString().ToLowerInvariant()} {ElementType.GetDisplayName()}";
        }
    }
}