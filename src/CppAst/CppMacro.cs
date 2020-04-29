// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ Macro, only valid if the parser is initialized with <see cref="CppParserOptions.ParseMacros"/>
    /// </summary>
    public class CppMacro : CppElement, ICppMember
    {
        /// <summary>
        /// Creates a new instance of a macro.
        /// </summary>
        /// <param name="name"></param>
        public CppMacro(string name)
        {
            Name = name;
            Tokens = new List<CppToken>();
        }

        /// <summary>
        /// Gets or sets the name of the macro.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parameters of this macro (e.g `param1` and `param2` in `#define MY_MACRO(param1, param2)`)
        /// </summary>
        public List<string> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the tokens of the value of the macro. The full string of the tokens is accessible via the <see cref="Value"/> property.
        /// </summary>
        /// <remarks>
        /// If tokens are updated, you need to call <see cref="UpdateValueFromTokens"/>
        /// </remarks>
        public List<CppToken> Tokens { get; }

        /// <summary>
        /// Gets a textual representation of the token values of this macro.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Call this method to update the <see cref="Value"/> property from the list of <see cref="Tokens"/>
        /// </summary>
        public void UpdateValueFromTokens()
        {
            Value = CppToken.TokensToString(Tokens);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Name);
            if (Parameters != null)
            {
                builder.Append("(");
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var parameter = Parameters[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(parameter);
                }

                builder.Append(")");
            }

            builder.Append(" = ").Append(Value);
            return builder.ToString();
        }
    }
}