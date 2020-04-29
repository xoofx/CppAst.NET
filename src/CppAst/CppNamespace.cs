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
            Attributes = new CppContainerList<CppAttribute>(this);
        }

        /// <summary>
        /// Name of the namespace.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        public CppContainerList<CppField> Fields { get; }

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
        public CppContainerList<CppAttribute> Attributes { get; }

        protected bool Equals(CppNamespace other)
        {
            return Equals(Parent, other.Parent) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CppNamespace)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Parent != null ? Parent.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

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