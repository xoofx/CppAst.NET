// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ standard or scoped enum.
    /// </summary>
    public sealed class CppEnum : CppTypeDeclaration, ICppMemberWithVisibility
    {
        /// <summary>
        /// Creates a new instance of this enum.
        /// </summary>
        /// <param name="name">Name of this enum</param>
        public CppEnum(string name) : base(CppTypeKind.Enum)
        {
            Name = name;
            Items = new CppContainerList<CppEnumItem>(this);
            Attributes = new CppContainerList<CppAttribute>(this);
        }

        /// <inheritdoc />
        public CppVisibility Visibility { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this enum is scoped.
        /// </summary>
        public bool IsScoped { get; set; }


        /// <summary>
        /// Gets or sets the underlying integer type of this enum.
        /// </summary>
        public CppType IntegerType { get; set; }

        /// <summary>
        /// Gets the definition of the enum items.
        /// </summary>
        public CppContainerList<CppEnumItem> Items { get; }

        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Gets the list of attached attributes.
        /// </summary>
        public CppContainerList<CppAttribute> Attributes { get; }

        private bool Equals(CppEnum other)
        {
            return base.Equals(other) && Equals(Parent, other.Parent) && Equals(Name, other.Name);
        }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => IntegerType?.SizeOf ?? 0;
            set => throw new InvalidOperationException("Cannot set the SizeOf an enum as it is determined only by the SizeOf of its underlying IntegerType");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppEnum other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType() => IntegerType;

        /// <inheritdoc />
        public override IEnumerable<ICppDeclaration> Children() => Items;

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Visibility != CppVisibility.Default)
            {
                builder.Append(Visibility.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            builder.Append("enum ");
            if (IsScoped)
            {
                builder.Append("class ");
            }

            builder.Append(Name);

            if (IntegerType != null && !(IntegerType is CppPrimitiveType primitive && primitive.Kind == CppPrimitiveKind.Int))
            {
                builder.Append(": ");
                builder.Append(IntegerType.GetDisplayName());
            }

            builder.Append(" {...}");
            return builder.ToString();
        }
    }
}