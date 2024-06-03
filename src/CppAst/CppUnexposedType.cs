// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <inheritdoc />
        public override int SizeOf { get; set; }

        /// <inheritdoc />
        public List<CppType> TemplateParameters { get; } = new List<CppType>();

        public List<CppTemplateArgument> TemplateArguments { get; } = new List<CppTemplateArgument>();

        /// <inheritdoc />
        public override CppType GetCanonicalType() => this;

        /// <inheritdoc />
        public override string ToString() => Name;

        public void UpdateTemplateArguments(CppType sourceParam, List<CppTemplateArgument> templateArguments)
        {
            // We need to remove all previously registered template arguments corresponding with 'sourceParam'
            // and update it with the new template arguments. The reason is that we "re-resolved" them 
            // in another layer (one layer above in the callstack) so we have a "better" resolution of them
            TemplateArguments.RemoveAll(arg => arg.SourceParam.Equals(sourceParam));
            TemplateArguments.AddRange(templateArguments);
        }
    }
}