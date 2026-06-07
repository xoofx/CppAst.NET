// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestTemplatesAdvanced : InlineTestBase
{
    [Test]
    public void TestTemplateParameterKindsSpecializationsAliasesAndMemberTemplates()
    {
        ParseAssert(@"
template<typename T, int N = 4>
struct Array
{
    T values[N];
};

template<template<typename> class Wrapper, typename T>
struct UsesWrapper
{
    Wrapper<T> value;
};

template<typename... Args>
struct Tuple
{
};

template<typename T>
struct Holder
{
    template<typename U>
    U Convert(U value);
};

template<typename T>
struct Traits;

template<>
struct Traits<int>
{
    using Type = int;
};

template<typename T>
struct Traits<T*>
{
    using Type = T;
};

Array<int, 8> ints;
Array<float> defaultFloats;
Tuple<int, float, char> tupleField;
Holder<int> holderField;
Traits<int> intTraits;
Traits<double*> pointerTraits;
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var arrayTemplate = compilation.Classes.Single(x => x.Name == "Array" && x.TemplateKind == CppTemplateKind.TemplateClass);
                Assert.AreEqual(2, arrayTemplate.TemplateParameters.Count);
                Assert.IsInstanceOf<CppTemplateParameterType>(arrayTemplate.TemplateParameters[0]);
                Assert.IsInstanceOf<CppTemplateParameterNonType>(arrayTemplate.TemplateParameters[1]);
                Assert.AreEqual("N", ((CppTemplateParameterNonType)arrayTemplate.TemplateParameters[1]).Name);

                var ints = (CppClass)compilation.Fields.Single(x => x.Name == "ints").Type;
                Assert.AreEqual("Array", ints.Name);
                Assert.AreEqual(CppTemplateKind.TemplateSpecializedClass, ints.TemplateKind);
                Assert.AreEqual(new[] { "int", "8" }, ints.TemplateSpecializedArguments.Select(x => x.ArgString).ToArray());
                Assert.AreEqual(arrayTemplate, ints.SpecializedTemplate);

                var defaultFloats = (CppClass)compilation.Fields.Single(x => x.Name == "defaultFloats").Type;
                Assert.AreEqual("Array", defaultFloats.Name);
                Assert.AreEqual("float", defaultFloats.TemplateSpecializedArguments[0].ArgString);

                var tuple = (CppClass)compilation.Fields.Single(x => x.Name == "tupleField").Type;
                Assert.AreEqual("Tuple", tuple.Name);
                Assert.AreEqual(new[] { "int", "float", "char" }, tuple.TemplateSpecializedArguments.Select(x => x.ArgString).ToArray());

                var holderTemplate = compilation.Classes.Single(x => x.Name == "Holder" && x.TemplateKind == CppTemplateKind.TemplateClass);
                var convert = holderTemplate.Functions.Single(x => x.Name == "Convert");
                Assert.True(convert.IsFunctionTemplate);
                Assert.AreEqual(1, convert.TemplateParameters.Count);
                Assert.AreEqual("U", convert.TemplateParameters[0].ToString());
                Assert.AreEqual("U", convert.ReturnType.GetDisplayName());

                var usesWrapper = compilation.Classes.Single(x => x.Name == "UsesWrapper" && x.TemplateKind == CppTemplateKind.TemplateClass);
                Assert.AreEqual(2, usesWrapper.TemplateParameters.Count);
                Assert.AreEqual("Wrapper", usesWrapper.TemplateParameters[0].ToString());
                Assert.AreEqual("T", usesWrapper.TemplateParameters[1].ToString());

                var traitsInt = compilation.Classes.Single(x => x.Name == "Traits" && x.TemplateKind == CppTemplateKind.TemplateSpecializedClass && x.TemplateSpecializedArguments.Any(a => a.ArgString == "int"));
                Assert.AreEqual(1, traitsInt.Typedefs.Count);
                Assert.AreEqual(CppPrimitiveType.Int, traitsInt.Typedefs[0].ElementType);

                var partialTraits = compilation.Classes.Single(x => x.Name == "Traits" && x.TemplateKind == CppTemplateKind.PartialTemplateClass);
                Assert.True(partialTraits.TemplateSpecializedArguments[0].ArgString.Contains("*"));
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++17" } });
    }
}
