// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class CppExtensions
    {
        /// <summary>
        /// Gets a boolean indicating whether this token kind is an identifier or keyword
        /// </summary>
        /// <param name="kind">The token kind</param>
        /// <returns><c>true</c> if the token is an identifier or keyword, <c>false</c> otherwise</returns>
        public static bool IsIdentifierOrKeyword(this CppTokenKind kind)
        {
            return kind == CppTokenKind.Identifier || kind == CppTokenKind.Keyword;
        }

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

        /// <summary>
        /// Gets a boolean indicating whether the attribute is a dllexport or visibility("default")
        /// </summary>
        /// <param name="attribute">The attribute to check against</param>
        /// <returns><c>true</c> if the attribute is a dllexport or visibility("default")</returns>
        public static bool IsPublicExport(this CppAttribute attribute)
        {
            return attribute.Name == "dllexport" || (attribute.Name == "visibility" && attribute.Arguments == "\"default\"");
        }
        
        /// <summary>
        /// Gets a boolean indicating whether the function is a dllexport or visibility("default")
        /// </summary>
        /// <param name="cppClass">The class to check against</param>
        /// <returns><c>true</c> if the class is a dllexport or visibility("default")</returns>
        public static bool IsPublicExport(this CppClass cppClass)
        {
            if(cppClass.Attributes != null)
            {
                foreach (var attr in cppClass.Attributes)
                {
                    if (attr.IsPublicExport()) return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Gets a boolean indicating whether the function is a dllexport or visibility("default")
        /// </summary>
        /// <param name="function">The function to check against</param>
        /// <returns><c>true</c> if the function is a dllexport or visibility("default")</returns>
        public static bool IsPublicExport(this CppFunction function)
        {
            if (function.Attributes != null)
            {
                foreach (var attr in function.Attributes)
                {
                    if (attr.IsPublicExport()) return true;
                }
            }

            return function.LinkageKind == CppLinkageKind.External || function.LinkageKind == CppLinkageKind.UniqueExternal;
        }
    }
}
