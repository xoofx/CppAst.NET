// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ field (of a struct/class) or global variable.
    /// </summary>
    public sealed class CppField : CppElement, ICppMemberWithVisibility
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
        /// Gets the type of this field/variable.
        /// </summary>
        public CppType Type { get; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <summary>
        /// Gets the associated default value.
        /// </summary>
        public CppValue DefaultValue { get; set; } 

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

            if (DefaultValue?.Value != null)
            {
                builder.Append(" = ");
                builder.Append(DefaultValue);
            }

            return builder.ToString();
        }
    }
}