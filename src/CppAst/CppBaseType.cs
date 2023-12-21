// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// A C++ base type used by <see cref="CppClass.BaseTypes"/>
    /// </summary>
    public sealed class CppBaseType : CppElement
    {
        /// <summary>
        /// Creates a base type.
        /// </summary>
        /// <param name="baseType">Type of the base</param>
        public CppBaseType(CppType baseType)
        {
            Type = baseType ?? throw new ArgumentNullException(nameof(baseType));
        }

        /// <summary>
        /// Gets or sets the visibility of this type.
        /// </summary>
        public CppVisibility Visibility { get; set; }

        /// <summary>
        /// Gets or sets if this element is virtual.
        /// </summary>
        public bool IsVirtual { get; set; }

        /// <summary>
        /// Gets the C++ type associated.
        /// </summary>
        public CppType Type { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Visibility != CppVisibility.Default && Visibility != CppVisibility.Public)
            {
                builder.Append(Visibility.ToString().ToLowerInvariant()).Append(" ");
            }

            if (IsVirtual)
            {
                builder.Append("virtual ");
            }

            builder.Append(Type.GetDisplayName());

            var cls = Type as CppClass;
            if(cls != null && cls.TemplateKind != CppTemplateKind.NormalClass)
            {
                builder.Append("<");

                if (cls.TemplateKind == CppTemplateKind.TemplateSpecializedClass)
                {
                    for (var i = 0; i < cls.TemplateSpecializedArguments.Count; i++)
                    {
                        if (i > 0) builder.Append(", ");
                        builder.Append(cls.TemplateSpecializedArguments[i].ToString());
                    }
                }
                else if (cls.TemplateKind == CppTemplateKind.TemplateClass)
                {
                    for (var i = 0; i < cls.TemplateParameters.Count; i++)
                    {
                        if (i > 0) builder.Append(", ");
                        builder.Append(cls.TemplateParameters[i].ToString());
                    }
                }

                builder.Append(">");
            }
            return builder.ToString();
        }
    }
}