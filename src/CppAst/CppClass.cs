// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CppAst
{
    /// <summary>
    /// A C++ class, struct or union.
    /// </summary>
    public sealed class CppClass : CppTypeDeclaration, ICppMemberWithVisibility, ICppDeclarationContainer, ICppTemplateOwner
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
            TemplateParameters = new List<CppType>();
            Attributes = new List<CppAttribute>();
            TokenAttributes = new List<CppAttribute>();
        }

        /// <summary>
        /// Kind of the instance (`class` `struct` or `union`)
        /// </summary>
        public CppClassKind ClassKind { get; set; }

        public CppTemplateKind TemplateKind { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

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
                    for (var i = 0; i < TemplateArguments.Count; i++)
                    {
                        if (i != 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(TemplateArguments[i].ArgString);
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

        /// <inheritdoc />
        public CppContainerList<CppField> Fields { get; }

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

        /// <inheritdoc />
        public List<CppType> TemplateParameters { get; }

        public List<CppTemplateArgument> TemplateArguments { get; } = new List<CppTemplateArgument>();

        /// <summary>
        /// The primary, unspecialized class template of this instance.
        /// </summary>
        public CppClass PrimaryTemplate { get; set; }


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
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!string.IsNullOrEmpty(Name))
            {
                builder.Append(Name);
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

            //Add template arguments here
            if(TemplateKind != CppTemplateKind.NormalClass)
            {
                builder.Append("<");

                if(TemplateKind == CppTemplateKind.TemplateSpecializedClass)
                {
                    for (var i = 0; i < TemplateArguments.Count; i++)
                    {
                        if(i > 0) builder.Append(", ");
                        builder.Append(TemplateArguments[i].ToString());
                    }
                }
                else if(TemplateKind == CppTemplateKind.TemplateClass)
                {
                    for (var i = 0; i < TemplateParameters.Count; i++)
                    {
                        if (i > 0) builder.Append(", ");
                        builder.Append(TemplateParameters[i].ToString());
                    }
                }

                builder.Append(">");
            }

            builder.Append(" { ... }");
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

        public void UpdateTemplateArguments(CppType sourceParam, List<CppTemplateArgument> templateArguments)
        {
            // We need to remove all previously registered template arguments corresponding with 'sourceParam'
            // and update it with the new template arguments. The reason is that we "re-resolved" them 
            // in another layer (one layer above in the callstack) so we have a "better" resolution of them
            TemplateArguments.RemoveAll(arg => arg.SourceParam.Equals(sourceParam));
            TemplateArguments.AddRange(templateArguments);
        }
    }
}