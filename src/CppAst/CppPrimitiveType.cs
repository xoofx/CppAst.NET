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
        /// Singleton instance of the `long` type.
        /// </summary>
        public static readonly CppPrimitiveType Long = new CppPrimitiveType(CppPrimitiveKind.Long);

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
        /// Singleton instance of the `unsigned long` type.
        /// </summary>
        public static readonly CppPrimitiveType UnsignedLong = new CppPrimitiveType(CppPrimitiveKind.UnsignedLong);
        
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
        /// ObjC `id` type.
        /// </summary>
        public static readonly CppPrimitiveType ObjCId = new CppPrimitiveType(CppPrimitiveKind.ObjCId);

        /// <summary>
        /// ObjC `SEL` type.
        /// </summary>
        public static readonly CppPrimitiveType ObjCSel = new CppPrimitiveType(CppPrimitiveKind.ObjCSel);

        /// <summary>
        /// ObjC `Class` type.
        /// </summary>
        public static readonly CppPrimitiveType ObjCClass = new CppPrimitiveType(CppPrimitiveKind.ObjCClass);

        /// <summary>
        /// ObjC `Object` type.
        /// </summary>
        public static readonly CppPrimitiveType ObjCObject = new CppPrimitiveType(CppPrimitiveKind.ObjCObject);

        /// <summary>
        /// Unsigned 128 bits integer type.
        /// </summary>
        public static readonly CppPrimitiveType UInt128 = new CppPrimitiveType(CppPrimitiveKind.UInt128);
        
        /// <summary>
        /// 128 bits integer type.
        /// </summary>
        public static readonly CppPrimitiveType Int128 = new CppPrimitiveType(CppPrimitiveKind.Int128);
        
        /// <summary>
        /// Float16 type.
        /// </summary>
        public static readonly CppPrimitiveType Float16 = new CppPrimitiveType(CppPrimitiveKind.Float16);
        
        /// <summary>
        /// BFloat16 type.
        /// </summary>
        public static readonly CppPrimitiveType BFloat16 = new CppPrimitiveType(CppPrimitiveKind.BFloat16);
        

        
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
                case CppPrimitiveKind.Long:
                case CppPrimitiveKind.UnsignedLong:
                    sizeOf = 4; // This is incorrect
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
                case CppPrimitiveKind.ObjCId:
                case CppPrimitiveKind.ObjCSel:
                case CppPrimitiveKind.ObjCClass:
                case CppPrimitiveKind.ObjCObject:
                    sizeOf = 8; // Valid only for 64 bits
                    break;
                case CppPrimitiveKind.UInt128:
                case CppPrimitiveKind.Int128:
                    sizeOf = 16;
                    break;
                case CppPrimitiveKind.Float16:
                case CppPrimitiveKind.BFloat16:
                    sizeOf = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Kind switch
            {
                CppPrimitiveKind.Void => "void",
                CppPrimitiveKind.WChar => "wchar_t",
                CppPrimitiveKind.Char => "char",
                CppPrimitiveKind.Short => "short",
                CppPrimitiveKind.Int => "int",
                CppPrimitiveKind.Long => "long",
                CppPrimitiveKind.UnsignedLong => "unsigned long",
                CppPrimitiveKind.LongLong => "long long",
                CppPrimitiveKind.UnsignedChar => "unsigned char",
                CppPrimitiveKind.UnsignedShort => "unsigned short",
                CppPrimitiveKind.UnsignedInt => "unsigned int",
                CppPrimitiveKind.UnsignedLongLong => "unsigned long long",
                CppPrimitiveKind.Float => "float",
                CppPrimitiveKind.Double => "double",
                CppPrimitiveKind.LongDouble => "long double",
                CppPrimitiveKind.Bool => "bool",
                CppPrimitiveKind.Int128 => "System.Int128",
                CppPrimitiveKind.UInt128 => "System.UInt128",
                CppPrimitiveKind.ObjCId => "ObjCId",
                CppPrimitiveKind.ObjCSel => "ObjCSel",
                CppPrimitiveKind.ObjCClass => "ObjCClass",
                CppPrimitiveKind.ObjCObject => "ObjCObject",
                CppPrimitiveKind.Float16 => "System.Half",
                CppPrimitiveKind.BFloat16 => "BFloat16",
                _ => throw new InvalidOperationException($"Unhandled PrimitiveKind: {Kind}")
            };
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
        public override CppType GetCanonicalType()
        {
            return this;
        }
    }
}