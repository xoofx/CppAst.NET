// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace CppAst.Tests;

public class TestModelInvariants : InlineTestBase
{
    [Test]
    public void TestMixedHeaderPreservesParentsLookupAndSourceSpans()
    {
        ParseAssert(@"
namespace api
{
inline namespace v1
{
enum Kind
{
    Kind_A = 1,
    Kind_B = 2,
};

struct Record
{
    Kind kind;
    int value;
};

void overload(int value);
void overload(float value);
}
}
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var apiNamespace = compilation.Namespaces.Single(x => x.Name == "api");
                var inlineNamespace = apiNamespace.Namespaces.Single(x => x.Name == "v1");
                Assert.True(inlineNamespace.IsInlineNamespace);

                var record = compilation.FindByFullName<CppClass>("api::Record");
                Assert.NotNull(record);
                Assert.AreEqual("api::Record", record.FullName);
                Assert.AreEqual(inlineNamespace.Classes.Single(x => x.Name == "Record"), record);

                var kind = compilation.FindByFullName<CppEnum>("api::Kind");
                Assert.NotNull(kind);
                Assert.AreEqual(inlineNamespace.Enums.Single(x => x.Name == "Kind"), kind);
                Assert.AreEqual(kind, record.Fields[0].Type);

                var overloads = compilation.FindListByName(inlineNamespace, "overload").OfType<CppFunction>().ToList();
                Assert.AreEqual(2, overloads.Count);
                Assert.AreEqual(new[] { "int", "float" }, overloads.Select(x => x.Parameters[0].Type.GetDisplayName()).ToArray());

                var children = compilation.Children().ToList();
                Assert.AreEqual(1, children.Count);
                Assert.AreEqual(apiNamespace, children[0]);

                foreach (var element in EnumerateElements(compilation))
                {
                    Assert.NotNull(element.Parent, $"{element} should have a parent container");
                }

                AssertSourceSpan(apiNamespace);
                AssertSourceSpan(inlineNamespace);
                AssertSourceSpan(kind);
                AssertSourceSpan(kind.Items[0]);
                AssertSourceSpan(record);
                AssertSourceSpan(record.Fields[0]);
                AssertSourceSpan(overloads[0]);
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++17" } });
    }

    private static IEnumerable<CppElement> EnumerateElements(ICppContainer container)
    {
        foreach (var child in container.Children().OfType<CppElement>())
        {
            yield return child;

            if (child is ICppContainer childContainer)
            {
                foreach (var descendant in EnumerateElements(childContainer))
                {
                    yield return descendant;
                }
            }

            if (child is CppEnum cppEnum)
            {
                foreach (var enumItem in cppEnum.Items)
                {
                    yield return enumItem;
                }
            }
        }
    }

    private static void AssertSourceSpan(CppElement element)
    {
        Assert.False(string.IsNullOrEmpty(element.SourceFile), $"{element} should have a source file");
        Assert.Greater(element.Span.Start.Line, 0, $"{element} should have a start line");
        Assert.GreaterOrEqual(element.Span.End.Offset, element.Span.Start.Offset, $"{element} should have a non-negative span");
    }
}
