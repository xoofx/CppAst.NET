// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ token used by <see cref="CppMacro"/>.
    /// </summary>
    public class CppToken : CppElement
    {
        /// <summary>
        /// Creates a new instance of a C++ token.
        /// </summary>
        /// <param name="kind">Kind of this token</param>
        /// <param name="text">Text of this token</param>
        public CppToken(CppTokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        /// <summary>
        /// Gets or sets the kind of this token.
        /// </summary>
        public CppTokenKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the text of this token.
        /// </summary>
        public string Text { get; set; }

        /// <inheritdoc />
        public override string ToString() => Text;

        public static string TokensToString(IEnumerable<CppToken> tokens)
        {
            var builder = new StringBuilder();
            CppTokenKind previousKind = 0;
            foreach (var token in tokens)
            {
                if (token.Kind == CppTokenKind.Comment) continue;

                // If previous token and new token are identifiers/keyword, we need a space between them
                if (previousKind.IsIdentifierOrKeyword() && token.Kind.IsIdentifierOrKeyword())
                {
                    builder.Append(" ");
                }
                builder.Append(token.Text);
                previousKind = token.Kind;
            }

            return builder.ToString();
        }
    }
}