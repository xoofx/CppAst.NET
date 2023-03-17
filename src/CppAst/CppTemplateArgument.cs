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
    public class CppTemplateArgument : CppElement
    {
		public CppTemplateArgument(CppType sourceParam, CppType typeArg)
        {
            SourceParam = sourceParam ?? throw new ArgumentNullException(nameof(sourceParam));
            ArgAsType = typeArg ?? throw new ArgumentNullException(nameof(typeArg));
            ArgKind = CppTemplateArgumentKind.AsType;
        }

		public CppTemplateArgument(CppType sourceParam, long intArg)
		{
			SourceParam = sourceParam ?? throw new ArgumentNullException(nameof(sourceParam));
            ArgAsInteger = intArg;
			ArgKind = CppTemplateArgumentKind.AsInteger;
		}

		public CppTemplateArgument(CppType sourceParam, string unknownStr)
        {
			SourceParam = sourceParam ?? throw new ArgumentNullException(nameof(sourceParam));
            ArgAsUnknown = unknownStr;
			ArgKind = CppTemplateArgumentKind.Unknown;
		}

		public CppTemplateArgumentKind ArgKind { get; }

        public CppType ArgAsType { get; }

        public long ArgAsInteger { get; }

        public string ArgAsUnknown { get; }

        public string ArgString
        {
            get
            {
                switch(ArgKind)
                {
                    case CppTemplateArgumentKind.AsType:
                        return ArgAsType.ToString();
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ SourceParam.GetHashCode() ^ ArgString.GetHashCode();
            }
        }


        /// <inheritdoc />
        public override string ToString() => $"{SourceParam} = {ArgString}";
    }
}