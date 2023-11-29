using System;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestMisc : InlineTestBase
    {
        [Test]
        public void TestMiscFeatures()
        {
            ParseAssert(@"

class Foo
{
public:
  Foo(int x) : x_{x} {}
private:
  int x_{0};
};

class Bar : public Foo
{
public:
  using Foo::Foo;
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(2, compilation.Classes.Count);
                    Assert.AreEqual(1, compilation.Classes[0].Constructors.Count);
                    // Bar will get 3 constructors
                    Assert.AreEqual(3, compilation.Classes[1].Constructors.Count);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[1].Constructors[0].Visibility);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[1].Constructors[1].Visibility);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[1].Constructors[2].Visibility);
                }
            );
        }
    }
}