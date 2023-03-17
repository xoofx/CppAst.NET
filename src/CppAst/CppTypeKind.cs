// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Kinds of a C++ type (e.g primitive, pointer...)
    /// </summary>
    public enum CppTypeKind
    {
        /// <summary>
        /// A primitive type (e.g `int`, `short`, `double`...)
        /// </summary>
        Primitive,
        /// <summary>
        /// A Pointer type (e.g `int*`)
        /// </summary>
        Pointer,
        /// <summary>
        /// A reference type (e.g `int&amp;`)
        /// </summary>
        Reference,
        /// <summary>
        /// An array type (e.g int[5])
        /// </summary>
        Array,
        /// <summary>
        /// A qualified type (e.g const int)
        /// </summary>
        Qualified,
        /// <summary>
        /// A function type
        /// </summary>
        Function,
        /// <summary>
        /// A typedef
        /// </summary>
        Typedef,
        /// <summary>
        /// A struct or a class.
        /// </summary>
        StructOrClass,
        /// <summary>
        /// An standard or scoped enum
        /// </summary>
        Enum,
        /// <summary>
        /// A template parameter type.
        /// </summary>
        TemplateParameterType,
		/// <summary>
		/// A none type template parameter type.
		/// </summary>
		TemplateParameterNonType,
		/// <summary>
		/// An unexposed type.
		/// </summary>
		Unexposed,
    }
}