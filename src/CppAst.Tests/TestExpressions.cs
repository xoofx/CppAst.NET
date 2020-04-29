// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestExpressions : InlineTestBase
    {
        [Test]
        public void TestInitListExpression()
        {
            ParseAssert(@"
#define INITGUID
#include <guiddef.h>
DEFINE_GUID(IID_ID3D11DeviceChild,0x1841e5c8,0x16b0,0x489b,0xbc,0xc8,0x44,0xcf,0xb0,0xd5,0xde,0xae);
", compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    Assert.AreEqual(1, compilation.Fields.Count);
                    var cppField = compilation.Fields[0];

                    Assert.Null(cppField.InitValue);

                    Assert.NotNull(cppField.InitExpression);
                    Assert.IsInstanceOf<CppInitListExpression>(cppField.InitExpression);

                    var toStr = cppField.InitExpression.ToString();

                    Assert.AreEqual("{0x1841e5c8, 0x16b0, 0x489b, {0xbc, 0xc8, 0x44, 0xcf, 0xb0, 0xd5, 0xde, 0xae}}", toStr);
                },
                new CppParserOptions().ConfigureForWindowsMsvc());
        }


        [Test]
        public void TestBinaryExpressions()
        {
            ParseAssert(@"
const int x = (0 + 1) << 2;
", compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Fields.Count);
                var cppField = compilation.Fields[0];

                Assert.NotNull(cppField.InitValue?.Value);
                Assert.AreEqual(4, cppField.InitValue.Value);

                Assert.NotNull(cppField.InitExpression);
                Assert.IsInstanceOf<CppBinaryExpression>(cppField.InitExpression);

                Assert.AreEqual("(0 + 1) << 2", cppField.InitExpression.ToString());
            });
        }

        [Test]
        public void TestUnaryExpressions()
        {
            ParseAssert(@"
const int x = ~(128 + 2);
", compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Fields.Count);
                var cppField = compilation.Fields[0];

                Assert.NotNull(cppField.InitValue?.Value);
                var result = ~(128 + 2);
                Assert.AreEqual(result, cppField.InitValue.Value);

                Assert.NotNull(cppField.InitExpression);
                Assert.IsInstanceOf<CppUnaryExpression>(cppField.InitExpression);

                Assert.AreEqual("~(128 + 2)", cppField.InitExpression.ToString());
            });
        }

        [Test]
        public void TestBinaryOr()
        {
            ParseAssert(@"
const int x = 12|1;
", compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Fields.Count);
                var cppField = compilation.Fields[0];

                Assert.NotNull(cppField.InitValue?.Value);
                var result = 12 | 1;
                Assert.AreEqual(result, cppField.InitValue.Value);

                Assert.NotNull(cppField.InitExpression);
                Assert.IsInstanceOf<CppBinaryExpression>(cppField.InitExpression);

                Assert.AreEqual("12 | 1", cppField.InitExpression.ToString());
            });
        }

        [Test]
        public void TestParameterDefaultValue()
        {
            ParseAssert(@"
void MyFunction(int x = (1 + 2) * 3);
", compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Functions.Count);
                var parameters = compilation.Functions[0].Parameters;
                Assert.AreEqual(1, parameters.Count);
                var cppParam = parameters[0];

                Assert.NotNull(cppParam.InitValue?.Value);
                Assert.AreEqual(9, cppParam.InitValue.Value);

                Assert.NotNull(cppParam.InitExpression);
                Assert.IsInstanceOf<CppBinaryExpression>(cppParam.InitExpression);

                Assert.AreEqual("(1 + 2) * 3", cppParam.InitExpression.ToString());

                Assert.AreEqual("void MyFunction(int x = (1 + 2) * 3)", compilation.Functions[0].ToString());
            });
        }

        [Test]
        public void TestNullPtrExpression()
        {
            ParseAssert(@"
const void* NullPtr = nullptr;
", compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(1, compilation.Fields.Count);
                var cppField = compilation.Fields[0];

                Assert.Null(cppField.InitValue?.Value);

                Assert.IsInstanceOf<CppRawExpression>(cppField.InitExpression);

                Assert.AreEqual("nullptr", cppField.InitExpression.ToString());
            });
        }
    }
}