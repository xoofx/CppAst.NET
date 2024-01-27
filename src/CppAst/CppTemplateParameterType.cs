// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

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
            TemplateTemplateParameters = new List<CppType>();
        }

        public CppTemplateParameterType(string name, System.Collections.Generic.List<CppType> templateTemplateParams) : base(CppTypeKind.TemplateParameterType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TemplateTemplateParameters = templateTemplateParams;
        }

        /// <summary>
        /// Name of the template parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a list of the parameters.
        /// </summary>
        public List<CppType> TemplateTemplateParameters { get; }

        private bool Equals(CppTemplateParameterType other)
        {
            return base.Equals(other) && 
              Name.Equals(other.Name) &&
              TemplateTemplateParameters.SequenceEqual(other.TemplateTemplateParameters);
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
            return ReferenceEquals(this, obj) || obj is CppTemplateParameterType other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode() * 397 ^ Name.GetHashCode();
                foreach (var templateTemplateParameter in TemplateTemplateParameters)
                {
                    hashCode = (hashCode * 397) ^ templateTemplateParameter.GetHashCode();
                }

                return hashCode;
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType() => this;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}