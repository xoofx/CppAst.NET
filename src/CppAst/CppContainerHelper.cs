// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace CppAst
{
    /// <summary>
    /// Internal helper class for visiting children
    /// </summary>
    internal static class CppContainerHelper
    {
        public static IEnumerable<ICppDeclaration> Children(ICppGlobalDeclarationContainer container)
        {
            foreach (var item in container.Enums)
            {
                yield return item;
            }

            foreach (var item in container.Classes)
            {
                yield return item;
            }

            foreach (var item in container.Typedefs)
            {
                yield return item;
            }

            foreach (var item in container.Fields)
            {
                yield return item;
            }

            foreach (var item in container.Functions)
            {
                yield return item;
            }

            foreach (var item in container.Namespaces)
            {
                yield return item;
            }
        }

        public static IEnumerable<ICppDeclaration> Children(ICppDeclarationContainer container)
        {
            foreach (var item in container.Enums)
            {
                yield return item;
            }

            foreach (var item in container.Classes)
            {
                yield return item;
            }

            foreach (var item in container.Typedefs)
            {
                yield return item;
            }

            foreach (var item in container.Fields)
            {
                yield return item;
            }

            foreach (var item in container.Functions)
            {
                yield return item;
            }
        }
    }
}