// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// A <see cref="ICppContainer"/> that can contain also namespaces.
    /// </summary>
    /// <seealso cref="CppNamespace"/>
    /// <seealso cref="CppCompilation"/>
    /// <seealso cref="CppGlobalDeclarationContainer"/>
    public interface ICppGlobalDeclarationContainer : ICppDeclarationContainer
    {
        /// <summary>
        /// Gets the declared namespaces
        /// </summary>
        CppContainerList<CppNamespace> Namespaces { get; }
    }
}