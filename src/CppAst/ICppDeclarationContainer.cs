// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Base interface of a <see cref="ICppContainer"/> containing fields, functions, enums, classes, typedefs.
    /// </summary>
    /// <seealso cref="CppClass"/>
    public interface ICppDeclarationContainer : ICppContainer
    {
        /// <summary>
        /// Gets the fields/variables.
        /// </summary>
        CppContainerList<CppField> Fields { get; }

        /// <summary>
        /// Gets the functions/methods.
        /// </summary>
        CppContainerList<CppFunction> Functions { get; }

        /// <summary>
        /// Gets the enums.
        /// </summary>
        CppContainerList<CppEnum> Enums { get; }

        /// <summary>
        /// Gets the classes, structs.
        /// </summary>
        CppContainerList<CppClass> Classes { get; }

        /// <summary>
        /// Gets the typedefs.
        /// </summary>
        CppContainerList<CppTypedef> Typedefs { get; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        CppContainerList<CppAttribute> Attributes { get; }
    }
}