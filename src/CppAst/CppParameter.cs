// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ function parameter.
    /// </summary>
    public sealed class CppParameter : CppDeclaration, ICppMember
    {
        /// <summary>
        /// Creates a new instance of a C++ function parameter.
        /// </summary>
        /// <param name="type">Type of the parameter.</param>
        /// <param name="name">Name of the parameter</param>
        public CppParameter(CppType type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the type of this parameter.
        /// </summary>
        public CppType Type { get; set; }

        /// <summary>
        /// Gets the name of this parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public CppValue InitValue { get; set; }

        /// <summary>
        /// Gets or sets the default value as an expression.
        /// </summary>
        public CppExpression InitExpression { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return InitExpression != null ? $"{Type.GetDisplayName()} = {InitExpression}" : $"{Type.GetDisplayName()}";
            }

            return InitExpression != null ? $"{Type.GetDisplayName()} {Name} = {InitExpression}" : $"{Type.GetDisplayName()} {Name}";
        }
    }
}