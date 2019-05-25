// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ typedef (e.g `typedef int XXX`)
    /// </summary>
    public sealed class CppTypedef : CppType, ICppMemberWithVisibility
    {
        /// <summary>
        /// Creates a new instance of a typedef.
        /// </summary>
        /// <param name="name">Name of the typedef (e.g `XXX`)</param>
        /// <param name="type">Underlying type.</param>
        public CppTypedef(string name, CppType type) : base(CppTypeKind.Typedef)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Visibility of this element.
        /// </summary>
        public CppVisibility Visibility { get; set; }

        /// <summary>
        /// Gets or sets the name of this type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the underlying type of this typedef.
        /// </summary>
        public CppType Type { get; }

        private bool Equals(CppTypedef other)
        {
            return base.Equals(other) && Name.Equals(other.Name) && Type.Equals(other.Type);
        }

        public override bool IsEquivalent(CppType other)
        {
            // Special case for typedef, as they are aliasing, we don't care about the name
            return Type.IsEquivalent(other);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTypedef other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"typedef {Type.GetDisplayName()} {Name}";
        }
    }
}