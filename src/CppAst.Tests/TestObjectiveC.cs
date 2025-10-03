// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace CppAst.Tests;

public class TestObjectiveC : InlineTestBase
{
    [Test]
    public void TestAppKitIncludes()
    {
        if (!OperatingSystem.IsMacOS())
        {
            NUnit.Framework.Assert.Ignore("Only on MacOS");
            return;
        }
        
        ParseAssert("""
                    #import <AppKit/AppKit.h>
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(0, compilation.Diagnostics.Messages.Count, "Parsing foundation headers should not generate warnings"); // No warnings
                Assert.IsTrue(compilation.System.Classes.Count > 1000);
            }, new()
            {
                ParserKind = CppParserKind.ObjC,
                TargetCpu = CppTargetCpu.ARM64,
                TargetVendor = "apple",
                TargetSystem = "darwin",
                ParseMacros = false,
                ParseSystemIncludes = true,
                AdditionalArguments =
                {
                    "-std=gnu11",
                    "-isysroot", "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk",
                    "-F", "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/System/Library/Frameworks",
                    "-isystem", "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/usr/include",
                    "-isystem", "/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib/clang/16/include",
                }
            }
        );
    }

    [Test]
    public void TestInterfaceWithMethods()
    {
        ParseAssert("""
                    @interface MyInterface
                        - (float)helloworld;
                        - (void)doSomething:(int)index argSpecial:(float)arg1;
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Classes.Count);
                var myInterface = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(2, myInterface.Functions.Count);
                
                Assert.AreEqual(0, myInterface.Functions[0].Parameters.Count);
                Assert.AreEqual("helloworld", myInterface.Functions[0].Name);
                Assert.IsTrue(myInterface.Functions[0].ReturnType is CppPrimitiveType primitive && primitive.Kind == CppPrimitiveKind.Float);
                
                Assert.AreEqual(2, myInterface.Functions[1].Parameters.Count);
                Assert.AreEqual("index", myInterface.Functions[1].Parameters[0].Name);
                Assert.AreEqual("arg1", myInterface.Functions[1].Parameters[1].Name);
                Assert.AreEqual("doSomething:argSpecial:", myInterface.Functions[1].Name);
                Assert.IsTrue(myInterface.Functions[1].ReturnType is CppPrimitiveType primitive2 && primitive2.Kind == CppPrimitiveKind.Void);
                Assert.IsTrue(myInterface.Functions[1].Parameters[1].Type is CppPrimitiveType primitive3 && primitive3.Kind == CppPrimitiveKind.Float);

            }, GetDefaultObjCOptions()
        );

    }

    [Test]
    public void TestInterfaceWithProperties()
    {
        ParseAssert("""
                    @interface MyInterface
                        @property int id;
                        @property (readonly) float id2;
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Classes.Count);
                var myInterface = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(2, myInterface.Properties.Count);
                Assert.AreEqual("id", myInterface.Properties[0].Name);
                Assert.AreEqual("id2", myInterface.Properties[1].Name);

                Assert.IsTrue(myInterface.Properties[0].Type is CppPrimitiveType primitive && primitive.Kind == CppPrimitiveKind.Int);
                Assert.IsTrue(myInterface.Properties[0].Getter is not null);
                Assert.IsTrue(myInterface.Properties[0].Setter is not null);

                Assert.IsTrue(myInterface.Properties[1].Type is CppPrimitiveType primitive2 && primitive2.Kind == CppPrimitiveKind.Float);
                Assert.IsTrue(myInterface.Properties[1].Setter is null);

                Assert.AreEqual(3, myInterface.Functions.Count);
                Assert.AreEqual("id", myInterface.Functions[0].Name);
                Assert.IsTrue(myInterface.Functions[0].ReturnType is CppPrimitiveType primitive3 && primitive3.Kind == CppPrimitiveKind.Int);

                Assert.AreEqual("setId:", myInterface.Functions[1].Name);
                Assert.IsTrue(myInterface.Functions[1].ReturnType is CppPrimitiveType primitive4 && primitive4.Kind == CppPrimitiveKind.Void);
                Assert.AreEqual(1, myInterface.Functions[1].Parameters.Count);
                Assert.AreEqual("id", myInterface.Functions[1].Parameters[0].Name);
                Assert.IsTrue(myInterface.Functions[1].Parameters[0].Type is CppPrimitiveType primitive5 && primitive5.Kind == CppPrimitiveKind.Int);

                Assert.AreEqual("id2", myInterface.Functions[2].Name);
                Assert.IsTrue(myInterface.Functions[2].ReturnType is CppPrimitiveType primitive6 && primitive6.Kind == CppPrimitiveKind.Float);
                Assert.AreEqual(0, myInterface.Functions[2].Parameters.Count);
            }, GetDefaultObjCOptions()
        );
    }

    [Test]
    public void TestInterfaceWithInstanceType()
    {
        ParseAssert("""
                    @interface MyInterface
                        + (instancetype)getInstance;
                        - (instancetype)get;
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Classes.Count);
                var myInterface = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(2, myInterface.Functions.Count);
                
                Assert.AreEqual("getInstance", myInterface.Functions[0].Name);
                Assert.IsTrue((myInterface.Functions[0].Flags & CppFunctionFlags.ClassMethod) != 0);
                var typedef = myInterface.Functions[0].ReturnType as CppTypedef;
                Assert.IsTrue(typedef is not null && typedef.ElementType is CppPrimitiveType prim && prim.Kind == CppPrimitiveKind.ObjCObject, $"Invalid return type found for getInstance {myInterface.Functions[0].ReturnType}");

                Assert.AreEqual("get", myInterface.Functions[1].Name);
                Assert.IsTrue((myInterface.Functions[1].Flags & CppFunctionFlags.Method) != 0);
                Assert.IsTrue((myInterface.Functions[1].Flags & CppFunctionFlags.ClassMethod) == 0);
                var typedef2 = myInterface.Functions[1].ReturnType as CppTypedef;
                Assert.IsTrue(typedef2 is not null && typedef2.ElementType is CppPrimitiveType prim2 && prim2.Kind == CppPrimitiveKind.ObjCObject, $"Invalid return type found for getInstance {myInterface.Functions[1].ReturnType}");

            }, GetDefaultObjCOptions()
        );
    }

    [Test]
    public void TestInterfaceWithMultipleGenericParameters()
    {
        ParseAssert("""
                    @interface BaseInterface
                    @end
                    
                    // Generics require a base class
                    @interface MyInterface<T1, T2> : BaseInterface
                        - (T1)get_at:(int)index;
                        - (T2)get_at2:(int)index;
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(2, compilation.Classes.Count);
                var myInterface = compilation.Classes[1];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(2, myInterface.TemplateParameters.Count);
                Assert.IsTrue(myInterface.TemplateParameters[0] is CppTemplateParameterType templateParam1 && templateParam1.Name == "T1");
                Assert.IsTrue(myInterface.TemplateParameters[1] is CppTemplateParameterType templateParam2 && templateParam2.Name == "T2");

                Assert.AreEqual(2, myInterface.Functions.Count);
                Assert.AreEqual("get_at:", myInterface.Functions[0].Name);
                Assert.AreEqual("get_at2:", myInterface.Functions[1].Name);
                Assert.IsTrue(myInterface.Functions[0].ReturnType is CppTemplateParameterType templateSpecialization && templateSpecialization.Name == "T1");
                Assert.IsTrue(myInterface.Functions[1].ReturnType is CppTemplateParameterType templateSpecialization2 && templateSpecialization2.Name == "T2");
            }, GetDefaultObjCOptions()
        );
    }


    [Test]
    public void TestInterfaceWithGenericsAndTypedef()
    {
        ParseAssert("""
                    @interface BaseInterface
                    @end

                    // Generics require a base class
                    @interface MyInterface<T1> : BaseInterface
                        typedef T1 HelloWorld;
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(2, compilation.Classes.Count);
                var myInterface = compilation.Classes[1];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(1, myInterface.TemplateParameters.Count);
                Assert.IsTrue(myInterface.TemplateParameters[0] is CppTemplateParameterType templateParam1 && templateParam1.Name == "T1");

                var text = myInterface.ToString();
                Assert.AreEqual("@interface MyInterface<T1> : BaseInterface", text);
                
                // By default, typedef declared within interfaces are global, but in that case, it is depending on a template parameter
                // So it is not part of the global namespace
                Assert.AreEqual(0, compilation.Typedefs.Count);
                Assert.AreEqual(1, myInterface.Typedefs.Count);
                var typedef = myInterface.Typedefs[0];
                Assert.AreEqual("HelloWorld", typedef.Name);
                Assert.IsTrue(typedef.ElementType is CppTemplateParameterType templateSpecialization && templateSpecialization.Name == "T1");
            }, GetDefaultObjCOptions()
        );
    }

    [Test]
    public void TestBlockFunctionPointer()
    {
        ParseAssert("""
                    typedef float (^MyBlock)(int a, int* b);
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Typedefs.Count);

                var typedef = compilation.Typedefs[0];
                Assert.AreEqual("MyBlock", typedef.Name);

                Assert.IsInstanceOf<CppBlockFunctionType>(typedef.ElementType);
                var blockType =  (CppBlockFunctionType)typedef.ElementType;

                Assert.AreEqual(CppTypeKind.ObjCBlockFunction, blockType.TypeKind);

                Assert.IsTrue(blockType.ReturnType is CppPrimitiveType primitive && primitive.Kind == CppPrimitiveKind.Float);

                Assert.AreEqual(2, blockType.Parameters.Count);
                Assert.IsTrue(blockType.Parameters[0].Type is CppPrimitiveType primitive2 && primitive2.Kind == CppPrimitiveKind.Int);
                Assert.IsTrue(blockType.Parameters[1].Type is CppPointerType pointerType && pointerType.ElementType is CppPrimitiveType primitive3 && primitive3.Kind == CppPrimitiveKind.Int);
            }, GetDefaultObjCOptions()
        );
    }
    
    [Test]
    public void TestProtocol()
    {
        ParseAssert("""
                    @protocol MyProtocol
                    @end
                    
                    @protocol MyProtocol1
                    @end

                    @protocol MyProtocol2 <MyProtocol, MyProtocol1>
                    @end
                    
                    @interface MyInterface <MyProtocol>
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(4, compilation.Classes.Count);

                var myProtocol = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCProtocol, myProtocol.ClassKind);
                Assert.AreEqual("MyProtocol", myProtocol.Name);

                var myProtocol1 = compilation.Classes[1];
                Assert.AreEqual(CppClassKind.ObjCProtocol, myProtocol1.ClassKind);
                Assert.AreEqual("MyProtocol1", myProtocol1.Name);

                var myProtocol2 = compilation.Classes[2];
                Assert.AreEqual(CppClassKind.ObjCProtocol, myProtocol2.ClassKind);
                Assert.AreEqual("MyProtocol2", myProtocol2.Name);
                Assert.AreEqual(2, myProtocol2.ObjCImplementedProtocols.Count);
                Assert.AreEqual(myProtocol, myProtocol2.ObjCImplementedProtocols[0]);
                Assert.AreEqual(myProtocol1, myProtocol2.ObjCImplementedProtocols[1]);
                
                var text2 = myProtocol2.ToString();
                Assert.AreEqual("@protocol MyProtocol2 <MyProtocol, MyProtocol1>", text2);
                
                var myInterface = compilation.Classes[3];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(1, myInterface.ObjCImplementedProtocols.Count);
                Assert.AreEqual(myProtocol, myInterface.ObjCImplementedProtocols[0]);

            }, GetDefaultObjCOptions()
        );
    }

    [Test]
    public void TestInterfaceBaseType()
    {
        ParseAssert("""
                    @interface InterfaceBase
                    @end
                    
                    @interface MyInterface : InterfaceBase
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(2, compilation.Classes.Count);

                var myInterfaceBase = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterfaceBase.ClassKind);
                Assert.AreEqual(0, myInterfaceBase.BaseTypes.Count);
                Assert.AreEqual("InterfaceBase", myInterfaceBase.Name);

                var myInterface = compilation.Classes[1];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);
                Assert.AreEqual(1, myInterface.BaseTypes.Count);
                Assert.AreEqual(myInterfaceBase, myInterface.BaseTypes[0].Type);
            }, GetDefaultObjCOptions()
        );
    }
    
    [Test]
    public void TestInterfaceWithCategory()
    {
        ParseAssert("""
                    @interface MyInterface
                    @end

                    @interface MyInterface (MyCategory)
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(2, compilation.Classes.Count);

                var myInterface = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface.ClassKind);
                Assert.AreEqual("MyInterface", myInterface.Name);

                var myInterfaceWithCategory = compilation.Classes[1];
                Assert.AreEqual(CppClassKind.ObjCInterfaceCategory, myInterfaceWithCategory.ClassKind);
                Assert.AreEqual("MyInterface", myInterfaceWithCategory.Name);
                Assert.AreEqual("MyCategory", myInterfaceWithCategory.ObjCCategoryName);
                Assert.AreEqual(myInterface, myInterfaceWithCategory.ObjCCategoryTargetClass);

                var text = myInterfaceWithCategory.ToString();
                Assert.AreEqual("@interface MyInterface (MyCategory)", text);
            }, GetDefaultObjCOptions()
        );
    }

    [Test]
    public void TestInterfaceWithMethodsReturningObjects()
    {
        ParseAssert("""
                    @interface MyInterface1
                    + (MyInterface1*)getInstance1;
                    @end
                    
                    @interface MyInterface2<T1> : MyInterface1
                    @end
                    
                    @interface MyInterface2NoGeneric
                    @end
                    
                    @protocol MyProtocol
                    @end
                    
                    @interface MyInterface3
                    + (MyInterface2<MyInterface1*>*)getInstance2;
                    + (MyInterface2NoGeneric<MyProtocol>*)getInstanceWithProtocol3;
                    + (id<MyProtocol>)getInstanceWithProtocol4;
                    @end
                    """,
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(5, compilation.Classes.Count);

                var myInterface1 = compilation.Classes[0];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface1.ClassKind);
                Assert.AreEqual("MyInterface1", myInterface1.Name);

                var myInterface2 = compilation.Classes[1];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface2.ClassKind);
                Assert.AreEqual("MyInterface2", myInterface2.Name);

                var myInterface2NoGeneric = compilation.Classes[2];
                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface2NoGeneric.ClassKind);
                Assert.AreEqual("MyInterface2NoGeneric", myInterface2NoGeneric.Name);

                var myProtocol = compilation.Classes[3];
                Assert.AreEqual(CppClassKind.ObjCProtocol, myProtocol.ClassKind);
                Assert.AreEqual("MyProtocol", myProtocol.Name);
                
                var myInterface3 = compilation.Classes[4];

                Assert.AreEqual(CppClassKind.ObjCInterface, myInterface3.ClassKind);
                Assert.AreEqual("MyInterface3", myInterface3.Name);
                Assert.AreEqual(3, myInterface3.Functions.Count);
                Assert.AreEqual("getInstance2", myInterface3.Functions[0].Name);
                Assert.AreEqual("getInstanceWithProtocol3", myInterface3.Functions[1].Name);
                Assert.AreEqual("getInstanceWithProtocol4", myInterface3.Functions[2].Name);

                Assert.IsTrue(myInterface3.Functions[0].ReturnType is CppPointerType pointerType &&
                              pointerType.ElementType is CppObjCGenericType genericType &&
                              ReferenceEquals(genericType.BaseType, myInterface2) &&
                              genericType.GenericArguments.Count == 1 &&
                              genericType.GenericArguments[0] is CppPointerType pointerType2 &&
                              ReferenceEquals(pointerType2.ElementType, myInterface1),
                    "Unable to resolve return type getInstance2. Must be MyInterface2<MyInterface1*>*"
                );

                Assert.IsTrue(myInterface3.Functions[1].ReturnType is CppPointerType pointerType3 &&
                              pointerType3.ElementType is CppObjCGenericType genericType2 &&
                              ReferenceEquals(genericType2.BaseType, myInterface2NoGeneric) &&
                              genericType2.ObjCProtocolRefs.Count == 1 &&
                              ReferenceEquals(genericType2.ObjCProtocolRefs[0], myProtocol),
                    "Unable to resolve return type getInstanceWithProtocol3. Must be MyInterface2NoGeneric<MyProtocol>*"
                );

                Assert.IsTrue(myInterface3.Functions[2].ReturnType is CppPointerType pointerType4 &&
                              pointerType4.ElementType is CppObjCGenericType genericType3 &&
                              genericType3.BaseType is CppPrimitiveType primitiveType3 &&
                              primitiveType3.Kind == CppPrimitiveKind.ObjCObject &&
                              genericType3.ObjCProtocolRefs.Count == 1 &&
                              ReferenceEquals(genericType3.ObjCProtocolRefs[0], myProtocol),
                    "Unable to resolve return type getInstanceWithProtocol4. Must be id<MyProtocol>"
                );

            }, GetDefaultObjCOptions()
        );
    }


    private static CppParserOptions GetDefaultObjCOptions()
    {
        return new CppParserOptions
        {
            ParserKind = CppParserKind.ObjC,
            TargetCpu = CppTargetCpu.ARM64,
            TargetVendor = "apple",
            TargetSystem = "darwin",
            ParseMacros = false,
            ParseSystemIncludes = false,
        };
    }
}