// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// An attached C++ attribute
    /// </summary>
    public class CppAttribute : CppElement
    {
        public CppAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets or sets the scope of this attribute
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets the attribute name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the attribute arguments
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether this attribute is variadic
        /// </summary>
        public bool IsVariadic { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Scope != null)
            {
                builder.Append(Scope).Append("::");
            }

            builder.Append(Name);
            if (Arguments != null)
            {
                builder.Append("(").Append(Arguments).Append(")");
            }

            if (IsVariadic)
            {
                builder.Append("...");
            }

            return builder.ToString();
        }
    }
}