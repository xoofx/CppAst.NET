// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace CppAst;

/// <summary>
/// A generic type, a type that has a base type and a list of generic type arguments.
/// </summary>
public class CppGenericType : CppType
{
    public CppGenericType(CppType baseType) : base(CppTypeKind.GenericType)
    {
        BaseType = baseType;
        GenericArguments = new List<CppType>();
    }
    
    public CppType BaseType { get; set; }
    
    public List<CppType> GenericArguments { get; }
    
    public override int SizeOf { get; set; }
    
    public override CppType GetCanonicalType() => this;


    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(BaseType.GetDisplayName());
        if (GenericArguments.Count > 0)
        {
            builder.Append('<');
            for (int i = 0; i < GenericArguments.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(GenericArguments[i].GetDisplayName());
            }
            builder.Append(">");
        }
        return builder.ToString();
    }
}