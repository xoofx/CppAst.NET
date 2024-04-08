// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// An enum item of <see cref="CppEnum"/>.
    /// </summary>
    public sealed class CppEnumItem : CppDeclaration, ICppMember, ICppAttributeContainer
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
        public List<CppAttribute> Attributes { get; } = new List<CppAttribute>();

        [Obsolete("TokenAttributes is deprecated. please use system attributes and annotate attributes")]
        public List<CppAttribute> TokenAttributes { get; } = new List<CppAttribute>();

        public MetaAttributeMap MetaAttributes { get; private set; } = new MetaAttributeMap();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} = {ValueExpression}";
        }
    }
}