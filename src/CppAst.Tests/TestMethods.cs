// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestMethods : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
class MyClass0
{
    public:
    void method0();

    private:
    static void method1();
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);

                    var cppClass = compilation.Classes[0];
                    Assert.AreEqual("MyClass0", cppClass.Name);

                    var methods = cppClass.Functions;
                    Assert.AreEqual(2, methods.Count);

                    Assert.AreEqual("public void method0()", methods[0].ToString());
                    Assert.AreEqual("private static void method1()", methods[1].ToString());
                }
            );
        }

        [Test]
        public void TestFinal()
        {
            ParseAssert(@"
class Base
{
public:
    virtual void Method();
};

class Leaf final : public Base
{
public:
    void Method() final;
    void OtherMethod();
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Classes.Count);

                    var baseClass = compilation.Classes[0];
                    Assert.False(baseClass.IsFinal);
                    Assert.AreEqual(1, baseClass.Functions.Count);
                    Assert.False(baseClass.Functions[0].IsFinal);

                    var leafClass = compilation.Classes[1];
                    Assert.True(leafClass.IsFinal);
                    Assert.AreEqual("class Leaf final : Base", leafClass.ToString());

                    Assert.AreEqual(2, leafClass.Functions.Count);
                    Assert.True(leafClass.Functions[0].IsFinal);
                    Assert.True(leafClass.Functions[0].Flags.HasFlag(CppFunctionFlags.Final));
                    Assert.AreEqual("public virtual void Method() final", leafClass.Functions[0].ToString());

                    Assert.False(leafClass.Functions[1].IsFinal);
                    Assert.AreEqual("public void OtherMethod()", leafClass.Functions[1].ToString());
                },
                new CppParserOptions { AdditionalArguments = { "-std=c++11" } }
            );
        }
    }
}
