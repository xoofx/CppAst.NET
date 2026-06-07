// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestExpressionsAdvanced : InlineTestBase
{
    [Test]
    public void TestCxxInitializersCastsLiteralsEnumRefsAndDefaultParameters()
    {
        ParseAssert(@"
#define BASE_VALUE 10

enum Flags
{
    Flag_A = 1,
    Flag_B = 2,
};

struct Pair
{
    int first;
    int second;
};

const int ternaryValue = true ? 1 : 2;
const int staticCastValue = static_cast<int>(3.5);
const int cCastValue = (int)4.5;
const bool boolValue = false;
const char charValue = 'z';
const char* textValue = ""abc"";
const Flags flagValue = (Flags)(Flag_A | Flag_B);
Pair pairValue = { 1, 2 };
int arrayValue[2] = { 3, 4 };
const int macroValue = BASE_VALUE + 2;
void defaults(const char* text = ""abc"", Flags flags = Flag_A, Pair pair = { 5, 6 });
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var ternary = compilation.Fields.Single(x => x.Name == "ternaryValue");
                Assert.AreEqual(CppExpressionKind.ConditionalOperator, ternary.InitExpression.Kind);
                Assert.AreEqual("true?1:2", ternary.InitExpression.ToString());

                var staticCast = compilation.Fields.Single(x => x.Name == "staticCastValue");
                Assert.AreEqual(CppExpressionKind.CXXStaticCast, staticCast.InitExpression.Kind);
                Assert.True(staticCast.InitExpression.ToString().Contains("static_cast"));

                var cCast = compilation.Fields.Single(x => x.Name == "cCastValue");
                Assert.AreEqual(CppExpressionKind.CStyleCast, cCast.InitExpression.Kind);
                Assert.True(cCast.InitExpression.ToString().Contains("(int)"));

                var boolValue = compilation.Fields.Single(x => x.Name == "boolValue");
                Assert.AreEqual(CppExpressionKind.CXXBoolLiteral, boolValue.InitExpression.Kind);
                Assert.AreEqual("false", boolValue.InitExpression.ToString());

                var charValue = compilation.Fields.Single(x => x.Name == "charValue");
                Assert.AreEqual(CppExpressionKind.CharacterLiteral, charValue.InitExpression.Kind);
                Assert.AreEqual("'z'", charValue.InitExpression.ToString());

                var textValue = compilation.Fields.Single(x => x.Name == "textValue");
                Assert.True(textValue.InitExpression.Kind == CppExpressionKind.StringLiteral || textValue.InitExpression.Kind == CppExpressionKind.Unexposed);
                Assert.AreEqual("\"abc\"", textValue.InitExpression.ToString());

                var flagValue = compilation.Fields.Single(x => x.Name == "flagValue");
                Assert.AreEqual(CppExpressionKind.CStyleCast, flagValue.InitExpression.Kind);
                Assert.True(flagValue.InitExpression.ToString().Contains("Flag_A"));
                Assert.True(flagValue.InitExpression.ToString().Contains("Flag_B"));

                Assert.AreEqual("{1, 2}", compilation.Fields.Single(x => x.Name == "pairValue").InitExpression.ToString());
                var arrayValue = compilation.Fields.Single(x => x.Name == "arrayValue");
                Assert.IsInstanceOf<CppArrayType>(arrayValue.Type);
                Assert.AreEqual(2, ((CppArrayType)arrayValue.Type).Size);
                Assert.NotNull(arrayValue.InitExpression);

                var macroValue = compilation.Fields.Single(x => x.Name == "macroValue");
                Assert.AreEqual(12, macroValue.InitValue.Value);
                Assert.AreEqual("10 + 2", macroValue.InitExpression.ToString());

                var defaults = compilation.Functions.Single(x => x.Name == "defaults");
                Assert.AreEqual(3, defaults.Parameters.Count);
                Assert.AreEqual("\"abc\"", defaults.Parameters[0].InitExpression.ToString());
                Assert.AreEqual("Flag_A", defaults.Parameters[1].InitExpression.ToString());
                Assert.AreEqual("{5, 6}", defaults.Parameters[2].InitExpression.ToString());
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++17" } });
    }
}
