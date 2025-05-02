// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ class, struct or union.
    /// </summary>
    public class CppClass : CppTypeDeclaration, ICppMemberWithVisibility, ICppDeclarationContainer, ICppTemplateOwner
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">Name of this type.</param>
        public CppClass(string name) : base(CppTypeKind.StructOrClass)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseTypes = new List<CppBaseType>();
            Fields = new CppContainerList<CppField>(this);
            Constructors = new CppContainerList<CppFunction>(this);
            Destructors = new CppContainerList<CppFunction>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            TemplateParameters = new CppContainerList<CppType>(this);
            Attributes = new List<CppAttribute>();
            TokenAttributes = new List<CppAttribute>();
            ObjCImplementedProtocols = new List<CppClass>();
            Properties = new CppContainerList<CppProperty>(this);
            ObjCCategories = new List<CppClass>();
            ObjCCategoryName = string.Empty;
        }

        /// <summary>
        /// Kind of the instance (`class` `struct` or `union`)
        /// </summary>
        public CppClassKind ClassKind { get; set; }

        public CppTemplateKind TemplateKind { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the target of the Objective-C category. Null if this class is not an <see cref="CppClassKind.ObjCInterfaceCategory"/>.
        /// </summary>
        public CppClass ObjCCategoryTargetClass { get; set; }

        /// <summary>
        /// Gets or sets the name of the Objective-C category. Empty if this class is not an <see cref="CppClassKind.ObjCInterfaceCategory"/>
        /// </summary>
        public string ObjCCategoryName { get; set; }

        public override string FullName
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                string fullparent = FullParentName;
                if (string.IsNullOrEmpty(fullparent))
                {
                    sb.Append(Name);
                }
                else
                {
                    sb.Append($"{fullparent}::{Name}");
                }

                if (TemplateKind == CppTemplateKind.TemplateClass
                    || TemplateKind == CppTemplateKind.PartialTemplateClass)
                {
                    sb.Append('<');
                    for (int i = 0; i < TemplateParameters.Count; i++)
                    {
                        var tp = TemplateParameters[i];
                        if (i != 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(tp.ToString());
                    }
                    sb.Append('>');
                }
                else if (TemplateKind == CppTemplateKind.TemplateSpecializedClass)
                {
                    sb.Append('<');
                    for (int i = 0; i < TemplateSpecializedArguments.Count; i++)
                    {
                        var ta = TemplateSpecializedArguments[i];
                        if (i != 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(ta.ArgString);
                    }
                    sb.Append('>');
                }
                //else if(TemplateKind == CppTemplateKind.PartialTemplateClass)
                //{
                //    sb.Append('<');
                //    sb.Append('>');
                //}
                return sb.ToString();
            }
        }

        /// <inheritdoc />
        public CppVisibility Visibility { get; set; }

        /// <inheritdoc />
        public List<CppAttribute> Attributes { get; }

        [Obsolete("TokenAttributes is deprecated. please use system attributes and annotate attributes")]
        public List<CppAttribute> TokenAttributes { get; }

        public MetaAttributeMap MetaAttributes { get; private set; } = new MetaAttributeMap();

        /// <summary>
        /// Gets or sets a boolean indicating if this type is a definition. If <c>false</c> the type was only declared but is not defined.
        /// </summary>
        public bool IsDefinition { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this declaration is anonymous.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Get the base types of this type.
        /// </summary>
        public List<CppBaseType> BaseTypes { get; }
        
        /// <summary>
        /// Get the Objective-C implemented protocols.
        /// </summary>
        public List<CppClass> ObjCImplementedProtocols { get; }
        
        /// <inheritdoc />
        public CppContainerList<CppField> Fields { get; }

        /// <inheritdoc />
        public CppContainerList<CppProperty> Properties { get; }

        /// <summary>
        /// Gets the constructors of this instance.
        /// </summary>
        public CppContainerList<CppFunction> Constructors { get; set; }

        /// <summary>
        /// Gets the destructors of this instance.
        /// </summary>
        public CppContainerList<CppFunction> Destructors { get; set; }

        /// <inheritdoc />
        public CppContainerList<CppFunction> Functions { get; }

        /// <inheritdoc />
        public CppContainerList<CppEnum> Enums { get; }

        /// <inheritdoc />
        public CppContainerList<CppClass> Classes { get; }

        /// <inheritdoc />
        public CppContainerList<CppTypedef> Typedefs { get; }
        
        /// <summary>
        /// Gets the Objective-C categories of this instance.
        /// </summary>
        public List<CppClass> ObjCCategories { get; }

        /// <inheritdoc />
        public CppContainerList<CppType> TemplateParameters { get; }

        public List<CppTemplateArgument> TemplateSpecializedArguments { get; } = new List<CppTemplateArgument>();

        /// <summary>
        /// Gets the specialized class template of this instance.
        /// </summary>
        public CppClass SpecializedTemplate { get; set; }


        public bool IsEmbeded => Parent is CppClass;

        public bool IsAbstract { get; set; }


        /// <inheritdoc />
        public override int SizeOf { get; set; }

        /// <summary>
        /// Gets the alignment of this instance.
        /// </summary>
        public int AlignOf { get; set; }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            switch (ClassKind)
            {
                case CppClassKind.Class:
                    builder.Append("class ");
                    break;
                case CppClassKind.Struct:
                    builder.Append("struct ");
                    break;
                case CppClassKind.Union:
                    builder.Append("union ");
                    break;
                case CppClassKind.ObjCInterface:
                case CppClassKind.ObjCInterfaceCategory:
                    builder.Append("@interface ");
                    break;
                case CppClassKind.ObjCProtocol:
                    builder.Append("@protocol ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(Name))
            {
                builder.Append(Name);
            }
            
            //Add template arguments here
            if(TemplateKind != CppTemplateKind.NormalClass)
            {
                builder.Append("<");

                if(TemplateKind == CppTemplateKind.TemplateSpecializedClass)
                {
                    for(var i = 0; i < TemplateSpecializedArguments.Count; i++)
                    {
                        if(i > 0) builder.Append(", ");
                        builder.Append(TemplateSpecializedArguments[i].ToString());
                    }
                }
                else if (TemplateParameters.Count > 0)
                {
                    for (var i = 0; i < TemplateParameters.Count; i++)
                    {
                        if (i > 0) builder.Append(", ");
                        builder.Append(TemplateParameters[i].ToString());
                    }
                }

                builder.Append(">");
            }

            if (BaseTypes.Count > 0)
            {
                builder.Append(" : ");
                for (var i = 0; i < BaseTypes.Count; i++)
                {
                    var baseType = BaseTypes[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(baseType);
                }
            }
            
            if (!string.IsNullOrEmpty(ObjCCategoryName))
            {
                builder.Append(" (").Append(ObjCCategoryName).Append(')');
            }

            if (ObjCImplementedProtocols.Count > 0)
            {
                builder.Append(" <");
                for (var i = 0; i < ObjCImplementedProtocols.Count; i++)
                {
                    var protocol = ObjCImplementedProtocols[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(protocol.Name);
                }
                builder.Append(">");
            }
            
            return builder.ToString();
        }

        public override IEnumerable<ICppDeclaration> Children()
        {
            foreach (var item in CppContainerHelper.Children(this))
            {
                yield return item;
            }

            foreach (var item in Constructors)
            {
                yield return item;
            } 

            foreach (var item in Destructors)
            {
                yield return item;
            }
        }
    }
}