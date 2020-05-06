// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ array (e.g int[5] or int[])
    /// </summary>
    public sealed class CppArrayType : CppTypeWithElementType, IEquatable<CppArrayType>
    {
        /// <summary>
        /// Constructor of a C++ array.
        /// </summary>
        /// <param name="elementType">The element type (e.g `int`)</param>
        /// <param name="size">The size of the array. 0 means an unbound array</param>
        public CppArrayType(CppType elementType, int size) : base(CppTypeKind.Array, elementType)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the array.
        /// </summary>
        public int Size { get; }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => Size * ElementType.SizeOf;
            set => throw new InvalidOperationException("Cannot set the SizeOf an array type. The SizeOf is calculated by the SizeOf its ElementType and the number of elements in the fixed array");
        }

        /// <inheritdoc />
        public bool Equals(CppArrayType other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Size == other.Size;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppArrayType other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Size;
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            var elementTypeCanonical = ElementType.GetCanonicalType();
            if (ReferenceEquals(elementTypeCanonical, ElementType)) return this;
            return new CppArrayType(elementTypeCanonical, Size);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ElementType.GetDisplayName()}[{Size}]";
        }
    }
}