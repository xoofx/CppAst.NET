// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// A base Cpp container for macros, classes, fields, functions, enums, typesdefs. 
    /// </summary>
    public class CppGlobalDeclarationContainer : CppElement, ICppGlobalDeclarationContainer
    {
        /// <summary>
        /// Create a new instance of this container.
        /// </summary>
        public CppGlobalDeclarationContainer()
        {
            Macros = new List<CppMacro>();
            Fields = new CppContainerList<CppField>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            Namespaces = new CppContainerList<CppNamespace>(this);
        }

        /// <summary>
        /// Gets the macros defines for this container.
        /// </summary>
        /// <remarks>
        /// Macros are only available if <see cref="CppParserOptions.ParseMacros"/> is <c>true</c>
        /// </remarks>
        public List<CppMacro> Macros { get;  }

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

        public virtual IEnumerable<CppElement> Children()
        {
            foreach (var item in Macros)
            {
                yield return item;
            }
            
            foreach (var item in CppContainerHelper.Children(this))
            {
                yield return item;
            }
        }
    }
}