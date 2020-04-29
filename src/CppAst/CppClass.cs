// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ class, struct or union.
    /// </summary>
    public sealed class CppClass : CppTypeDeclaration, ICppMemberWithVisibility, ICppDeclarationContainer, ICppTemplateOwner
    {
        /// <summary>
        /// Creates a new instance. 
        /// </summary>
        /// <param name="name">Name of this type.</param>
        public CppClass(string name) : base(CppTypeKind.StructOrClass)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseTypes = new List<CppBaseType>();
            Fields = new CppContainerList<CppField>(this);
            Constructors = new CppContainerList<CppFunction>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            TemplateParameters = new List<CppType>();
            Attributes = new CppContainerList<CppAttribute>(this);
        }

        /// <summary>
        /// Kind of the instance (`class` `struct` or `union`)
        /// </summary>
        public CppClassKind ClassKind { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public CppVisibility Visibility { get; set; }

        /// <inheritdoc />
        public CppContainerList<CppAttribute> Attributes { get; }

        /// <summary>
        /// Gets or sets a boolean indicating if this type is a definition. If <c>false</c> the type was only declared but is not defined.
        /// </summary>
        public bool IsDefinition { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this declaration is anonymous.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Get the base types of this type.
        /// </summary>
        public List<CppBaseType> BaseTypes { get; }

        /// <inheritdoc />
        public CppContainerList<CppField> Fields { get; }

        /// <summary>
        /// Gets the constructors of this instance.
        /// </summary>
        public CppContainerList<CppFunction> Constructors { get; set; }

        /// <inheritdoc />
        public CppContainerList<CppFunction> Functions { get; }

        /// <inheritdoc />
        public CppContainerList<CppEnum> Enums { get; }

        /// <inheritdoc />
        public CppContainerList<CppClass> Classes { get; }

        /// <inheritdoc />
        public CppContainerList<CppTypedef> Typedefs { get; }

        /// <inheritdoc />
        public List<CppType> TemplateParameters { get; }

        private bool Equals(CppClass other)
        {
            return base.Equals(other) && Equals(Parent, other.Parent) && Name.Equals(other.Name);
        }

        /// <inheritdoc />
        public override int SizeOf { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppClass other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            switch (ClassKind)
            {
                case CppClassKind.Class:
                    builder.Append("class");
                    break;
                case CppClassKind.Struct:
                    builder.Append("struct");
                    break;
                case CppClassKind.Union:
                    builder.Append("union");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (string.IsNullOrEmpty(Name))
            {
                builder.Append(Name);
            }

            if (BaseTypes.Count > 0)
            {
                builder.Append(" : ");
                for (var i = 0; i < BaseTypes.Count; i++)
                {
                    var baseType = BaseTypes[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(baseType);
                }
            }

            builder.Append(" { ... }");
            return builder.ToString();
        }

        public override IEnumerable<ICppDeclaration> Children()
        {
            foreach (var item in CppContainerHelper.Children(this))
            {
                yield return item;
            }

            foreach (var item in Constructors)
            {
                yield return item;
            }
        }
    }
}