// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

namespace CppAst.Tests;

public class TestOpenIssueCharacterization
{
    [Test]
    public void TestIssue119SizeTTracksTargetPointerSizeWithoutChangingMappingPolicy()
    {
        const string text = @"
typedef __SIZE_TYPE__ size_t;
size_t value;
";

        var x86 = CppParser.Parse(text, new CppParserOptions
        {
            ParserKind = CppParserKind.C,
            AdditionalArguments = { "-std=c11" },
        }.ConfigureForWindowsMsvc(CppTargetCpu.X86));

        Assert.False(x86.HasErrors);
        var x86SizeT = x86.Typedefs.Single(x => x.Name == "size_t");
        Assert.AreEqual(4, x86SizeT.SizeOf);
        Assert.AreEqual(x86SizeT, x86.Fields.Single(x => x.Name == "value").Type);

        var x64 = CppParser.Parse(text, new CppParserOptions
        {
            ParserKind = CppParserKind.C,
            AdditionalArguments = { "-std=c11" },
        }.ConfigureForWindowsMsvc(CppTargetCpu.X86_64));

        Assert.False(x64.HasErrors);
        var x64SizeT = x64.Typedefs.Single(x => x.Name == "size_t");
        Assert.AreEqual(8, x64SizeT.SizeOf);
        Assert.AreEqual(x64SizeT, x64.Fields.Single(x => x.Name == "value").Type);
    }

    [Test]
    public void TestIssues106And116SystemHeaderLinkageSpecDoesNotThrow()
    {
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "system-parent-null", Guid.NewGuid().ToString("N"));
        var systemDirectory = Path.Combine(directory, "system");
        Directory.CreateDirectory(systemDirectory);

        File.WriteAllText(Path.Combine(systemDirectory, "corecrt_like.h"), @"
extern ""C""
{
typedef struct CoreCrtLike
{
    int value;
} CoreCrtLike;
}
");

        var rootHeader = Path.Combine(directory, "root.h");
        File.WriteAllText(rootHeader, "#include <corecrt_like.h>\n");

        CppCompilation compilation = null;
        Assert.DoesNotThrow(() => compilation = CppParser.ParseFile(rootHeader, new CppParserOptions
        {
            SystemIncludeFolders = { systemDirectory },
            ParseSystemIncludes = true,
        }));

        Assert.NotNull(compilation);
        Assert.False(compilation.HasErrors);
        Assert.AreEqual(1, compilation.System.Classes.Count);
        Assert.AreEqual("CoreCrtLike", compilation.System.Classes[0].Name);
    }
}
