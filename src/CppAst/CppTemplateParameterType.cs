// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ template parameter type.
    /// </summary>
    public sealed class CppTemplateParameterType : CppType
    {
        /// <summary>
        /// Constructor of this template parameter type.
        /// </summary>
        /// <param name="name"></param>
        public CppTemplateParameterType(string name) : base(CppTypeKind.TemplateParameterType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Name of the template parameter.
        /// </summary>
        public string Name { get; }

        private bool Equals(CppTemplateParameterType other)
        {
            return base.Equals(other) && Name.Equals(other.Name);
        }

        public override int SizeOf
        {
            get => 0;

            set => throw new InvalidOperationException("This type does not support SizeOf");
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