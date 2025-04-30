// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst;

/// <summary>
/// An Objective-C proeprty.
/// </summary>
public sealed class CppProperty : CppDeclaration, ICppMember, ICppAttributeContainer
{
    public CppProperty(CppType type, string name)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = name;
        Attributes = new List<CppAttribute>();
    }

    /// <summary>
    /// Gets attached attributes. Might be null.
    /// </summary>
    public List<CppAttribute> Attributes { get; }

    [Obsolete("TokenAttributes is deprecated. please use system attributes and annotate attributes")]
    public List<CppAttribute> TokenAttributes { get; } = new();

    public MetaAttributeMap MetaAttributes { get; private set; } = new MetaAttributeMap();

    /// <summary>
    /// Gets the type of this field/variable.
    /// </summary>
    public CppType Type { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the getter method.
    /// </summary>
    internal string GetterName { get; set; }
    
    /// <summary>
    /// Gets or sets the getter method.
    /// </summary>
    public CppFunction Getter { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the setter method.
    /// </summary>
    internal string SetterName { get; set; }
    
    /// <summary>
    /// Gets or sets the setter method.
    /// </summary>
    public CppFunction Setter { get; set; }
    
    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append(Type.GetDisplayName());
        builder.Append(" ");
        builder.Append(Name);
        return builder.ToString();
    }
}