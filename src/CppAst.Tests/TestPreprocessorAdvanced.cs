// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestPreprocessorAdvanced : InlineTestBase
{
    [Test]
    public void TestFunctionLikeVariadicStringizingTokenPastingAndMultilineMacros()
    {
        ParseAssert(@"
#define STR(x) #x
#define JOIN(a, b) a ## b
#define LOG(fmt, ...) log_impl(fmt, __VA_ARGS__)
#define MULTI(a, b) \
    ((a) + \
     (b))
#define WITH_COMMENT 1 /* note */
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var macros = compilation.Macros.Where(x => !x.Name.StartsWith("__")).ToDictionary(x => x.Name);
                Assert.True(macros.ContainsKey("STR"));
                Assert.True(macros.ContainsKey("JOIN"));
                Assert.True(macros.ContainsKey("LOG"));
                Assert.True(macros.ContainsKey("MULTI"));
                Assert.True(macros.ContainsKey("WITH_COMMENT"));

                var str = macros["STR"];
                Assert.AreEqual(new[] { "x" }, str.Parameters.ToArray());
                Assert.AreEqual("#x", str.Value);
                Assert.AreEqual("#", str.Tokens[0].Text);

                var join = macros["JOIN"];
                Assert.AreEqual(new[] { "a", "b" }, join.Parameters.ToArray());
                Assert.AreEqual("a##b", join.Value);
                Assert.True(join.Tokens.Any(x => x.Text == "##"));

                var log = macros["LOG"];
                Assert.AreEqual("fmt", log.Parameters[0]);
                Assert.True(log.Value.Contains("__VA_ARGS__"));

                var multi = macros["MULTI"];
                Assert.AreEqual(new[] { "a", "b" }, multi.Parameters.ToArray());
                Assert.True(multi.Value.Contains("a"));
                Assert.True(multi.Value.Contains("b"));

                var withComment = macros["WITH_COMMENT"];
                Assert.AreEqual("1", withComment.Tokens[0].Text);
                Assert.True(withComment.Tokens.Any(x => x.Kind == CppTokenKind.Comment && x.Text.Contains("note")));
            },
            new CppParserOptions().EnableMacros());
    }
}
