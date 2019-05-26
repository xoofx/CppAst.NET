// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ function type (e.g `void (*)(int arg1, int arg2)`)
    /// </summary>
    public sealed class CppFunctionType : CppType
    {
        /// <summary>
        /// Constructor of a function type.
        /// </summary>
        /// <param name="returnType">Return type of this function type.</param>
        public CppFunctionType(CppType returnType) : base(CppTypeKind.Function)
        {
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ParameterTypes = new List<CppType>();
        }

        /// <summary>
        /// Gets or sets the return type of this function type.
        /// </summary>
        public CppType ReturnType { get; set; }
        
        /// <summary>
        /// Gets the types of the function parameters.
        /// </summary>
        public List<CppType> ParameterTypes { get; }

        private bool Equals(CppFunctionType other)
        {
            return base.Equals(other) && ReturnType.Equals(other.ReturnType) && ParameterTypes.SequenceEqual(other.ParameterTypes);

        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppFunctionType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
                foreach (var parameterType in ParameterTypes)
                {
                    hashCode = (hashCode * 397) ^ parameterType.GetHashCode();
                }
                return hashCode;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(ReturnType.GetDisplayName());
            builder.Append(" ");
            builder.Append("(*)(");
            for (var i = 0; i < ParameterTypes.Count; i++)
            {
                var param = ParameterTypes[i];
                if (i > 0) builder.Append(", ");
                builder.Append(param);
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}