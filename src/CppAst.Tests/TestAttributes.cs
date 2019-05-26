// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestAttributes : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
__declspec(dllimport) int i;
__declspec(dllexport) void func0();
extern ""C"" void __stdcall func1(int a, int b, int c);
void *fun2(int align) __attribute__((alloc_align(1)));
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Fields.Count);
                    Assert.NotNull(compilation.Fields[0].Attributes);
                    Assert.AreEqual("dllimport", compilation.Fields[0].Attributes[0].ToString());

                    Assert.AreEqual(3, compilation.Functions.Count);
                    Assert.NotNull(compilation.Functions[0].Attributes);
                    Assert.AreEqual(1, compilation.Functions[0].Attributes.Count);
                    Assert.AreEqual("dllexport", compilation.Functions[0].Attributes[0].ToString());

                    Assert.AreEqual(CppCallingConvention.X86StdCall, compilation.Functions[1].CallingConvention);

                    Assert.NotNull(compilation.Functions[2].Attributes);
                    Assert.AreEqual(1, compilation.Functions[2].Attributes.Count);
                    Assert.AreEqual("alloc_align(1)", compilation.Functions[2].Attributes[0].ToString());

                },
                new CppParserOptions()
                {
                    IsWindows = true,
                    AdditionalArguments =
                    {
                        "--target=i686-pc-win32"
                    }
                }
            );
        }
    }
}