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

        /// <summary>
        /// Gets the canonical type of this type instance.
        /// </summary>
        /// <returns>A canonical type of this type instance</returns>
        public abstract CppType GetCanonicalType();

        /// <summary>
        /// We can use this name in exporter to use this type.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return ToString();
            }
        }
    }
}