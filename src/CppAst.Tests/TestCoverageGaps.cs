// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestCoverageGaps : InlineTestBase
{
    [Test]
    public void TestCStructBitFieldsUnionArrayAndFunctionPointer()
    {
        ParseAssert(@"
typedef enum Flags
{
    FlagA = 1,
    FlagB = 2,
} Flags;

typedef struct Packet
{
    unsigned char tag;
    unsigned int version : 3;
    unsigned int flags : 5;
    union
    {
        int i;
        float f;
    } payload;
    int values[4];
} Packet;

void process_packet(const Packet* packet, void (*callback)(Packet* item));
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var flagsEnum = compilation.Enums.Single(x => x.Name == "Flags");
                Assert.AreEqual(2, flagsEnum.Items.Count);
                Assert.AreEqual("FlagA", flagsEnum.Items[0].Name);
                Assert.AreEqual(1, flagsEnum.Items[0].Value);

                var packet = compilation.Classes.Single(x => x.Name == "Packet");
                Assert.AreEqual(CppClassKind.Struct, packet.ClassKind);
                Assert.AreEqual(new[] { "tag", "version", "flags", "payload", "values" }, packet.Fields.Select(x => x.Name).ToArray());
                Assert.True(packet.Fields[1].IsBitField);
                Assert.AreEqual(3, packet.Fields[1].BitFieldWidth);
                Assert.True(packet.Fields[2].IsBitField);
                Assert.AreEqual(5, packet.Fields[2].BitFieldWidth);

                Assert.IsInstanceOf<CppClass>(packet.Fields[3].Type);
                Assert.AreEqual(CppClassKind.Union, ((CppClass)packet.Fields[3].Type).ClassKind);

                Assert.IsInstanceOf<CppArrayType>(packet.Fields[4].Type);
                Assert.AreEqual(4, ((CppArrayType)packet.Fields[4].Type).Size);

                var function = compilation.Functions.Single(x => x.Name == "process_packet");
                Assert.AreEqual(2, function.Parameters.Count);
                Assert.AreEqual("packet", function.Parameters[0].Name);
                Assert.IsInstanceOf<CppPointerType>(function.Parameters[0].Type);
                Assert.AreEqual("Packet const *", function.Parameters[0].Type.GetDisplayName());

                Assert.AreEqual("callback", function.Parameters[1].Name);
                var callbackPointer = (CppPointerType)function.Parameters[1].Type;
                var callbackType = (CppFunctionType)callbackPointer.ElementType;
                Assert.AreEqual(CppPrimitiveType.Void, callbackType.ReturnType);
                Assert.AreEqual(1, callbackType.Parameters.Count);
                Assert.AreEqual("item", callbackType.Parameters[0].Name);
                Assert.AreEqual("Packet *", callbackType.Parameters[0].Type.GetDisplayName());
            },
            new CppParserOptions
            {
                ParserKind = CppParserKind.C,
                AdditionalArguments = { "-std=c11" },
            });
    }

    [Test]
    public void TestCppClassConstructorsAccessAndOutOfLineBody()
    {
        ParseAssert(@"
namespace outer
{
namespace inner
{
class Widget
{
public:
    Widget();
    explicit Widget(int value);
    int Get() const;
protected:
    static Widget Create();
private:
    int value_;
};

inline int Widget::Get() const
{
    return value_;
}
}
}
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var widget = compilation.Namespaces.Single(x => x.Name == "outer").Namespaces.Single(x => x.Name == "inner").Classes.Single(x => x.Name == "Widget");
                Assert.AreEqual(2, widget.Constructors.Count);
                Assert.AreEqual(CppVisibility.Public, widget.Constructors[0].Visibility);
                Assert.AreEqual(0, widget.Constructors[0].Parameters.Count);
                Assert.AreEqual(CppVisibility.Public, widget.Constructors[1].Visibility);
                Assert.AreEqual(1, widget.Constructors[1].Parameters.Count);
                Assert.AreEqual(CppPrimitiveType.Int, widget.Constructors[1].Parameters[0].Type);

                Assert.AreEqual(2, widget.Functions.Count);
                Assert.AreEqual("Get", widget.Functions[0].Name);
                Assert.True(widget.Functions[0].IsConst);
                Assert.True(widget.Functions[0].Flags.HasFlag(CppFunctionFlags.Inline));
                Assert.AreNotEqual(default(CppSourceSpan), widget.Functions[0].BodySpan);

                Assert.AreEqual("Create", widget.Functions[1].Name);
                Assert.AreEqual(CppVisibility.Protected, widget.Functions[1].Visibility);
                Assert.AreEqual(CppStorageQualifier.Static, widget.Functions[1].StorageQualifier);

                Assert.AreEqual(1, widget.Fields.Count);
                Assert.AreEqual(CppVisibility.Private, widget.Fields[0].Visibility);
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++11" }, ParseFunctionBodies = true });
    }
}
