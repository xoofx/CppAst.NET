// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ function type (e.g `void (*)(int arg1, int arg2)`)
    /// </summary>
    public sealed class CppFunctionType : CppTypeDeclaration
    {
        /// <summary>
        /// Constructor of a function type.
        /// </summary>
        /// <param name="returnType">Return type of this function type.</param>
        public CppFunctionType(CppType returnType) : base(CppTypeKind.Function)
        {
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            Parameters = new CppContainerList<CppParameter>(this);
        }

        /// <summary>
        /// Gets or sets the calling convention of this function type.
        /// </summary>
        public CppCallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Gets or sets the return type of this function type.
        /// </summary>
        public CppType ReturnType { get; set; }

        /// <summary>
        /// Gets a list of the parameters.
        /// </summary>
        public CppContainerList<CppParameter> Parameters { get; }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => 0;

            set => throw new InvalidOperationException("This type does not support SizeOf");
        }

        /// <inheritdoc />
        public override IEnumerable<ICppDeclaration> Children() => Parameters;

        /// <inheritdoc />
        public override CppType GetCanonicalType() => this;

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(ReturnType.GetDisplayName());
            builder.Append(" ");
            builder.Append("(*)(");
            for (var i = 0; i < Parameters.Count; i++)
            {
                var param = Parameters[i];
                if (i > 0) builder.Append(", ");
                builder.Append(param);
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}