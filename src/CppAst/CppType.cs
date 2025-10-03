// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Base class for C++ types.
    /// </summary>
    public abstract class CppType : CppElement
    {
        /// <summary>
        /// Constructor with the specified type kind.
        /// </summary>
        /// <param name="typeKind"></param>
        protected CppType(CppTypeKind typeKind)
        {
            TypeKind = typeKind;
        }

        /// <summary>
        /// Gets the <see cref="CppTypeKind"/> of this instance.
        /// </summary>
        public CppTypeKind TypeKind { get; }

        public abstract int SizeOf { get; set; }

        /// <summary>
        /// Gets the canonical type of this type instance.
        /// </summary>
        /// <returns>A canonical type of this type instance</returns>
        public abstract CppType GetCanonicalType();

        /// <summary>
        /// Gets a boolean indicating if this type is a pointer to an Objective-C type.
        /// </summary>
        public bool IsPointerToObjCType =>
            this is CppPointerType pointerType && pointerType.ElementType.IsObjCType;

        /// <summary>
        /// Gets a boolean indicating if this type is an Objective-C type.
        /// </summary>
        public bool IsObjCType =>
            (
                (this is CppClass cppClass && (cppClass.ClassKind == CppClassKind.ObjCInterface || cppClass.ClassKind == CppClassKind.ObjCInterfaceCategory || cppClass.ClassKind == CppClassKind.ObjCProtocol)) ||
                (this is CppPrimitiveType primitive && (primitive.Kind == CppPrimitiveKind.ObjCObject || primitive.Kind == CppPrimitiveKind.ObjCClass)) ||
                (this is CppObjCGenericType genericType) ||
                (this is CppTemplateParameterType templateParameterType && templateParameterType.Kind == CppTemplateParameterTypeKind.ObjC)
            );

        public static bool IsOpaqueStruct(CppType type)
        {
            var cppElement = type;

            while (cppElement is CppTypedef cppTypedef)
            {
                cppElement = cppTypedef.ElementType;
            }

            return cppElement is CppClass cppClass && !cppClass.IsDefinition;
        }
        
        /// <summary>
        /// We can use this name in exporter to use this type.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return ToString();
            }
        }
    }
}