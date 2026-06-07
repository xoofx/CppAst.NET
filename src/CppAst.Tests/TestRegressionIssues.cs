// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestRegressionIssues : InlineTestBase
{
    [Test]
    public void TestIssue81InitListDefaultParameter()
    {
        ParseAssert(@"
template<typename T>
struct Vector
{
};

struct Pair
{
    int first;
    int second;
};

void take_vector(const Vector<int>& input = {});
void take_pair(Pair input = { 1, 2 });
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(2, compilation.Functions.Count);

                var vectorDefault = compilation.Functions[0].Parameters[0].InitExpression;
                Assert.IsNotNull(vectorDefault);
                Assert.AreEqual(CppExpressionKind.InitList, vectorDefault.Kind);
                Assert.AreEqual("{}", vectorDefault.ToString());

                var pairDefault = compilation.Functions[1].Parameters[0].InitExpression;
                Assert.IsNotNull(pairDefault);
                Assert.AreEqual(CppExpressionKind.InitList, pairDefault.Kind);
                Assert.AreEqual("{1, 2}", pairDefault.ToString());
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++11" } });
    }

    [Test]
    public void TestIssue90NestedTemplateSpecialization()
    {
        ParseAssert(@"
namespace chrono
{
    template<typename Rep, typename Period = int>
    struct duration
    {
    };
}

template<typename T>
struct abi
{
};

template<>
struct abi<chrono::duration<long long>>
{
    using type = long long;
};
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var abiSpecialization = compilation.Classes.Single(x =>
                    x.Name == "abi" && x.TemplateKind == CppTemplateKind.TemplateSpecializedClass);
                Assert.AreEqual(1, abiSpecialization.TemplateSpecializedArguments.Count);
                Assert.AreEqual(CppTemplateArgumentKind.AsType, abiSpecialization.TemplateSpecializedArguments[0].ArgKind);
                Assert.AreEqual("duration", ((CppClass)abiSpecialization.TemplateSpecializedArguments[0].ArgAsType).Name);
                Assert.AreEqual(1, abiSpecialization.Typedefs.Count);
                Assert.AreEqual("type", abiSpecialization.Typedefs[0].Name);
                Assert.AreEqual(CppPrimitiveType.LongLong, abiSpecialization.Typedefs[0].ElementType);
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++11" } });
    }

    [Test]
    public void TestIssue91NestedTemplateArgumentsUseOuterTemplateParameters()
    {
        ParseAssert(@"
namespace std
{
    template<typename _Ty>
    class shared_ptr
    {
    };

    template<typename Variant1, typename Variant2, typename Variant3>
    class variant
    {
    };
}

template<typename DataType, typename Variant1, typename Variant2, typename Variant3>
class Foo
{
public:
    Foo(const std::shared_ptr<DataType>& foo);
    Foo(const std::variant<Variant1, Variant2, Variant3>& bar);
private:
    std::shared_ptr<DataType> foo_;
    std::variant<Variant1, Variant2, Variant3> bar_;
};

using FooInt = Foo<int, double, char, long>;
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var foo = compilation.Classes.Single(x => x.Name == "Foo" && x.TemplateKind == CppTemplateKind.TemplateClass);
                Assert.AreEqual(2, foo.Fields.Count);

                var sharedPtr = (CppClass)foo.Fields[0].Type;
                Assert.AreEqual("shared_ptr", sharedPtr.Name);
                Assert.AreEqual(1, sharedPtr.TemplateSpecializedArguments.Count);
                Assert.AreEqual("DataType", sharedPtr.TemplateSpecializedArguments[0].ArgString);

                var variant = (CppClass)foo.Fields[1].Type;
                Assert.AreEqual("variant", variant.Name);
                Assert.AreEqual(3, variant.TemplateSpecializedArguments.Count);
                Assert.AreEqual(new[] { "Variant1", "Variant2", "Variant3" }, variant.TemplateSpecializedArguments.Select(x => x.ArgString).ToArray());

                var fooInt = compilation.Typedefs.Single(x => x.Name == "FooInt");
                var specializedFoo = (CppClass)fooInt.ElementType;
                Assert.AreEqual("Foo", specializedFoo.Name);
                Assert.AreEqual(new[] { "int", "double", "char", "long" }, specializedFoo.TemplateSpecializedArguments.Select(x => x.ArgString).ToArray());
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++11" } });
    }
}
