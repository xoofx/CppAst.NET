// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.IO;
using System.Linq;

namespace CppAst.Tests;

public class TestIncludesAdvanced
{
    [Test]
    public void TestDuplicateIncludesDoNotThrowAndRecordDirectives()
    {
        var directory = CreateTestDirectory();
        var includedHeader = Path.Combine(directory, "included.h");
        File.WriteAllText(includedHeader, @"
#pragma once
struct IncludedFromDuplicate
{
    int value;
};
");

        var rootHeader = Path.Combine(directory, "root.h");
        File.WriteAllText(rootHeader, @"
#include ""included.h""
#include ""included.h""
");

        CppCompilation compilation = null;
        Assert.DoesNotThrow(() => compilation = CppParser.ParseFile(rootHeader));

        Assert.NotNull(compilation);
        Assert.False(compilation.HasErrors);
        Assert.AreEqual(1, compilation.Classes.Count);
        Assert.AreEqual("IncludedFromDuplicate", compilation.Classes[0].Name);
        Assert.True(compilation.InclusionDirectives.Any(x => string.Equals(x.FileName, includedHeader, StringComparison.OrdinalIgnoreCase)));
    }

    [Test]
    public void TestNestedIncludesContributeDeclarationsAndDirectives()
    {
        var directory = CreateTestDirectory();
        var leafHeader = Path.Combine(directory, "leaf.h");
        File.WriteAllText(leafHeader, @"
struct Leaf
{
    int value;
};
");

        var middleHeader = Path.Combine(directory, "middle.h");
        File.WriteAllText(middleHeader, @"
#include ""leaf.h""
struct Middle
{
    Leaf leaf;
};
");

        var rootHeader = Path.Combine(directory, "root.h");
        File.WriteAllText(rootHeader, "#include \"middle.h\"\n");

        var compilation = CppParser.ParseFile(rootHeader);

        Assert.False(compilation.HasErrors);
        Assert.AreEqual(new[] { "Leaf", "Middle" }, compilation.Classes.Select(x => x.Name).ToArray());
        CollectionAssert.IsSubsetOf(new[] { leafHeader, middleHeader }, compilation.InclusionDirectives.Select(x => x.FileName).ToArray());
        Assert.AreEqual(compilation.Classes[0], compilation.Classes[1].Fields[0].Type);
    }

    [Test]
    public void TestIncludeFoldersResolveQuotedHeaders()
    {
        var directory = CreateTestDirectory();
        var includeDirectory = Path.Combine(directory, "include");
        Directory.CreateDirectory(includeDirectory);

        var apiHeader = Path.Combine(includeDirectory, "api_types.h");
        File.WriteAllText(apiHeader, @"
typedef int ApiInt;
struct ApiStruct
{
    ApiInt value;
};
");

        var rootHeader = Path.Combine(directory, "root.h");
        File.WriteAllText(rootHeader, @"
#include ""api_types.h""
ApiStruct globalApiStruct;
");

        var compilation = CppParser.ParseFile(rootHeader, new CppParserOptions { IncludeFolders = { includeDirectory } });

        Assert.False(compilation.HasErrors);
        Assert.AreEqual(1, compilation.Typedefs.Count);
        Assert.AreEqual("ApiInt", compilation.Typedefs[0].Name);
        Assert.AreEqual(1, compilation.Classes.Count);
        Assert.AreEqual("ApiStruct", compilation.Classes[0].Name);
        Assert.AreEqual(1, compilation.Fields.Count);
        Assert.AreEqual(compilation.Classes[0], compilation.Fields[0].Type);
    }

    [Test]
    public void TestParseSystemIncludesFiltersSyntheticSystemHeader()
    {
        var directory = CreateTestDirectory();
        var systemDirectory = Path.Combine(directory, "system");
        Directory.CreateDirectory(systemDirectory);

        var systemHeader = Path.Combine(systemDirectory, "system_api.h");
        File.WriteAllText(systemHeader, @"
struct SystemOnly
{
    int value;
};
");

        var rootHeader = Path.Combine(directory, "root.h");
        File.WriteAllText(rootHeader, "#include <system_api.h>\n");

        var filtered = CppParser.ParseFile(rootHeader, new CppParserOptions
        {
            ParseSystemIncludes = false,
            SystemIncludeFolders = { systemDirectory },
        });

        Assert.False(filtered.HasErrors);
        Assert.AreEqual(0, filtered.Classes.Count);
        Assert.AreEqual(0, filtered.System.Classes.Count);
        Assert.True(filtered.InclusionDirectives.Any(x => string.Equals(x.FileName, systemHeader, StringComparison.OrdinalIgnoreCase)));

        var included = CppParser.ParseFile(rootHeader, new CppParserOptions
        {
            ParseSystemIncludes = true,
            SystemIncludeFolders = { systemDirectory },
        });

        Assert.False(included.HasErrors);
        Assert.AreEqual(0, included.Classes.Count);
        Assert.AreEqual(1, included.System.Classes.Count);
        Assert.AreEqual("SystemOnly", included.System.Classes[0].Name);
    }

    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "includes-advanced", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}
