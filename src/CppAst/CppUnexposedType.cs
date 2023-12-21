// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// A type not fully/correctly exposed by the C++ parser.
    /// </summary>
    /// <remarks>
    /// Template parameter type instance are actually exposed with this type.
    /// </remarks>
    public sealed class CppUnexposedType : CppType, ICppTemplateOwner
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="name">Fullname of the unexposed type</param>
        public CppUnexposedType(string name) : base(CppTypeKind.Unexposed)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TemplateParameters = new List<CppType>();
        }

        /// <summary>
        /// Full name of the unexposed type
        /// </summary>
        public string Name { get; }

        private bool Equals(CppTemplateParameterType other)
        {
            return base.Equals(other) && Name.Equals(other.Name);
        }

        /// <inheritdoc />
        public override int SizeOf { get; set; }

        /// <inheritdoc />
        public List<CppType> TemplateParameters { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTemplateParameterType other && Equals(other);
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
        public override CppType GetCanonicalType() => this;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}