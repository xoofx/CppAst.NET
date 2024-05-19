// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Xml.Linq;

namespace CppAst
{
    /// <summary>
    /// For c++ specialized template argument
    /// </summary>
    public class CppTemplateArgument : CppType
    {
        public CppTemplateArgument(CppType sourceParam, CppType typeArg, bool isSpecializedArgument) : base(CppTypeKind.TemplateArgumentType)
        {
            SourceParam = sourceParam ?? throw new ArgumentNullException(nameof(sourceParam));
            ArgAsType = typeArg ?? throw new ArgumentNullException(nameof(typeArg));
            ArgKind = CppTemplateArgumentKind.AsType;
            IsSpecializedArgument = isSpecializedArgument;
        }

        public CppTemplateArgument(CppType sourceParam, long intArg) : base(CppTypeKind.TemplateArgumentType)
        {
			SourceParam = sourceParam ?? throw new ArgumentNullException(nameof(sourceParam));
            ArgAsInteger = intArg;
            ArgKind = CppTemplateArgumentKind.AsInteger;
            IsSpecializedArgument = true;
        }

		public CppTemplateArgument(CppType sourceParam, string unknownStr) : base(CppTypeKind.TemplateArgumentType)
        {
			SourceParam = sourceParam ?? throw new ArgumentNullException(nameof(sourceParam));
            ArgAsUnknown = unknownStr;
            ArgKind = CppTemplateArgumentKind.Unknown;
            IsSpecializedArgument = true;
        }

        public CppTemplateArgumentKind ArgKind { get; }

        public CppType ArgAsType { get; }

        public long ArgAsInteger { get; }

        public string ArgAsUnknown { get; }

        public string ArgString
        {
            get
            {
                switch (ArgKind)
                {
                    case CppTemplateArgumentKind.AsType:
                        return ArgAsType.FullName;
                    case CppTemplateArgumentKind.AsInteger:
                        return ArgAsInteger.ToString();
                    case CppTemplateArgumentKind.Unknown:
                        return ArgAsUnknown;
                    default:
                        return "?";
                }
            }
        }


        /// <summary>
        /// Gets the default value.
        /// </summary>
        public CppType SourceParam { get; }

        public bool IsSpecializedArgument { get; }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => 0;
            set => throw new InvalidOperationException("This type does not support SizeOf");
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType() => this;

        /// <inheritdoc />


        /// <inheritdoc />
        public override string ToString() => $"{SourceParam} = {ArgString}";
    }
}