// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestComments : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
// This is a header of the file

// This is a comment of f0
int f0;

// This is a comment of function0
void function0();

// This is a comment of MyStruct0
struct MyStruct0
{
};

// This is a comment of Enum0
enum Enum0
{
    // This is a comment of Enum0_item0
    Enum0_item0,
    // This is a comment of Enum0_item1
    Enum0_item1,
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var cppElements = compilation.Children().ToList();
                    Assert.AreEqual(4, cppElements.Count);

                    var results = cppElements.Select(x => (x.Comment, x.GetType())).ToList();

                    Assert.AreEqual(1, compilation.Enums.Count);

                    results.AddRange(compilation.Enums[0].Children().Select(x => (x.Comment, x.GetType())));

                    var expectedResults = new List<(string, Type)>()
                    {
                        ("This is a comment of Enum0", typeof(CppEnum)),
                        ("This is a comment of f0", typeof(CppField)),
                        ("This is a comment of MyStruct0", typeof(CppClass)),
                        ("This is a comment of function0", typeof(CppFunction)),
                        ("This is a comment of Enum0_item0", typeof(CppEnumItem)),
                        ("This is a comment of Enum0_item1", typeof(CppEnumItem)),
                    };
                    
                    Assert.AreEqual(expectedResults, results);
               }
            );
        }
    }
}