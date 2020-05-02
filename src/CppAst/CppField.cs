// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ field (of a struct/class) or global variable.
    /// </summary>
    public sealed class CppField : CppDeclaration, ICppMemberWithVisibility
    {
        public CppField(CppType type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name;
        }

        /// <inheritdoc />
        public CppVisibility Visibility { get; set; }

        /// <summary>
        /// Gets or sets the storage qualifier of this field/variable.
        /// </summary>
        public CppStorageQualifier StorageQualifier { get; set; }

        /// <summary>
        /// Gets attached attributes. Might be null.
        /// </summary>
        public List<CppAttribute> Attributes { get; set; }

        /// <summary>
        /// Gets the type of this field/variable.
        /// </summary>
        public CppType Type { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this field was created from an anonymous type
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Gets the associated init value (either an integer or a string...)
        /// </summary>
        public CppValue InitValue { get; set; }

        /// <summary>
        /// Gets the associated init value as an expression.
        /// </summary>
        public CppExpression InitExpression { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating that this field is a bit field. See <see cref="BitFieldWidth"/> to get the width of this field if <see cref="IsBitField"/> is <c>true</c>
        /// </summary>
        public bool IsBitField { get; set; }

        /// <summary>
        /// Gets or sets the number of bits for this bit field. Only valid if <see cref="IsBitField"/> is <c>true</c>.
        /// </summary>
        public int BitFieldWidth { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Visibility != CppVisibility.Default)
            {
                builder.Append(Visibility.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            if (StorageQualifier != CppStorageQualifier.None)
            {
                builder.Append(StorageQualifier.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            builder.Append(Type.GetDisplayName());
            builder.Append(" ");
            builder.Append(Name);

            if (InitExpression != null)
            {
                builder.Append(" = ");
                var initExpressionStr = InitExpression.ToString();
                if (string.IsNullOrEmpty(initExpressionStr))
                {
                    builder.Append(InitValue);
                }
                else
                {
                    builder.Append(initExpressionStr);
                }
            }
            else if (InitValue != null)
            {
                builder.Append(" = ");
                builder.Append(InitValue);
            }

            return builder.ToString();
        }
    }
}