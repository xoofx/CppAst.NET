// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// A C++ typedef (e.g `typedef int XXX`)
    /// </summary>
    public sealed class CppTypedef : CppTypeDeclaration, ICppMemberWithVisibility, ICppAttributeContainer
    {
        /// <summary>
        /// Creates a new instance of a typedef.
        /// </summary>
        /// <param name="name">Name of the typedef (e.g `XXX`)</param>
        /// <param name="type">Underlying type.</param>
        public CppTypedef(string name, CppType type) : base(CppTypeKind.Typedef)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ElementType = type;
            Attributes = new List<CppAttribute>();
            TokenAttributes = new List<CppAttribute>();
            MetaAttributes = new MetaAttributeMap();
        }

        public List<CppAttribute> Attributes { get; }

        [Obsolete("TokenAttributes is deprecated. please use system attributes and annotate attributes")]
        public List<CppAttribute> TokenAttributes { get; }

        public MetaAttributeMap MetaAttributes { get; private set; }
        
        public CppType ElementType { get; }

        /// <summary>
        /// Visibility of this element.
        /// </summary>
        public CppVisibility Visibility { get; set; }

        /// <summary>
        /// Gets or sets the name of this type.
        /// </summary>
        public string Name { get; set; }

        public override string FullName
        {
            get
            {
                string fullparent = FullParentName;
                if (string.IsNullOrEmpty(fullparent))
                {
                    return Name;
                }
                else
                {
                    return $"{fullparent}::{Name}";
                }
            }
        }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => ElementType.SizeOf;
            set => throw new InvalidOperationException("Cannot set the SizeOf a TypeDef. The SizeOf is determined by the underlying ElementType");
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            return ElementType.GetCanonicalType();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"typedef {ElementType.GetDisplayName()} {Name}";
        }
    }
}