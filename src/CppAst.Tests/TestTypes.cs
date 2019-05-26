// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

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
char* f0; // pointer type
const int f2 = 5; // qualified type
int f3[5]; // array type
void (*f4)(int arg1, float arg2); // function type
typedef int& f1; // reference type
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(4, compilation.Fields.Count);
                    Assert.AreEqual(1, compilation.Typedefs.Count);

                    var types = new CppType[]
                    {
                        new CppPointerType(CppPrimitiveType.Char),
                        new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Int),
                        new CppArrayType(CppPrimitiveType.Int, 5),
                        new CppPointerType(new CppFunctionType(CppPrimitiveType.Void)
                        {
                            ParameterTypes =
                            {
                                CppPrimitiveType.Int,
                                CppPrimitiveType.Float,
                            }
                        }),
                        new CppReferenceType(CppPrimitiveType.Int),
                    };

                    var parsedTypes = compilation.Fields.Select(x => x.Type).Concat(compilation.Typedefs.Select(x => x.Type)).ToList();

                    Assert.AreEqual(types, parsedTypes);
                }
            );
        }
    }
}