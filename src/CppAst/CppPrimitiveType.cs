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

        private readonly int _sizeOf;

        /// <summary>
        /// Base constructor of a primitive
        /// </summary>
        /// <param name="kind"></param>
        private CppPrimitiveType(CppPrimitiveKind kind) : base(CppTypeKind.Primitive)
        {
            Kind = kind;
            UpdateSize(out _sizeOf);
        }

        /// <summary>
        /// The kind of primitive.
        /// </summary>
        public CppPrimitiveKind Kind { get; }

        private void UpdateSize(out int sizeOf)
        {
            switch (Kind)
            {
                case CppPrimitiveKind.Void:
                    sizeOf = 0;
                    break;
                case CppPrimitiveKind.Bool:
                    sizeOf = 1;
                    break;
                case CppPrimitiveKind.WChar:
                    sizeOf = 2;
                    break;
                case CppPrimitiveKind.Char:
                    sizeOf = 1;
                    break;
                case CppPrimitiveKind.Short:
                    sizeOf = 2;
                    break;
                case CppPrimitiveKind.Int:
                    sizeOf = 4;
                    break;
                case CppPrimitiveKind.LongLong:
                    sizeOf = 8;
                    break;
                case CppPrimitiveKind.UnsignedChar:
                    sizeOf = 1;
                    break;
                case CppPrimitiveKind.UnsignedShort:
                    sizeOf = 2;
                    break;
                case CppPrimitiveKind.UnsignedInt:
                    sizeOf = 4;
                    break;
                case CppPrimitiveKind.UnsignedLongLong:
                    sizeOf = 8;
                    break;
                case CppPrimitiveKind.Float:
                    sizeOf = 4;
                    break;
                case CppPrimitiveKind.Double:
                    sizeOf = 8;
                    break;
                case CppPrimitiveKind.LongDouble:
                    sizeOf = 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
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
                case CppPrimitiveKind.Bool:
                    return "bool";
                default:
                    throw new InvalidOperationException($"Unhandled PrimitiveKind: {Kind}");
            }
        }

        private bool Equals(CppPrimitiveType other)
        {
            return base.Equals(other) && Kind == other.Kind;
        }

        /// <inheritdoc />
        public override int SizeOf
        {
            get => _sizeOf;
            set => throw new InvalidOperationException("Cannot set the SizeOf of a primitive type");
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppPrimitiveType other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int)Kind;
            }
        }

        /// <inheritdoc />
        public override CppType GetCanonicalType()
        {
            return this;
        }
    }
}