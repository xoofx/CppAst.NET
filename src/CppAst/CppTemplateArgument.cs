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
        //Nested types
		public enum CppTemplateArgumentKind
		{
			AsType,
			AsInteger,
			Unknown,
		}

		public CppTemplateArgument(CppType source_param, CppType type_arg)
        {
            SourceParam = source_param ?? throw new ArgumentNullException(nameof(source_param));
            ArgAsType = type_arg ?? throw new ArgumentNullException(nameof(type_arg));
            ArgKind = CppTemplateArgumentKind.AsType;
        }

		public CppTemplateArgument(CppType source_param, long int_arg)
		{
			SourceParam = source_param ?? throw new ArgumentNullException(nameof(source_param));
            ArgAsInteger = int_arg;
			ArgKind = CppTemplateArgumentKind.AsInteger;
		}

		public CppTemplateArgument(CppType source_param, string unknown_str)
        {
			SourceParam = source_param ?? throw new ArgumentNullException(nameof(source_param));
            ArgAsUnknown = unknown_str;
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