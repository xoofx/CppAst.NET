// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestEnumsAdvanced : InlineTestBase
{
    [Test]
    public void TestEnumExpressionsLargeValuesScopedReferencesAndAnonymousTypedefs()
    {
        ParseAssert(@"
enum Plain
{
    PlainNegative = -2,
    PlainImplicit,
    PlainShift = 1 << 4,
    PlainOr = PlainShift | 2,
};

enum class Scoped : unsigned long long
{
    None = 0,
    Big = 1ull << 40,
};

typedef enum
{
    AnonymousA = 7,
    AnonymousB,
} AnonymousEnum;

const int plainValue = PlainOr;
Scoped scopedValue = Scoped::Big;
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var plain = compilation.Enums.Single(x => x.Name == "Plain");
                Assert.False(plain.IsScoped);
                Assert.AreEqual(new[] { "PlainNegative", "PlainImplicit", "PlainShift", "PlainOr" }, plain.Items.Select(x => x.Name).ToArray());
                Assert.AreEqual(new long[] { -2, -1, 16, 18 }, plain.Items.Select(x => x.Value).ToArray());
                Assert.NotNull(plain.Items[2].ValueExpression);
                Assert.AreEqual("1 << 4", plain.Items[2].ValueExpression.ToString());

                var scoped = compilation.Enums.Single(x => x.Name == "Scoped");
                Assert.True(scoped.IsScoped);
                Assert.AreEqual(CppPrimitiveType.UnsignedLongLong, scoped.IntegerType);
                Assert.AreEqual(1L << 40, scoped.Items.Single(x => x.Name == "Big").Value);

                var anonymousEnum = compilation.FindByName<CppEnum>("AnonymousEnum");
                Assert.NotNull(anonymousEnum);
                Assert.False(anonymousEnum.IsAnonymous);
                Assert.AreEqual(new long[] { 7, 8 }, anonymousEnum.Items.Select(x => x.Value).ToArray());

                var plainValue = compilation.Fields.Single(x => x.Name == "plainValue");
                Assert.NotNull(plainValue.InitExpression);
                Assert.True(plainValue.InitExpression.ToString().Contains("PlainOr"));

                var scopedValue = compilation.Fields.Single(x => x.Name == "scopedValue");
                Assert.AreEqual(scoped, scopedValue.Type);
                Assert.NotNull(scopedValue.InitExpression);
                Assert.True(scopedValue.InitExpression.ToString().Contains("Big"));
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++17" } });
    }
}
