// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestNamespaces : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
namespace A
{
    namespace B {
        int b;
    }
};

namespace A
{
    int a;
};

namespace A::B::C
{
    int c;
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var namespaces = new List<string>() { "A", "B", "C" };

                    ICppGlobalDeclarationContainer container = compilation;

                    foreach (var nsName in namespaces)
                    {
                        Assert.AreEqual(1, container.Namespaces.Count);
                        var ns = container.Namespaces[0];
                        Assert.AreEqual(nsName, ns.Name);
                        Assert.AreEqual(1, ns.Fields.Count);
                        Assert.AreEqual(nsName.ToLowerInvariant(), ns.Fields[0].Name);

                        // Continue on the sub-namespaces
                        container = ns;
                    }
                }
            );
        }

        [Test]
        public void TestNamespacedTypedef() {
            ParseAssert(@"
namespace A
{
    typedef int (*a)(int b);
}
A::a c;
",
                compilation => {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Namespaces.Count);
                    ICppGlobalDeclarationContainer container = compilation.Namespaces[0];
                    Assert.AreEqual(1, container.Typedefs.Count);
                    Assert.AreEqual(1, compilation.Fields.Count);

                    CppTypedef typedef = container.Typedefs[0];
                    CppField field = compilation.Fields[0];

                    Assert.AreEqual(typedef, field.Type);
                }
            );
        }



        [Test]
        public void TestNamespaceFindByFullName()
        {
            var text = @"
namespace A
{
// Test using Template
template <typename T>
struct MyStruct;

using MyStructInt = MyStruct<int>;
}

";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Namespaces.Count);

                    var cppStruct = compilation.FindByFullName<CppClass>("A::MyStruct");
                    Assert.AreEqual(compilation.Namespaces[0].Classes[0], cppStruct);
                }
            );
        }

        [Test]
        public void TestInlineNamespace()
        {
            var text = @"
namespace A
{

inline namespace __1
{
    // Test using Template
    template <typename T>
    struct MyStruct;

    using MyStructInt = MyStruct<int>;
}

}

";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Namespaces.Count);

                    var inlineNs = compilation.Namespaces[0].Namespaces[0];
                    Assert.AreEqual(inlineNs.Name, "__1");
                    Assert.AreEqual(true, inlineNs.IsInlineNamespace);

                    var cppStruct = compilation.FindByFullName<CppClass>("A::MyStruct");
                    Assert.AreEqual(inlineNs.Classes[0], cppStruct);
                    Assert.AreEqual(cppStruct.FullName, "A::MyStruct<T>");

                    var cppTypedef = compilation.FindByFullName<CppTypedef>("A::MyStructInt");
                    var cppStructInt = cppTypedef.ElementType as CppClass;
                    //So now we can use this full name in exporter convenience.
                    Assert.AreEqual(cppStructInt.FullName, "A::MyStruct<int>");
                }
            );
        }
    }
}