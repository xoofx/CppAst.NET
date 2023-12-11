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
        public void TestPartialSpecialization()
        {
            ParseAssert(@"

template <typename T, int N>
class ArrayBase
{
public:
  ArrayBase(T array[N]) { for(int i=0;i<N;i++) array_[i] = array_[i]; }
private:
  T array_[N];
};

template <typename T>
class Array2d : public ArrayBase<T, 2>
{
public:
  using ArrayBase<T,2>::ArrayBase;
};

using Array2i = Array2d<int>;

#include <array>

template <typename T, int N>
class VectorBase : public virtual std::array<T, N>
{
};

template <typename T>
class Vector3d : public VectorBase<T, 3>
{
public:
  Vector3d(const std::array<T, 3> & elements = {0, 0, 0})
      : Vector3d<T>(elements[0], elements[1], elements[2]) {}

  Vector3d(T x, T y, T z)
  {
    (*this)[0] = x, (*this)[1] = y; (*this)[2] = z;
  }
};

using Vect3i = Vector3d<int32_t>;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(6, compilation.Classes.Count);

                    /** Tests whether we are a generic template class with only non-specialized parameters (T,N) */
                    Assert.AreEqual(CppTemplateKind.TemplateClass, compilation.Classes[0].TemplateKind);
                    Assert.AreEqual(2, compilation.Classes[0].TemplateParameters.Count);
                    Assert.AreEqual(0, compilation.Classes[0].TemplateSpecializedArguments.Count);
                    Assert.AreEqual("T", compilation.Classes[0].TemplateParameters[0].FullName);
                    Assert.AreEqual("int N", compilation.Classes[0].TemplateParameters[1].FullName);

                    /** Tests whether we are a partially specialized template class with one non-specialized (T) and one specialized parameter (2) */
                    Assert.AreEqual(CppTemplateKind.PartialTemplateClass, compilation.Classes[1].TemplateKind);
                    Assert.AreEqual(1, compilation.Classes[1].TemplateParameters.Count);
                    Assert.AreEqual("T", compilation.Classes[1].TemplateParameters[0].FullName);
                    Assert.AreEqual(1, compilation.Classes[1].TemplateSpecializedArguments.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsInteger, compilation.Classes[1].TemplateSpecializedArguments[0].ArgKind);
                    Assert.AreEqual(2, compilation.Classes[1].TemplateSpecializedArguments[0].ArgAsInteger);

                    /** Tests whether we are a fully specialized template class with two specialized parameters (int, 2) */
                    Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, compilation.Classes[2].TemplateKind);
                    Assert.AreEqual(2, compilation.Classes[2].TemplateSpecializedArguments.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, compilation.Classes[2].TemplateSpecializedArguments[0].ArgKind);
                    Assert.AreEqual("int", compilation.Classes[2].TemplateSpecializedArguments[0].ArgAsType.FullName);
                    Assert.AreEqual(CppTemplateArgumentKind.AsInteger, compilation.Classes[2].TemplateSpecializedArguments[1].ArgKind);
                    Assert.AreEqual(2, compilation.Classes[2].TemplateSpecializedArguments[1].ArgAsInteger);

                    /** Tests whether we are a generic template class with only non-specialized parameters (T, N) */
                    Assert.AreEqual(CppTemplateKind.TemplateClass, compilation.Classes[3].TemplateKind);
                    Assert.AreEqual(2, compilation.Classes[3].TemplateParameters.Count);
                    Assert.AreEqual(0, compilation.Classes[3].TemplateSpecializedArguments.Count);
                    Assert.AreEqual("T", compilation.Classes[3].TemplateParameters[0].FullName);
                    Assert.AreEqual("int N", compilation.Classes[3].TemplateParameters[1].FullName);

                    /** Tests whether we are a partially specialized template class with one non-specialized (T) and one specialized parameter (3) */
                    Assert.AreEqual(CppTemplateKind.PartialTemplateClass, compilation.Classes[4].TemplateKind);
                    Assert.AreEqual(1, compilation.Classes[4].TemplateParameters.Count);
                    Assert.AreEqual("T", compilation.Classes[4].TemplateParameters[0].FullName);
                    Assert.AreEqual(1, compilation.Classes[4].TemplateSpecializedArguments.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsInteger, compilation.Classes[4].TemplateSpecializedArguments[0].ArgKind);
                    Assert.AreEqual(3, compilation.Classes[4].TemplateSpecializedArguments[0].ArgAsInteger);

                    /** Tests whether we are a fully specialized template class with two specialized parameters (int,3) */
                    Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, compilation.Classes[5].TemplateKind);
                    Assert.AreEqual(2, compilation.Classes[5].TemplateSpecializedArguments.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, compilation.Classes[5].TemplateSpecializedArguments[0].ArgKind);
                    Assert.AreEqual("int", compilation.Classes[5].TemplateSpecializedArguments[0].ArgAsType.FullName);
                    Assert.AreEqual(CppTemplateArgumentKind.AsInteger, compilation.Classes[5].TemplateSpecializedArguments[1].ArgKind);
                    Assert.AreEqual(3, compilation.Classes[5].TemplateSpecializedArguments[1].ArgAsInteger);

                }
            );
        }
    }
}