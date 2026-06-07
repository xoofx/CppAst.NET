// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestParserOptionsAndDiagnostics : InlineTestBase
{
    [Test]
    public void TestParserKindSelectsCLanguageMode()
    {
        var cCompilation = CppParser.Parse(
            "namespace ns { int value; }",
            new CppParserOptions
            {
                ParserKind = CppParserKind.C,
                AdditionalArguments = { "-std=c11" },
            });

        Assert.True(cCompilation.HasErrors);
        Assert.True(cCompilation.Diagnostics.Messages.Any(x => x.Type == CppLogMessageType.Error));

        var cppCompilation = CppParser.Parse(
            "namespace ns { int value; }",
            new CppParserOptions { AdditionalArguments = { "-std=c++17" } });

        Assert.False(cppCompilation.HasErrors);
        Assert.AreEqual(1, cppCompilation.Namespaces.Count);
        Assert.AreEqual("ns", cppCompilation.Namespaces[0].Name);
    }

    [Test]
    public void TestDefinesAndPrePostHeadersAreApplied()
    {
        ParseAssert(@"
typedef PreType AliasFromPreHeader;
#if FEATURE_ENABLED
AliasFromPreHeader enabledField;
#endif
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(2, compilation.Typedefs.Count);
                Assert.AreEqual("PreType", compilation.Typedefs[0].Name);
                Assert.AreEqual("AliasFromPreHeader", compilation.Typedefs[1].Name);
                Assert.AreEqual(2, compilation.Fields.Count);
                Assert.AreEqual("enabledField", compilation.Fields[0].Name);
                Assert.AreEqual("postField", compilation.Fields[1].Name);
                Assert.True(compilation.InputText.Contains("typedef int PreType;"));
                Assert.True(compilation.InputText.Contains("int postField;"));
            },
            new CppParserOptions
            {
                Defines = { "FEATURE_ENABLED=1" },
                PreHeaderText = "typedef int PreType;",
                PostHeaderText = "int postField;",
            });
    }

    [Test]
    public void TestParseMacrosOptionControlsMacroCollection()
    {
        const string text = @"
#define LOCAL_VALUE 7
#define LOCAL_ADD(x, y) ((x) + (y))
int value = LOCAL_VALUE;
";

        ParseAssert(text,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(0, compilation.Macros.Count);
            });

        ParseAssert(text,
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                var macros = compilation.Macros.Where(x => x.Name.StartsWith("LOCAL_")).ToList();
                Assert.AreEqual(2, macros.Count);
                Assert.AreEqual("LOCAL_VALUE", macros[0].Name);
                Assert.AreEqual("7", macros[0].Value);
                Assert.AreEqual("LOCAL_ADD", macros[1].Name);
                Assert.AreEqual(new[] { "x", "y" }, macros[1].Parameters.ToArray());
            },
            new CppParserOptions().EnableMacros());
    }

    [Test]
    public void TestParseCommentsFalseSuppressesNonDoxygenComments()
    {
        ParseAssert(@"
// This should not be attached when ParseComments is false.
int value;
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Fields.Count);
                Assert.Null(compilation.Fields[0].Comment);
            },
            new CppParserOptions { ParseComments = false });
    }

    [Test]
    public void TestSyntaxErrorsProduceDiagnosticsWithoutThrowing()
    {
        CppCompilation compilation = null;
        Assert.DoesNotThrow(() => compilation = CppParser.Parse("int broken = ;"));

        Assert.NotNull(compilation);
        Assert.True(compilation.HasErrors);
        Assert.True(compilation.Diagnostics.Messages.Any(x => x.Type == CppLogMessageType.Error));
        Assert.True(compilation.Diagnostics.Messages.Any(x => x.Text.Contains("Compilation aborted")));
        Assert.AreEqual(0, compilation.Fields.Count);
    }

    [Test]
    public void TestConfigureForWindowsMsvcProvidesExpectedDefines()
    {
        ParseAssert(@"
#ifdef _WIN64
int win64Field;
#endif
#ifdef _MSC_VER
int msvcField;
#endif
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(new[] { "win64Field", "msvcField" }, compilation.Fields.Select(x => x.Name).ToArray());
            },
            new CppParserOptions().ConfigureForWindowsMsvc(CppTargetCpu.X86_64));
    }
}
