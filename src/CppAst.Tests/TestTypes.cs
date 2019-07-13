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
    }
}