using System;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestMisc : InlineTestBase
    {
        [Test]
        public void TestUsing()
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

        [Test]
        public void TestTemplate()
        {
            var options = new CppParserOptions();
            options.AdditionalArguments.Add("-std=c++17");
            ParseAssert(@"

#include <memory>
#include <variant>

template<typename DataType, typename Variant1, typename Variant2, typename Variant3>
class Foo
{
public:
  Foo(const std::shared_ptr<DataType> & foo) : foo_{foo} {}
  Foo(const std::variant<Variant1, Variant2, Variant3> & bar) : bar_{bar} {}
private:
  std::shared_ptr<DataType> foo_;
  std::variant<Variant1, Variant2, Variant3> bar_;
};

using FooInt = Foo<int, double, char, long>;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(2, compilation.Classes.Count);
                    Assert.AreEqual(2, compilation.Classes[0].Constructors.Count);
                    Assert.AreEqual(1, compilation.Classes[0].Constructors[0].Parameters.Count);
                    Assert.AreEqual("foo", compilation.Classes[0].Constructors[0].Parameters[0].Name );
                    Assert.AreEqual(CppTypeKind.Reference, compilation.Classes[0].Constructors[0].Parameters[0].Type.TypeKind);
                    Assert.AreEqual(CppTypeKind.Qualified, (compilation.Classes[0].Constructors[0].Parameters[0].Type as CppReferenceType).ElementType.TypeKind);
                    Assert.AreEqual(CppTypeKind.StructOrClass, ((compilation.Classes[0].Constructors[0].Parameters[0].Type as CppReferenceType).ElementType as CppQualifiedType).ElementType.TypeKind);
                    var clazz = (((compilation.Classes[0].Constructors[0].Parameters[0].Type as CppReferenceType).ElementType as CppQualifiedType).ElementType as CppClass);
                    Assert.AreEqual(CppTemplateKind.TemplateClass, clazz.TemplateKind);
                    Assert.AreEqual(1, clazz.TemplateArguments.Count);
                    Assert.AreEqual(1, clazz.TemplateParameters.Count);
                    // Don't check the actual shared_ptr<T>'s name, it differs on Windows and on Linux, just make sure the same being mapped
                    Assert.AreEqual(clazz.TemplateArguments[0].SourceParam.FullName, clazz.TemplateParameters[0].FullName);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, clazz.TemplateArguments[0].ArgKind);
                    Assert.AreEqual("DataType", clazz.TemplateArguments[0].ArgAsType.FullName);

                    Assert.AreEqual(4, compilation.Classes[1].TemplateArguments.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, compilation.Classes[1].TemplateArguments[0].ArgKind);
                    Assert.AreEqual("DataType", compilation.Classes[1].TemplateArguments[0].SourceParam.FullName);
                    Assert.AreEqual("int", compilation.Classes[1].TemplateArguments[0].ArgAsType.FullName);
                    Assert.AreEqual("Variant1", compilation.Classes[1].TemplateArguments[1].SourceParam.FullName);
                    Assert.AreEqual("double", compilation.Classes[1].TemplateArguments[1].ArgAsType.FullName);
                    Assert.AreEqual("Variant2", compilation.Classes[1].TemplateArguments[2].SourceParam.FullName);
                    Assert.AreEqual("char", compilation.Classes[1].TemplateArguments[2].ArgAsType.FullName);
                    Assert.AreEqual("Variant3", compilation.Classes[1].TemplateArguments[3].SourceParam.FullName);
                    Assert.AreEqual("int", compilation.Classes[1].TemplateArguments[3].ArgAsType.FullName);
                    Assert.AreEqual(4, compilation.Classes[1].TemplateParameters.Count);
                    Assert.AreEqual("DataType", compilation.Classes[1].TemplateParameters[0].FullName);
                    Assert.AreEqual("Variant1", compilation.Classes[1].TemplateParameters[1].FullName);
                    Assert.AreEqual("Variant2", compilation.Classes[1].TemplateParameters[2].FullName);
                    Assert.AreEqual("Variant3", compilation.Classes[1].TemplateParameters[3].FullName);
                }, options
            );
        }

        [Test]
        public void TestStdVariadicTemplate()
        {
            var options = new CppParserOptions();
            options.AdditionalArguments.Add("-std=c++17");
            ParseAssert(@"
#include <variant>

void function1(std::variant<float, short, std::variant<double, char>> input);
void function2(std::tuple<std::variant<float, short>, char, int, std::variant<char, double, long>> input);
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(2, compilation.Functions.Count);
                var function = compilation.Functions[0];
                Assert.AreEqual(1, function.Parameters.Count);
                var parameter = function.Parameters[0];
                Assert.True(parameter.Type is CppClass);
                Assert.AreEqual(parameter.Name, "input");
                var parameterClass = parameter.Type as CppClass;
                Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, parameterClass.TemplateKind);
                Assert.AreEqual("variant", parameterClass.Name);
                Assert.AreEqual(1, parameterClass.TemplateParameters.Count);
                Assert.AreEqual(3, parameterClass.TemplateArguments.Count);
                Assert.AreEqual("float", parameterClass.TemplateArguments[0].ArgAsType.FullName);
                Assert.AreEqual("short", parameterClass.TemplateArguments[1].ArgAsType.FullName);
                var paramVariant = parameterClass.TemplateArguments[2].ArgAsType as CppClass;
                Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, paramVariant.TemplateKind);
                Assert.AreEqual("variant", paramVariant.Name);
                Assert.AreEqual(1, paramVariant.TemplateParameters.Count);
                Assert.AreEqual(2, paramVariant.TemplateArguments.Count);
                Assert.AreEqual("double", paramVariant.TemplateArguments[0].ArgAsType.FullName);
                Assert.AreEqual("char", paramVariant.TemplateArguments[1].ArgAsType.FullName);

                function = compilation.Functions[1];
                Assert.AreEqual(1, function.Parameters.Count);
                parameter = function.Parameters[0];
                Assert.True(parameter.Type is CppClass);
                Assert.AreEqual("input", parameter.Name);
                parameterClass = parameter.Type as CppClass;
                Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, parameterClass.TemplateKind);
                Assert.AreEqual("tuple", parameterClass.Name);
                Assert.AreEqual(4, parameterClass.TemplateArguments.Count);
                paramVariant = parameterClass.TemplateArguments[0].ArgAsType as CppClass;
                Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, paramVariant.TemplateKind);
                Assert.AreEqual("variant", paramVariant.Name);
                Assert.AreEqual(1, paramVariant.TemplateParameters.Count);
                Assert.AreEqual(2, paramVariant.TemplateArguments.Count);
                Assert.AreEqual("float", paramVariant.TemplateArguments[0].ArgAsType.FullName);
                Assert.AreEqual("short", paramVariant.TemplateArguments[1].ArgAsType.FullName);
                Assert.AreEqual("char", parameterClass.TemplateArguments[1].ArgAsType.FullName);
                Assert.AreEqual("int", parameterClass.TemplateArguments[2].ArgAsType.FullName);
                paramVariant = parameterClass.TemplateArguments[3].ArgAsType as CppClass;
                Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, paramVariant.TemplateKind);
                Assert.AreEqual("variant", paramVariant.Name);
                Assert.AreEqual(1, paramVariant.TemplateParameters.Count);
                Assert.AreEqual(3, paramVariant.TemplateArguments.Count);
                Assert.AreEqual("char", paramVariant.TemplateArguments[0].ArgAsType.FullName);
                Assert.AreEqual("double", paramVariant.TemplateArguments[1].ArgAsType.FullName);
                Assert.AreEqual("int", paramVariant.TemplateArguments[2].ArgAsType.FullName);
            }, options);
        }

        [Test]
        public void TestUnderlyingType()
        {
            ParseAssert(@"
#include <type_traits>

enum class OneTwoThree : int16_t
{
  One = 1,
  Two,
  Three
};
typedef std::underlying_type<OneTwoThree>::type OneTwoThreeType;

void function1(const OneTwoThreeType& testEnumType);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(1, compilation.Functions.Count);
                    var function = compilation.Functions[0];
                    Assert.AreEqual(1, function.Parameters.Count);
                    Assert.True(function.Parameters[0].Type is CppReferenceType);
                    var referenceType = function.Parameters[0].Type as CppReferenceType;
                    Assert.True(referenceType.ElementType is CppQualifiedType);
                    var qualifiedType = referenceType.ElementType as CppQualifiedType;
                    Assert.True(qualifiedType.ElementType is CppTypedef);
                    var typedefType = qualifiedType.ElementType as CppTypedef;
                    Assert.True(typedefType.ElementType is CppPrimitiveType);
                    var primitiveType = typedefType.ElementType as CppPrimitiveType;
                    Assert.AreEqual("short", primitiveType.FullName);
                }
            );
        }

    }
}