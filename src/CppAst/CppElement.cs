﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

namespace CppAst
{
    /// <summary>
    /// Base class for all Cpp elements of the AST nodes.
    /// </summary>
    public abstract class CppElement : ICppElement
    {
        /// <summary>
        /// Gets or sets the source span of this element.
        /// </summary>
        public CppSourceSpan Span;

        /// <summary>
        /// Gets or sets the parent container of this element. Might be null.
        /// </summary>
        public ICppContainer Parent { get; internal set; }

        public sealed override bool Equals(object obj) => ReferenceEquals(this, obj);

        public sealed override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

        public string FullParentName
        {
            get
            {
                string tmpname = "";
                var p = Parent;
                while (p != null)
                {
                    if (p is CppClass)
                    {
                        var cpp = p as CppClass;
                        tmpname = $"{cpp.Name}::{tmpname}";
                        p = cpp.Parent;
                    }
                    else if (p is CppNamespace)
                    {
                        var ns = p as CppNamespace;

                        //Just ignore inline namespace
                        if (!ns.IsInlineNamespace)
                        {
                            tmpname = $"{ns.Name}::{tmpname}";
                        }
                        p = ns.Parent;
                    }
                    else if (p is CppCompilation)
                    {
                        // root namespace here, just ignore~
                        p = null;
                    }
                    else if (p is CppGlobalDeclarationContainer)
                    {
                       // root container here, just ignore~
                       p = null;
                    }
                    else
                    {
                        throw new NotImplementedException("Can not be here, not support type here!");
                    }
                }

                //Try to remove not need `::` in string tails.
                if (tmpname.EndsWith("::"))
                {
                    tmpname = tmpname.Substring(0, tmpname.Length - 2);
                }

                return tmpname;
            }
        }

        /// <summary>
        /// Gets the source file of this element.
        /// </summary>
        public string SourceFile => string.IsNullOrWhiteSpace(Span.Start.File) ? (Parent as CppElement)?.SourceFile : Span.Start.File;

    }
}