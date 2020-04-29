// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ typedef (e.g `typedef int XXX`)
    /// </summary>
    public sealed class CppTypedef : CppTypeDeclaration, ICppMemberWithVisibility
    {
        /// <summary>
        /// Creates a new instance of a typedef.
        /// </summary>
        /// <param name="name">Name of the typedef (e.g `XXX`)</param>
        /// <param name="type">Underlying type.</param>
        public CppTypedef(string name, CppType type) : base(CppTypeKind.Typedef)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ElementType = type;
        }

        public CppType ElementType { get; }

        /// <summary>
        /// Visibility of this element.
        /// </summary>
        public CppVisibility Visibility { get; set; }

        /// <summary>
        /// Gets or sets the name of this type.
        /// </summary>
        public string Name { get; set; }

        private bool Equals(CppTypedef other)
        {
            return base.Equals(other) && string.Equals(Name, other.Name);
        }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => ElementType.SizeOf;
            set => throw new InvalidOperationException("Cannot set the SizeOf a TypeDef. The SizeOf is determined by the underlying ElementType");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTypedef other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            return ElementType.GetCanonicalType();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"typedef {ElementType.GetDisplayName()} {Name}";
        }
    }
}