// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A type not fully/correctly exposed by the C++ parser.
    /// </summary>
    /// <remarks>
    /// Template parameter type instance are actually exposed with this type.
    /// </remarks>
    public sealed class CppUnexposedType : CppType
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="name">Fullname of the unexposed type</param>
        public CppUnexposedType(string name) : base(CppTypeKind.Unexposed)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Full name of the unexposed type
        /// </summary>
        public string Name { get; }

        private bool Equals(CppTemplateParameterType other)
        {
            return base.Equals(other) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTemplateParameterType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public override CppType GetCanonicalType()
        {
            return this;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}