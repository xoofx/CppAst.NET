using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestMacros : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
#define MACRO0
#define MACRO1 1
#define MACRO2(x)
#define MACRO3(x) x + 1
#define MACRO4 (x)
#define MACRO5 1 /* with a comment */
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(6, compilation.Macros.Count);

                    {
                        var macro = compilation.Macros[0];
                        Assert.AreEqual("MACRO0", macro.Name);
                        Assert.AreEqual("", macro.Value);
                        Assert.AreEqual(0, macro.Tokens.Count);
                        Assert.Null(macro.Parameters);
                    }

                    {
                        var macro = compilation.Macros[1];
                        Assert.AreEqual("MACRO1", macro.Name);
                        Assert.AreEqual("1", macro.Value);
                        Assert.AreEqual(1, macro.Tokens.Count);
                        Assert.AreEqual("1", macro.Tokens[0].Text);
                        Assert.AreEqual(CppTokenKind.Literal, macro.Tokens[0].Kind);
                        Assert.Null(macro.Parameters);
                    }

                    {
                        var macro = compilation.Macros[2];
                        Assert.AreEqual("MACRO2", macro.Name);
                        Assert.AreEqual("", macro.Value);
                        Assert.NotNull(macro.Parameters);
                        Assert.AreEqual(1, macro.Parameters.Count);
                        Assert.AreEqual("x", macro.Parameters[0]);
                    }

                    {
                        var macro = compilation.Macros[3];
                        Assert.AreEqual("MACRO3", macro.Name);
                        Assert.AreEqual("x+1", macro.Value);
                        Assert.NotNull(macro.Parameters);
                        Assert.AreEqual(1, macro.Parameters.Count);
                        Assert.AreEqual("x", macro.Parameters[0]);

                        Assert.AreEqual(3, macro.Tokens.Count);
                        Assert.AreEqual("x", macro.Tokens[0].Text);
                        Assert.AreEqual("+", macro.Tokens[1].Text);
                        Assert.AreEqual("1", macro.Tokens[2].Text);
                        Assert.AreEqual(CppTokenKind.Identifier, macro.Tokens[0].Kind);
                        Assert.AreEqual(CppTokenKind.Punctuation, macro.Tokens[1].Kind);
                        Assert.AreEqual(CppTokenKind.Literal, macro.Tokens[2].Kind);
                    }

                    {
                        var macro = compilation.Macros[4];
                        Assert.AreEqual("MACRO4", macro.Name);
                        Assert.AreEqual("(x)", macro.Value);
                        Assert.Null(macro.Parameters);

                        Assert.AreEqual(3, macro.Tokens.Count);
                        Assert.AreEqual("(", macro.Tokens[0].Text);
                        Assert.AreEqual("x", macro.Tokens[1].Text);
                        Assert.AreEqual(")", macro.Tokens[2].Text);
                        Assert.AreEqual(CppTokenKind.Punctuation, macro.Tokens[0].Kind);
                        Assert.AreEqual(CppTokenKind.Identifier, macro.Tokens[1].Kind);
                        Assert.AreEqual(CppTokenKind.Punctuation, macro.Tokens[2].Kind);
                    }

                    {
                        var macro = compilation.Macros[5];
                        Assert.AreEqual("MACRO5", macro.Name);
                        Assert.AreEqual("1", macro.Value);
                        Assert.Null(macro.Parameters);

                        Assert.AreEqual(2, macro.Tokens.Count);
                        Assert.AreEqual("1", macro.Tokens[0].Text);
                        Assert.AreEqual("/* with a comment */", macro.Tokens[1].Text);
                        Assert.AreEqual(CppTokenKind.Literal, macro.Tokens[0].Kind);
                        Assert.AreEqual(CppTokenKind.Comment, macro.Tokens[1].Kind);
                    }
                }
                , new CppParserOptions().EnableMacros()
            );
        }
   }
}