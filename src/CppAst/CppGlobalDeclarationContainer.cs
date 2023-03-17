// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CppAst
{
    /// <summary>
    /// A base Cpp container for macros, classes, fields, functions, enums, typesdefs.
    /// </summary>
    public class CppGlobalDeclarationContainer : CppElement, ICppGlobalDeclarationContainer
    {
        private readonly Dictionary<ICppContainer, Dictionary<string, CacheByName>> _multiCacheByName;

        /// <summary>
        /// Create a new instance of this container.
        /// </summary>
        public CppGlobalDeclarationContainer()
        {
            _multiCacheByName = new Dictionary<ICppContainer, Dictionary<string, CacheByName>>(ReferenceEqualityComparer<ICppContainer>.Default);
            Macros = new List<CppMacro>();
            Fields = new CppContainerList<CppField>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            Namespaces = new CppContainerList<CppNamespace>(this);
            Attributes = new CppContainerList<CppAttribute>(this);
        }

        /// <summary>
        /// Gets the macros defines for this container.
        /// </summary>
        /// <remarks>
        /// Macros are only available if <see cref="CppParserOptions.ParseMacros"/> is <c>true</c>
        /// </remarks>
        public List<CppMacro> Macros { get; }

        /// <inheritdoc />
        public CppContainerList<CppField> Fields { get; }

        /// <inheritdoc />
        public CppContainerList<CppFunction> Functions { get; }

        /// <inheritdoc />
        public CppContainerList<CppEnum> Enums { get; }

        /// <inheritdoc />
        public CppContainerList<CppClass> Classes { get; }

        /// <inheritdoc />
        public CppContainerList<CppTypedef> Typedefs { get; }

        /// <inheritdoc />
        public CppContainerList<CppNamespace> Namespaces { get; }

        /// <inheritdoc />
        public CppContainerList<CppAttribute> Attributes { get; }

        /// <inheritdoc />
        public virtual IEnumerable<ICppDeclaration> Children()
        {
            return CppContainerHelper.Children(this);
        }

        /// <summary>
        /// Find a <see cref="CppElement"/> by name declared directly by this container.
        /// </summary>
        /// <param name="name">Name of the element to find</param>
        /// <returns>The CppElement found or null if not found</returns>
        public CppElement FindByName(string name)
        {
            return FindByName(this, name);
        }

        private CppElement SearchForChild(CppElement parent, string child_name)
        {
            ICppDeclarationContainer container = null;
            if(parent is CppNamespace)
            {
                var ns = parent as CppNamespace;
                var n = ns.Namespaces.FirstOrDefault(x => x.Name == child_name);
                if (n != null) return n;

                container = ns;
            }
            else if(parent is CppClass)
            {
                container = parent as ICppDeclarationContainer;
            }

            if(container != null)
            {
                var c = container.Classes.FirstOrDefault(x => x.Name == child_name);
                if (c != null) return c;

                var e = container.Enums.FirstOrDefault(x => x.Name == child_name);
                if (e != null) return e;

                var f = container.Functions.FirstOrDefault(x => x.Name == child_name);
                if (f != null) return f;

                var t = container.Typedefs.FirstOrDefault(x => x.Name == child_name);
                if (t != null) return t;
            }

            return null;
        }

        /// <summary>
        /// Find a <see cref="CppElement"/> by full name(such as gbf::math::Vector3).
        /// </summary>
        /// <param name="name">Name of the element to find</param>
        /// <returns>The CppElement found or null if not found</returns>
        public CppElement FindByFullName(string name)
        {
            var arr = name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
            if(arr.Length == 0) return null;

            CppElement elem = null;
            for(int i = 0; i < arr.Length; i++)
            {
                if (i == 0)
                {
                   elem = FindByName(arr[0]);
                }
                else
                {
                   elem = SearchForChild(elem, arr[i]);
                }

                if (elem == null) return null;
            }
            return elem;
        }

        /// <summary>
        /// Find a <see cref="CppElement"/> by full name(such as gbf::math::Vector3).
        /// </summary>
        /// <param name="name">Name of the element to find</param>
        /// <returns>The CppElement found or null if not found</returns>
        public TCppElement FindByFullName<TCppElement>(string name) where TCppElement : CppElement
        {
            return (TCppElement)FindByFullName(name);
        }

        /// <summary>
        /// Find a <see cref="CppElement"/> by name declared within the specified container.
        /// </summary>
        /// <param name="container">The container to search for the element by name</param>
        /// <param name="name">Name of the element to find</param>
        /// <returns>The CppElement found or null if not found</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public CppElement FindByName(ICppContainer container, string name)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var cacheByName = FindByNameInternal(container, name);
            return cacheByName.Element;
        }

        /// <summary>
        /// Find a list of <see cref="CppElement"/> matching name (overloads) declared within the specified container.
        /// </summary>
        /// <param name="container">The container to search for the element by name</param>
        /// <param name="name">Name of the element to find</param>
        /// <returns>A list of CppElement found or empty enumeration if not found</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<CppElement> FindListByName(ICppContainer container, string name)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var cacheByName = FindByNameInternal(container, name);
            return cacheByName;
        }

        /// <summary>
        /// Find a <see cref="CppElement"/> by name and type declared directly by this container.
        /// </summary>
        /// <param name="name">Name of the element to find</param>
        /// <returns>The CppElement found or null if not found</returns>
        public TCppElement FindByName<TCppElement>(string name) where TCppElement : CppElement
        {
            return FindByName<TCppElement>(this, name);
        }

        /// <summary>
        /// Find a <see cref="CppElement"/> by name and type declared within the specified container.
        /// </summary>
        /// <param name="container">The container to search for the element by name</param>
        /// <param name="name">Name of the element to find</param>
        /// <returns>The CppElement found or null if not found</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public TCppElement FindByName<TCppElement>(ICppContainer container, string name) where TCppElement : CppElement
        {
            return (TCppElement)FindByName(container, name);
        }

        /// <summary>
        /// Clear the cache used by all <see cref="FindByName(string)"/> functions.
        /// </summary>
        /// <remarks>
        /// Used this method when new elements are added to this instance.
        /// </remarks>
        public void ClearCacheByName()
        {
            // TODO: reuse previous internal cache
            _multiCacheByName.Clear();
        }

        private CacheByName FindByNameInternal(ICppContainer container, string name)
        {
            if (!_multiCacheByName.TryGetValue(container, out var cacheByNames))
            {
                cacheByNames = new Dictionary<string, CacheByName>();
                _multiCacheByName.Add(container, cacheByNames);

                foreach (var element in container.Children())
                {
                    var cppElement = (CppElement)element;
                    if (element is ICppMember member && !string.IsNullOrEmpty(member.Name))
                    {
                        var elementName = member.Name;
                        if (!cacheByNames.TryGetValue(elementName, out var cacheByName))
                        {
                            cacheByName = new CacheByName();
                        }

                        if (cacheByName.Element == null)
                        {
                            cacheByName.Element = cppElement;
                        }
                        else
                        {
                            if (cacheByName.List == null)
                            {
                                cacheByName.List = new List<CppElement>();
                            }
                            cacheByName.List.Add(cppElement);
                        }

                        cacheByNames[elementName] = cacheByName;
                    }
                }

            }

            return cacheByNames.TryGetValue(name, out var cacheByNameFound) ? cacheByNameFound : new CacheByName();
        }

        private struct CacheByName : IEnumerable<CppElement>
        {
            public CppElement Element;

            public List<CppElement> List;
            public IEnumerator<CppElement> GetEnumerator()
            {
                if (Element != null) yield return Element;
                if (List != null)
                {
                    foreach (var cppElement in List)
                    {
                        yield return cppElement;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        public static readonly ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}