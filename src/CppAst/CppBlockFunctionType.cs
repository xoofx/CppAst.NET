// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// An Objective-C block function type (e.g `void (^)(int arg1, int arg2)`)
    /// </summary>
    public sealed class CppBlockFunctionType : CppFunctionTypeBase
    {
        /// <summary>
        /// Constructor of a function type.
        /// </summary>
        /// <param name="returnType">Return type of this function type.</param>
        public CppBlockFunctionType(CppType returnType) : base(CppTypeKind.ObjCBlockFunction, returnType)
        {
        }
    }
}