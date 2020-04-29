// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ default value used to initialize <see cref="CppParameter"/>
    /// </summary>
    public class CppValue : CppElement
    {
        /// <summary>
        /// A default C++ value.
        /// </summary>
        /// <param name="value"></param>
        public CppValue(object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }
}