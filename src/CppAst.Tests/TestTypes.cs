// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Linq;
using NUnit.Framework;

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
                    Assert.AreEqual(types, canonicalTypes);
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
                    Assert.AreEqual(CppPrimitiveKind.Int, (exposed.TemplateParameters[0] as CppPrimitiveType).Kind);
                    Assert.AreEqual("Struct2", (exposed.TemplateParameters[1] as CppClass).Name);

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
                    Assert.AreEqual(CppPrimitiveKind.Int, (unexposed.TemplateParameters[0] as CppPrimitiveType).Kind);
                    Assert.AreEqual("Struct2", (unexposed.TemplateParameters[1] as CppClass).Name);

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
                    Assert.AreEqual(derived, baseClassSpecialized.TemplateParameters[0]);
                    Assert.AreEqual(baseTemplate, baseClassSpecialized.SpecializedTemplate);
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

                    var hoge = tmpClass1.Type.GetDisplayName();
                    var hoge2 = tmpClass2.Type.GetDisplayName();
                    Assert.AreEqual("const TmpClass*", tmpClass1.Type.GetDisplayName());
                    Assert.AreEqual("volatile TmpClass*", tmpClass2.Type.GetDisplayName());
                }
            );
        }
    }
}