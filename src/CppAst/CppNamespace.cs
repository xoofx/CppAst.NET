// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// Defines a C++ namespace. This is only one level of namespace (e.g `A` in `A::B::C`)
    /// </summary>
    public class CppNamespace : CppDeclaration, ICppMember, ICppGlobalDeclarationContainer
    {
        /// <summary>
        /// Creates a namespace with the specified name.
        /// </summary>
        /// <param name="name">Name of the namespace.</param>
        public CppNamespace(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Fields = new CppContainerList<CppField>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            Namespaces = new CppContainerList<CppNamespace>(this);
            Attributes = new List<CppAttribute>();
            TokenAttributes = new List<CppAttribute>();
            Properties = new CppContainerList<CppProperty>(this);
        }

        /// <summary>
        /// Name of the namespace.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is the namespace inline or not(such as std::__1::vector).
        /// </summary>
        public bool IsInlineNamespace { get; set; }

        /// <inheritdoc />
        public CppContainerList<CppField> Fields { get; }

        /// <inheritdoc />
        public CppContainerList<CppProperty> Properties { get; }

        /// <inheritdoc />
        public CppContainerList<CppFunction> Functions { get; }

        /// <inheritdoc />
        public CppContainerList<CppEnum> Enums { get; }

        /// <inheritdoc />
        public CppContainerList<CppClass> Classes { get; }

        /// <inheritdoc />
        public CppContainerList<CppTypedef> Typedefs { get; }

        /// <inheritdoc />
        public CppContainerList<CppNamespace> Namespaces { get; }

        /// <inheritdoc />
        public List<CppAttribute> Attributes { get; }
        [Obsolete("TokenAttributes is deprecated. please use system attributes and annotate attributes")]
        public List<CppAttribute> TokenAttributes { get; }

        public MetaAttributeMap MetaAttributes { get; private set; } = new MetaAttributeMap();

        public override string ToString()
        {
            return $"namespace {Name} {{...}}";
        }

        public IEnumerable<ICppDeclaration> Children()
        {
            foreach (var item in CppContainerHelper.Children(this))
            {
                yield return item;
            }
        }
    }
}