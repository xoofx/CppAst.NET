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

class Baz : public Bar
{
public:
  using Bar::Bar;
  Baz(double y) : y_{y} {}
private:
  double y_{0};
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(3, compilation.Classes.Count);
                    Assert.AreEqual(1, compilation.Classes[0].Constructors.Count);
                    Assert.AreEqual(2, compilation.Classes[1].Constructors.Count);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[1].Constructors[0].Visibility);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[1].Constructors[1].Visibility);

                    Assert.AreEqual(3, compilation.Classes[2].Constructors.Count);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[2].Constructors[0].Visibility);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[2].Constructors[1].Visibility);
                    Assert.AreEqual(CppVisibility.Public, compilation.Classes[2].Constructors[2].Visibility);

                }
            );
        }

        [Test]
        public void TestAuto()
        {
            ParseAssert(@"

class Foo
{
public:
  Foo(int foo) : foo_{foo} {}
  const auto & foo() const { return foo_; }
private:
  int foo_{0};
};

class Bar
{
public:
  auto Get42() const { return Foo(42); }
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(2, compilation.Classes.Count);
                    Assert.AreEqual(1, compilation.Classes[0].Functions.Count);
                    Assert.AreEqual(CppTypeKind.Reference, compilation.Classes[0].Functions[0].ReturnType.TypeKind);
                    Assert.AreEqual("const int&", (compilation.Classes[0].Functions[0].ReturnType as CppReferenceType).GetCanonicalType().ToString());
                    Assert.AreEqual(1, compilation.Classes[1].Functions.Count);
                    Assert.AreEqual(CppTypeKind.StructOrClass, compilation.Classes[1].Functions[0].ReturnType.TypeKind);
                    Assert.AreEqual("Foo", (compilation.Classes[1].Functions[0].ReturnType as CppClass).Name);
                }
            );
        }
    }
}