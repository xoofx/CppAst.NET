// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// An enum item of <see cref="CppEnum"/>.
    /// </summary>
    public sealed class CppEnumItem : CppDeclaration, ICppMember
    {
        /// <summary>
        /// Creates a new instance of this enum item.
        /// </summary>
        /// <param name="name">Name of the enum item.</param>
        /// <param name="value">Associated value of this enum item.</param>
        public CppEnumItem(string name, long value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <summary>
        /// Gets the value of this enum item.
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// Gets the value of this enum item as an expression.
        /// </summary>
        public CppExpression ValueExpression { get; set; }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} = {ValueExpression}";
        }
    }
}