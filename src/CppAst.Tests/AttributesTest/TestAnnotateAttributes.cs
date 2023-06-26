// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestAnnotateAttributes : InlineTestBase
    {
        [Test]
        public void TestAnnotateAttribute()
        {
            var text = @"

#if !defined(__cppast) 
#define __cppast(...)
#endif

__cppast(script, is_browsable=true, desc=""a function"")
void TestFunc()
{
}

enum class __cppast(script, is_browsable=true, desc=""a enum"") TestEnum
{
};

class __cppast(script, is_browsable=true, desc=""a class"") TestClass
{
  public:
    __cppast(desc=""a member function"")
    void TestMemberFunc();

    __cppast(desc=""a member field"")
    int X;
};
";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    //annotate attribute support on global function
                    var cppFunc = compilation.Functions[0];
                    Assert.AreEqual(1, cppFunc.Attributes.Count);
                    Assert.AreEqual(cppFunc.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(cppFunc.Attributes[0].Arguments, "script, is_browsable=true, desc=\"a function\"");

                    //annotate attribute support on enum
                    var cppEnum = compilation.Enums[0];
                    Assert.AreEqual(1, cppEnum.Attributes.Count);
                    Assert.AreEqual(cppEnum.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(cppEnum.Attributes[0].Arguments, "script, is_browsable=true, desc=\"a enum\"");

                    //annotate attribute support on class
                    var cppClass = compilation.Classes[0];
                    Assert.AreEqual(1, cppClass.Attributes.Count);
                    Assert.AreEqual(cppClass.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(cppClass.Attributes[0].Arguments, "script, is_browsable=true, desc=\"a class\"");
                    
                    Assert.AreEqual(1, cppClass.Functions.Count);
                    var memFunc = cppClass.Functions[0];
                    Assert.AreEqual(1, memFunc.Attributes.Count);
                    Assert.AreEqual(memFunc.Attributes[0].Arguments, "desc=\"a member function\"");
                    

                    Assert.AreEqual(1, cppClass.Fields.Count);
                    var memField = cppClass.Fields[0];
                    Assert.AreEqual(1, memField.Attributes.Count);
                    Assert.AreEqual(memField.Attributes[0].Arguments, "desc=\"a member field\"");
                }
            );
        }


        [Test]
        public void TestAnnotateAttributeInNamespace()
        {
            var text = @"

#if !defined(__cppast)
#define __cppast(...)
#endif

namespace __cppast(script, is_browsable=true, desc=""a namespace test"") TestNs{

}

";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    //annotate attribute support on namespace
                    var ns = compilation.Namespaces[0];
                    Assert.AreEqual(1, ns.Attributes.Count);
                    Assert.AreEqual(ns.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(ns.Attributes[0].Arguments, "script, is_browsable=true, desc=\"a namespace test\"");

                }
            );
        }

        [Test]
        public void TestAnnotateAttributeWithMacro()
        {
            var text = @"

#if !defined(__cppast)
#define __cppast(...)
#endif

#define UUID() 12345

__cppast(id=UUID(), desc=""a function with macro"")
void TestFunc()
{
}

";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    //annotate attribute support on namespace
                    var func = compilation.Functions[0];
                    Assert.AreEqual(1, func.Attributes.Count);
                    Assert.AreEqual(func.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(func.Attributes[0].Arguments, "id=12345, desc=\"a function with macro\"");

                }
            );
        }
    }
}
