// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Base class for C++ types.
    /// </summary>
    public abstract class CppType : CppElement
    {
        /// <summary>
        /// Constructor with the specified type kind.
        /// </summary>
        /// <param name="typeKind"></param>
        protected CppType(CppTypeKind typeKind)
        {
            TypeKind = typeKind;
        }

        /// <summary>
        /// Gets the <see cref="CppTypeKind"/> of this instance.
        /// </summary>
        public CppTypeKind TypeKind { get; }

        public abstract int SizeOf { get; set; }

        protected bool Equals(CppType other)
        {
            return TypeKind == other.TypeKind;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CppType type && Equals(type);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)TypeKind;
        }

        /// <summary>
        /// Gets the canonical type of this type instance.
        /// </summary>
        /// <returns>A canonical type of this type instance</returns>
        public abstract CppType GetCanonicalType();
    }
}