// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Linq;
using NUnit.Framework;
using static CppAst.CppTemplateArgument;

namespace CppAst.Tests
{
    public class TestTypes : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
typedef int& t0; // reference type
typedef const float t1;
char* f0; // pointer type
const int f1 = 5; // qualified type
int f2[5]; // array type
void (*f3)(int arg1, float arg2); // function type
t1* f4;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(5, compilation.Fields.Count);
                    Assert.AreEqual(2, compilation.Typedefs.Count);

                    var types = new CppType[]
                    {
                        new CppReferenceType(CppPrimitiveType.Int) ,
                        new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Float),

                        new CppPointerType(CppPrimitiveType.Char),
                        new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Int),
                        new CppArrayType(CppPrimitiveType.Int, 5),
                        new CppPointerType(new CppFunctionType(CppPrimitiveType.Void)
                        {
                            Parameters =
                            {
                                new CppParameter(CppPrimitiveType.Int, "a"),
                                new CppParameter(CppPrimitiveType.Float, "b"),
                            }
                        }) { SizeOf = IntPtr.Size },
                        new CppPointerType(new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Float))
                    };

                    var canonicalTypes = compilation.Typedefs.Select(x => x.GetCanonicalType()).Concat(compilation.Fields.Select(x => x.Type.GetCanonicalType())).ToList();
                    Assert.AreEqual(types.Select(x => x.SizeOf), canonicalTypes.Select(x => x.SizeOf));
                }
            );
        }

        [Test]
        public void TestTemplateParameters()
        {
            ParseAssert(@"
template <typename T, typename U>
struct TemplateStruct
{
    T field0;
    U field1;
};

struct Struct2
{
};

::TemplateStruct<int, Struct2> exposed;
TemplateStruct<int, Struct2> unexposed;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Fields.Count);

                    var exposed = compilation.Fields[0].Type as CppClass;
                    Assert.AreEqual("TemplateStruct", exposed.Name);
                    Assert.AreEqual(2, exposed.TemplateParameters.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, exposed.TemplateSpecializedArguments[0]?.ArgKind);
                    Assert.AreEqual(CppPrimitiveKind.Int, (exposed.TemplateSpecializedArguments[0]?.ArgAsType as CppPrimitiveType).Kind);
                    Assert.AreEqual("Struct2", (exposed.TemplateSpecializedArguments[1].ArgAsType as CppClass)?.Name);

                    var specialized = exposed.SpecializedTemplate;
                    Assert.AreEqual("TemplateStruct", specialized.Name);
                    Assert.AreEqual(2, specialized.Fields.Count);
                    Assert.AreEqual("field0", specialized.Fields[0].Name);
                    Assert.AreEqual("T", specialized.Fields[0].Type.GetDisplayName());
                    Assert.AreEqual("field1", specialized.Fields[1].Name);
                    Assert.AreEqual("U", specialized.Fields[1].Type.GetDisplayName());

                    var unexposed = compilation.Fields[1].Type as CppClass;
                    Assert.AreEqual("TemplateStruct", unexposed.Name);
                    Assert.AreEqual(2, unexposed.TemplateParameters.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, unexposed.TemplateSpecializedArguments[0]?.ArgKind);
                    Assert.AreEqual(CppPrimitiveKind.Int, (exposed.TemplateSpecializedArguments[0]?.ArgAsType as CppPrimitiveType).Kind);
                    Assert.AreEqual("Struct2", (unexposed.TemplateSpecializedArguments[1].ArgAsType as CppClass)?.Name);

                    Assert.AreNotEqual(exposed.GetHashCode(), specialized.GetHashCode());
                    Assert.AreEqual(exposed.GetHashCode(), unexposed.GetHashCode());
                }
            );
        }

        [Test]
        public void TestTemplateInheritance()
        {
            ParseAssert(@"
template <typename T>
class BaseTemplate
{
};

class Derived : public ::BaseTemplate<::Derived>
{
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Classes.Count);

                    var baseTemplate = compilation.Classes[0];
                    var derived = compilation.Classes[1];
                    var baseClassSpecialized = compilation.Classes[2];

                    Assert.AreEqual("BaseTemplate", baseTemplate.Name);
                    Assert.AreEqual("Derived", derived.Name);
                    Assert.AreEqual("BaseTemplate", baseClassSpecialized.Name);

                    Assert.AreEqual(1, derived.BaseTypes.Count);
                    Assert.AreEqual(baseClassSpecialized, derived.BaseTypes[0].Type);

                    Assert.AreEqual(1, baseClassSpecialized.TemplateParameters.Count);

                    //Here change to argument as a template deduce instance, not as a Template Parameters~~
                    Assert.AreEqual(derived, baseClassSpecialized.TemplateSpecializedArguments[0].ArgAsType);
                    Assert.AreEqual(baseTemplate, baseClassSpecialized.SpecializedTemplate);
                }
            );
        }


        [Test]
        public void TestTemplatePartialSpecialization()
        {
            ParseAssert(@"
template<typename A, typename B>
struct foo {};

template<typename B>
struct foo<int, B> {};

foo<int, int> foobar;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Classes.Count);
                    Assert.AreEqual(1, compilation.Fields.Count);

                    var baseTemplate = compilation.Classes[0];
                    var fullSpecializedClass = compilation.Classes[1];
                    var partialSpecializedTemplate = compilation.Classes[2];

                    var field = compilation.Fields[0];
                    Assert.AreEqual(field.Name, "foobar");

                    Assert.AreEqual(baseTemplate.TemplateKind, CppAst.CppTemplateKind.TemplateClass);
                    Assert.AreEqual(fullSpecializedClass.TemplateKind, CppAst.CppTemplateKind.TemplateSpecializedClass);
                    Assert.AreEqual(partialSpecializedTemplate.TemplateKind, CppAst.CppTemplateKind.PartialTemplateClass);

                    //Need be a specialized for partial template here
                    Assert.AreEqual(fullSpecializedClass.SpecializedTemplate, partialSpecializedTemplate);

                    //Need be a full specialized class for this field
                    Assert.AreEqual(field.Type, fullSpecializedClass);

                    Assert.AreEqual(partialSpecializedTemplate.TemplateSpecializedArguments.Count, 2);
                    //The first argument is integer now
                    Assert.AreEqual(partialSpecializedTemplate.TemplateSpecializedArguments[0].ArgString, "int");
                    //The second argument is not a specialized argument, we do not specialized a `B` template parameter here(partial specialized template)
                    Assert.AreEqual(partialSpecializedTemplate.TemplateSpecializedArguments[1].IsSpecializedArgument, false);

                    //The field use type is a full specialized type here~, so we can have two `int` template parmerater here
                    //It's a not template or partial template class, so we can instantiate it, see `foo<int, int> foobar;` before.
                    Assert.AreEqual(fullSpecializedClass.TemplateSpecializedArguments.Count, 2);
                    //The first argument is integer now
                    Assert.AreEqual(fullSpecializedClass.TemplateSpecializedArguments[0].ArgString, "int");
                    //The second argument is not a specialized argument
                    Assert.AreEqual(fullSpecializedClass.TemplateSpecializedArguments[1].ArgString, "int");
                }
            );
        }

        [Test]
        public void TestClassPrototype()
        {
            ParseAssert(@"
namespace ns1 {
class TmpClass;
}

namespace ns2 {
const ns1::TmpClass* tmpClass1;
volatile ns1::TmpClass* tmpClass2;
const unsigned int * const dummy_pu32 = (const unsigned int * const)0x12345678;
}

namespace ns1 {
class TmpClass {
};
}
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var tmpClass1 = compilation.Namespaces[1].Fields[0];
                    var tmpClass2 = compilation.Namespaces[1].Fields[1];
                    var constDummyPointer = compilation.Namespaces[1].Fields[2];

                    var hoge = tmpClass1.Type.GetDisplayName();
                    var hoge2 = tmpClass2.Type.GetDisplayName();
                    var hoge3 = tmpClass2.Type.GetDisplayName();
                    Assert.AreEqual("TmpClass const *", tmpClass1.Type.GetDisplayName());
                    Assert.AreEqual("TmpClass volatile *", tmpClass2.Type.GetDisplayName());
                    Assert.AreEqual("unsigned int const * const", constDummyPointer.Type.GetDisplayName());
                }
            );
        }
    }
}