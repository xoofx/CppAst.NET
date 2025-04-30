// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CppAst;

/// <summary>
/// A base Cpp container for macros, classes, fields, functions, enums, typesdefs.
/// </summary>
public class CppObjCCategory : CppElement, ICppDeclarationContainer
{
    /// <summary>
    /// Create a new instance of this container.
    /// </summary>
    public CppObjCCategory(CppClass targetClass, string categoryName)
    {
        TargetClass = targetClass;
        CategoryName = categoryName;
        Fields = new CppContainerList<CppField>(this);
        Functions = new CppContainerList<CppFunction>(this);
        Enums = new CppContainerList<CppEnum>(this);
        Classes = new CppContainerList<CppClass>(this);
        Typedefs = new CppContainerList<CppTypedef>(this);
        Attributes = new List<CppAttribute>();
        TokenAttributes = new List<CppAttribute>();
        Properties = new CppContainerList<CppProperty>(this);
    }
    
    /// <summary>
    /// Gets or sets the target class of this category.
    /// </summary>
    public CppClass TargetClass { get; set; }
    
    /// <summary>
    /// Gets or sets the name of this category.
    /// </summary>
    public string CategoryName { get; set; }

    /// <inheritdoc />
    public CppContainerList<CppField> Fields { get; }

    /// <inheritdoc />
    public CppContainerList<CppProperty> Properties { get; }

    /// <inheritdoc />
    public CppContainerList<CppFunction> Functions { get; }

    /// <inheritdoc />
    public CppContainerList<CppEnum> Enums { get; }

    /// <inheritdoc />
    public CppContainerList<CppClass> Classes { get; }

    /// <inheritdoc />
    public CppContainerList<CppTypedef> Typedefs { get; }

    /// <inheritdoc />
    public List<CppAttribute> Attributes { get; }

    [Obsolete("TokenAttributes is deprecated. please use system attributes and annotate attributes")]
    public List<CppAttribute> TokenAttributes { get; }

    public MetaAttributeMap MetaAttributes { get; private set; } = new MetaAttributeMap();

    /// <inheritdoc />
    public virtual IEnumerable<ICppDeclaration> Children()
    {
        return CppContainerHelper.Children(this);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("@interface ");
        builder.Append(TargetClass.Name);
        builder.Append(" (");
        builder.Append(CategoryName);
        builder.Append(")");
        
        return builder.ToString();
    }
}