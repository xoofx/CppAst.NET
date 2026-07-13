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
    public sealed class CppUnexposedType : CppType, ICppTemplateOwner, ICppContainer
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

        /// <inheritdoc />
        public override int SizeOf { get; set; }

        /// <summary>
        /// Gets the template argument types referenced by this unexposed type.
        /// </summary>
        /// <remarks>
        /// This list does not own its elements and does not change their <see cref="CppElement.Parent"/>.
        /// </remarks>
        public List<CppType> TemplateParameters { get; }

        IList<CppType> ICppTemplateOwner.TemplateParameters => TemplateParameters;

        /// <inheritdoc />
        public override CppType GetCanonicalType() => this;

        /// <inheritdoc />
        public override string ToString() => Name;

        public IEnumerable<ICppDeclaration> Children()
        {
            yield break;
        }
    }
}
