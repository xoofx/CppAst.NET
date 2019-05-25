// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst
{
    /// <summary>
    /// A C++ primitive type (e.g `int`, `void`, `bool`...)
    /// </summary>
    public sealed class CppPrimitiveType : CppType
    { 
        /// <summary>
        /// Singleton instance of the `void` type.
        /// </summary>
        public static readonly CppPrimitiveType Void = new CppPrimitiveType(CppPrimitiveKind.Void);

        /// <summary>
        /// Singleton instance of the `bool` type.
        /// </summary>
        public static readonly CppPrimitiveType Bool = new CppPrimitiveType(CppPrimitiveKind.Bool);

        /// <summary>
        /// Singleton instance of the `wchar` type.
        /// </summary>
        public static readonly CppPrimitiveType WChar = new CppPrimitiveType(CppPrimitiveKind.WChar);

        /// <summary>
        /// Singleton instance of the `char` type.
        /// </summary>
        public static readonly CppPrimitiveType Char = new CppPrimitiveType(CppPrimitiveKind.Char);

        /// <summary>
        /// Singleton instance of the `short` type.
        /// </summary>
        public static readonly CppPrimitiveType Short = new CppPrimitiveType(CppPrimitiveKind.Short);

        /// <summary>
        /// Singleton instance of the `int` type.
        /// </summary>
        public static readonly CppPrimitiveType Int = new CppPrimitiveType(CppPrimitiveKind.Int);

        /// <summary>
        /// Singleton instance of the `long long` type.
        /// </summary>
        public static readonly CppPrimitiveType LongLong = new CppPrimitiveType(CppPrimitiveKind.LongLong);

        /// <summary>
        /// Singleton instance of the `unsigned char` type.
        /// </summary>
        public static readonly CppPrimitiveType UnsignedChar = new CppPrimitiveType(CppPrimitiveKind.UnsignedChar);

        /// <summary>
        /// Singleton instance of the `unsigned short` type.
        /// </summary>
        public static readonly CppPrimitiveType UnsignedShort = new CppPrimitiveType(CppPrimitiveKind.UnsignedShort);

        /// <summary>
        /// Singleton instance of the `unsigned int` type.
        /// </summary>
        public static readonly CppPrimitiveType UnsignedInt = new CppPrimitiveType(CppPrimitiveKind.UnsignedInt);

        /// <summary>
        /// Singleton instance of the `unsigned long long` type.
        /// </summary>
        public static readonly CppPrimitiveType UnsignedLongLong = new CppPrimitiveType(CppPrimitiveKind.UnsignedLongLong);

        /// <summary>
        /// Singleton instance of the `float` type.
        /// </summary>
        public static readonly CppPrimitiveType Float = new CppPrimitiveType(CppPrimitiveKind.Float);

        /// <summary>
        /// Singleton instance of the `float` type.
        /// </summary>
        public static readonly CppPrimitiveType Double = new CppPrimitiveType(CppPrimitiveKind.Double);

        /// <summary>
        /// Singleton instance of the `long double` type.
        /// </summary>
        public static readonly CppPrimitiveType LongDouble = new CppPrimitiveType(CppPrimitiveKind.LongDouble);

        /// <summary>
        /// Base constructor of a primitive
        /// </summary>
        /// <param name="kind"></param>
        private CppPrimitiveType(CppPrimitiveKind kind) : base(CppTypeKind.Primitive)
        {
            Kind = kind;
        }

        /// <summary>
        /// The kind of primitive.
        /// </summary>
        public CppPrimitiveKind Kind { get; }

        public override string ToString()
        {
            switch (Kind)
            {
                case CppPrimitiveKind.Void:
                    return "void";
                case CppPrimitiveKind.WChar:
                    return "wchar";
                case CppPrimitiveKind.Char:
                    return "char";
                case CppPrimitiveKind.Short:
                    return "short";
                case CppPrimitiveKind.Int:
                    return "int";
                case CppPrimitiveKind.LongLong:
                    return "long long";
                case CppPrimitiveKind.UnsignedChar:
                    return "unsigned char";
                case CppPrimitiveKind.UnsignedShort:
                    return "unsigned short";
                case CppPrimitiveKind.UnsignedInt:
                    return "unsigned int";
                case CppPrimitiveKind.UnsignedLongLong:
                    return "unsigned long long";
                case CppPrimitiveKind.Float:
                    return "float";
                case CppPrimitiveKind.Double:
                    return "double";
                case CppPrimitiveKind.LongDouble:
                    return "long double";
                default:
                    throw new InvalidOperationException($"Unhandled PrimitiveKind: {Kind}");
            }
        }

        private bool Equals(CppPrimitiveType other)
        {
            return base.Equals(other) && Kind == other.Kind;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppPrimitiveType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) Kind;
            }
        }
    }
}