// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ template parameter type.
    /// </summary>
    public sealed class CppTemplateParameterNonType : CppType
    {
        /// <summary>
        /// Constructor of this none type template parameter type.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="templateNonType"></param>
        public CppTemplateParameterNonType(string name, CppType templateNonType) : base(CppTypeKind.TemplateParameterNonType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            NoneTemplateType = templateNonType ?? throw new ArgumentNullException(nameof(templateNonType));
        }

        /// <summary>
        /// Name of the template parameter.
        /// </summary>
        public string Name { get; }

        public CppType NoneTemplateType { get; }

        private bool Equals(CppTemplateParameterNonType other)
        {
            return base.Equals(other) && Name.Equals(other.Name) && NoneTemplateType.Equals(other.NoneTemplateType);
        }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => 0;
            set => throw new InvalidOperationException("This type does not support SizeOf");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTemplateParameterNonType other && Equals(other);
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
        public override string ToString() => $"{NoneTemplateType.ToString()} {Name}";
    }
}