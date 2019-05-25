// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Extensions of <see cref="CppType"/>.
    /// </summary>
    public static class CppTypeExtension
    {
        /// <summary>
        /// Gets the display name of the specified type. If the type is <see cref="ICppMember"/> it will
        /// only use the name provided by <see cref="ICppMember.Name"/>
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The display name</returns>
        public static string GetDisplayName(this CppType type)
        {
            if (type is ICppMember member) return member.Name;
            return type.ToString();
        }
    }
}