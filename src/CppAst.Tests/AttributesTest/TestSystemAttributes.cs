// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestSystemAttributes : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
__declspec(dllimport) int i;
__declspec(dllexport) void func0();
extern ""C"" void __stdcall func1(int a, int b, int c);
void *fun2(int align) __attribute__((alloc_align(1)));
",
                compilation =>
                {

                    // Print diagnostic messages
                    foreach (var message in compilation.Diagnostics.Messages)
                        Console.WriteLine(message);

                    // Print All enums
                    foreach (var cppEnum in compilation.Enums)
                        Console.WriteLine(cppEnum);

                    // Print All functions
                    foreach (var cppFunction in compilation.Functions)
                        Console.WriteLine(cppFunction);

                    // Print All classes, structs
                    foreach (var cppClass in compilation.Classes)
                        Console.WriteLine(cppClass);

                    // Print All typedefs
                    foreach (var cppTypedef in compilation.Typedefs)
                        Console.WriteLine(cppTypedef);


                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Fields.Count);
                    Assert.NotNull(compilation.Fields[0].Attributes);
                    Assert.AreEqual("dllimport", compilation.Fields[0].Attributes[0].Name);

                    Assert.AreEqual(3, compilation.Functions.Count);
                    Assert.NotNull(compilation.Functions[0].Attributes);
                    Assert.AreEqual(1, compilation.Functions[0].Attributes.Count);
                    Assert.AreEqual("dllexport", compilation.Functions[0].Attributes[0].Name);

                    Assert.AreEqual(CppCallingConvention.X86StdCall, compilation.Functions[1].CallingConvention);

                    Assert.NotNull(compilation.Functions[2].Attributes);
                    Assert.AreEqual(1, compilation.Functions[2].Attributes.Count);
                    Assert.AreEqual("allocalign", compilation.Functions[2].Attributes[0].Name);

                },
                new CppParserOptions() {  }.ConfigureForWindowsMsvc() // Force using X86 to get __stdcall calling convention
            );
        }

        [Test]
        public void TestStructAttributes()
        {
            ParseAssert(@"
struct __declspec(uuid(""1841e5c8-16b0-489b-bcc8-44cfb0d5deae"")) __declspec(novtable) Test{
    int a;
    int b;
};", compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);

                    Assert.NotNull(compilation.Classes[0].Attributes);

                    Assert.AreEqual(2, compilation.Classes[0].Attributes.Count);

                    {
                        var attr = compilation.Classes[0].Attributes[0];
                        Assert.AreEqual("uuid", attr.Name);
                    }

                    {
                        var attr = compilation.Classes[0].Attributes[1];
                        Assert.AreEqual("msnovtable", attr.Name);
                    }
                },
                new CppParserOptions() {  }.ConfigureForWindowsMsvc());
        }

        [Test]
        public void TestCpp11VarAlignas()
        {
            ParseAssert(@"
alignas(128) char cacheline[128];", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Fields.Count);
                Assert.AreEqual(1, compilation.Fields[0].Attributes.Count);
                {
                    var attr = compilation.Fields[0].Attributes[0];
                    Assert.AreEqual("alignas", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11StructAlignas()
        {
            ParseAssert(@"
struct alignas(8) S {};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Classes.Count);
                Assert.AreEqual(1, compilation.Classes[0].Attributes.Count);
                {
                    var attr = compilation.Classes[0].Attributes[0];
                    Assert.AreEqual("alignas", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11StructAlignasWithAttribute()
        {
            ParseAssert(@"
struct [[deprecated(""abc"")]] alignas(8) S {};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Classes.Count);
                Assert.AreEqual(2, compilation.Classes[0].Attributes.Count);
                {
                    var attr = compilation.Classes[0].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }

                {
                    var attr = compilation.Classes[0].Attributes[1];
                    Assert.AreEqual("alignas", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" }}
          );
        }

        [Test]
        public void TestCpp11StructAttributes()
        {
            ParseAssert(@"
struct [[deprecated]] Test{
    int a;
    int b;
};

struct [[deprecated(""old"")]] TestMessage{
    int a;
    int b;
};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(2, compilation.Classes.Count);
                Assert.AreEqual(1, compilation.Classes[0].Attributes.Count);
                {
                    var attr = compilation.Classes[0].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }

                Assert.AreEqual(1, compilation.Classes[1].Attributes.Count);
                {
                    var attr = compilation.Classes[1].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11VariablesAttributes()
        {
            ParseAssert(@"
struct Test{
    [[deprecated]] int a;
    int b;
};

[[deprecated]] int x;", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Classes.Count);
                Assert.AreEqual(2, compilation.Classes[0].Fields.Count);
                Assert.AreEqual(1, compilation.Classes[0].Fields[0].Attributes.Count);
                {
                    var attr = compilation.Classes[0].Fields[0].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }

                Assert.AreEqual(1, compilation.Fields.Count);
                Assert.AreEqual(1, compilation.Fields[0].Attributes.Count);
                {
                    var attr = compilation.Fields[0].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11FunctionsAttributes()
        {
            ParseAssert(@"
[[noreturn]] void x() {};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Functions.Count);
                Assert.AreEqual(1, compilation.Functions[0].Attributes.Count);
                {
                    var attr = compilation.Functions[0].Attributes[0];
                    Assert.AreEqual("cxx11noreturn", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11NamespaceAttributes()
        {
            ParseAssert(@"
namespace [[deprecated]] cppast {};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Namespaces.Count);
                Assert.AreEqual(1, compilation.Namespaces[0].Attributes.Count);
                {
                    var attr = compilation.Namespaces[0].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11EnumAttributes()
        {
            ParseAssert(@"
enum [[deprecated]] E { };", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Enums.Count);
                Assert.AreEqual(1, compilation.Enums[0].Attributes.Count);
                {
                    var attr = compilation.Enums[0].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }

        [Test]
        public void TestCpp11TemplateStructAttributes()
        {
            ParseAssert(@"
template<typename T> struct X {};
template<> struct [[deprecated]] X<int> {};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(2, compilation.Classes.Count);
                Assert.AreEqual(0, compilation.Classes[0].Attributes.Count);
                Assert.AreEqual(1, compilation.Classes[1].Attributes.Count);
                {
                    var attr = compilation.Classes[1].Attributes[0];
                    Assert.AreEqual("deprecated", attr.Name);
                }
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" } }
          );
        }


        [Test]
        public void TestCppNoParseOptionsAttributes()
        {
            ParseAssert(@"
[[noreturn]] void x() {};", compilation =>
            {
                Assert.False(compilation.HasErrors);

                Assert.AreEqual(1, compilation.Functions.Count);
                Assert.AreEqual(1, compilation.Functions[0].Attributes.Count);
            },
            // we are using a C++14 attribute because it can be used everywhere
            new CppParserOptions() { AdditionalArguments = { "-std=c++14" }}
          );
        }
        
        [Test]
        public void TestClassPublicExportAttribute()
        {
            var text = @"
#ifdef WIN32
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API __attribute__((visibility(""default"")))
#endif
class EXPORT_API TestClass
{
};
";
            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var cppClass = compilation.Classes[0];
                    Assert.AreEqual(1, cppClass.Attributes.Count);
                    Assert.True(cppClass.IsPublicExport());
                    
                },
                new CppParserOptions() {  }
            );
            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var cppClass = compilation.Classes[0];
                    Assert.AreEqual(1, cppClass.Attributes.Count);
                    Assert.True(cppClass.IsPublicExport());
                }, new CppParserOptions() { }.ConfigureForWindowsMsvc()
            );
        }

    }
}
