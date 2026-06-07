// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestGlobalVariablesAdvanced : InlineTestBase
{
    [Test]
    public void TestGlobalVariablesCoverArraysPointersCallbacksAndCppInlineConstants()
    {
        ParseAssert(@"
enum Kind
{
    Kind_A = 1,
};

extern int externalValue;
static const char* name = ""hello"";
int numbers[3] = { 1, 2, 3 };
void (*globalCallback)(int code);
const char letter = 'x';
const int fromEnum = Kind_A;
inline constexpr int cppInline = 5;
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(7, compilation.Fields.Count);
                Assert.AreEqual(CppStorageQualifier.Extern, compilation.Fields.Single(x => x.Name == "externalValue").StorageQualifier);

                var name = compilation.Fields.Single(x => x.Name == "name");
                Assert.AreEqual(CppStorageQualifier.Static, name.StorageQualifier);
                Assert.True(name.Type.GetDisplayName().Contains("char"));
                Assert.NotNull(name.InitExpression);
                Assert.True(name.InitExpression.ToString().Contains("hello"));

                var numbers = compilation.Fields.Single(x => x.Name == "numbers");
                Assert.IsInstanceOf<CppArrayType>(numbers.Type);
                Assert.AreEqual(3, ((CppArrayType)numbers.Type).Size);
                Assert.NotNull(numbers.InitExpression);

                var callbackField = compilation.Fields.Single(x => x.Name == "globalCallback");
                Assert.IsInstanceOf<CppPointerType>(callbackField.Type);
                var callbackPointer = (CppPointerType)callbackField.Type;
                Assert.IsInstanceOf<CppFunctionType>(callbackPointer.ElementType);
                var callback = (CppFunctionType)callbackPointer.ElementType;
                Assert.AreEqual(CppPrimitiveType.Void, callback.ReturnType);
                Assert.AreEqual("code", callback.Parameters[0].Name);
                Assert.AreEqual(CppPrimitiveType.Int, callback.Parameters[0].Type);

                var letter = compilation.Fields.Single(x => x.Name == "letter");
                Assert.AreEqual("'x'", letter.InitExpression.ToString());

                var fromEnum = compilation.Fields.Single(x => x.Name == "fromEnum");
                Assert.True(fromEnum.InitExpression.ToString().Contains("Kind_A"));

                var cppInline = compilation.Fields.Single(x => x.Name == "cppInline");
                Assert.NotNull(cppInline.InitValue);
                Assert.AreEqual(5, cppInline.InitValue.Value);
                Assert.Greater(cppInline.Span.Start.Line, 0);
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++17" } });
    }
}
