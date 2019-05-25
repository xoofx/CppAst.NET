// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ array (e.g int[5] or int[])
    /// </summary>
    public sealed class CppArrayType : CppType, IEquatable<CppArrayType>
    {
        /// <summary>
        /// Constructor of a C++ array.
        /// </summary>
        /// <param name="elementType">The element type (e.g `int`)</param>
        /// <param name="size">The size of the array. 0 means an unbound array</param>
        public CppArrayType(CppType elementType, int size) : base(CppTypeKind.Array)
        { 
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            Size = size;
        }

        /// <summary>
        /// Gets the type of the array element.
        /// </summary>
        public CppType ElementType { get; }

        /// <summary>
        /// Gets the size of the array.
        /// </summary>
        public int Size { get; }

        public bool Equals(CppArrayType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ElementType.Equals(other.ElementType) && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppArrayType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ElementType.GetHashCode();
                hashCode = (hashCode * 397) ^ Size;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{ElementType.GetDisplayName()}[{Size}]";
        }
    }
}