// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestCDeclarationsAdvanced : InlineTestBase
{
    [Test]
    public void TestCStorageQualifiersPointersBitFieldsFlexibleArraysAndCallbacks()
    {
        ParseAssert(@"
typedef struct Forward Forward;
struct Forward
{
    int id;
};

typedef enum Status
{
    Status_Ok = 0,
    Status_Fail = -1,
} Status;

struct Packet
{
    unsigned int version : 3;
    unsigned int : 0;
    unsigned int flags : 5;
    union
    {
        int i;
        float f;
    };
    const char* name;
    int values[];
};

extern const volatile int* restrict external_values;
static int internal_counter;
typedef void (*Callback)(struct Packet* packet, Status status);
void register_callback(Callback callback, int values[]);
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var forward = compilation.Classes.Single(x => x.Name == "Forward");
                Assert.AreEqual(CppClassKind.Struct, forward.ClassKind);
                Assert.AreEqual(1, forward.Fields.Count);
                Assert.AreEqual("id", forward.Fields[0].Name);

                var status = compilation.Enums.Single(x => x.Name == "Status");
                Assert.AreEqual(new[] { "Status_Ok", "Status_Fail" }, status.Items.Select(x => x.Name).ToArray());
                Assert.AreEqual(0, status.Items[0].Value);
                Assert.AreEqual(-1, status.Items[1].Value);

                var packet = compilation.Classes.Single(x => x.Name == "Packet");
                var version = packet.Fields.Single(x => x.Name == "version");
                Assert.True(version.IsBitField);
                Assert.AreEqual(3, version.BitFieldWidth);

                var flags = packet.Fields.Single(x => x.Name == "flags");
                Assert.True(flags.IsBitField);
                Assert.AreEqual(5, flags.BitFieldWidth);

                var anonymousUnionField = packet.Fields.Single(x => x.Name == string.Empty && x.Type is CppClass);
                var anonymousUnion = (CppClass)anonymousUnionField.Type;
                Assert.True(anonymousUnion.IsAnonymous);
                Assert.AreEqual(CppClassKind.Union, anonymousUnion.ClassKind);
                Assert.AreEqual(new[] { "i", "f" }, anonymousUnion.Fields.Select(x => x.Name).ToArray());

                var values = packet.Fields.Single(x => x.Name == "values");
                Assert.IsInstanceOf<CppArrayType>(values.Type);
                Assert.AreEqual(-1, ((CppArrayType)values.Type).Size);

                var externalValues = compilation.Fields.Single(x => x.Name == "external_values");
                Assert.AreEqual(CppStorageQualifier.Extern, externalValues.StorageQualifier);
                Assert.IsInstanceOf<CppPointerType>(externalValues.Type);

                var internalCounter = compilation.Fields.Single(x => x.Name == "internal_counter");
                Assert.AreEqual(CppStorageQualifier.Static, internalCounter.StorageQualifier);

                var callbackTypedef = compilation.Typedefs.Single(x => x.Name == "Callback");
                Assert.IsInstanceOf<CppPointerType>(callbackTypedef.ElementType);
                var callbackPointer = (CppPointerType)callbackTypedef.ElementType;
                Assert.IsInstanceOf<CppFunctionType>(callbackPointer.ElementType);
                var callback = (CppFunctionType)callbackPointer.ElementType;
                Assert.AreEqual(CppPrimitiveType.Void, callback.ReturnType);
                Assert.AreEqual(new[] { "packet", "status" }, callback.Parameters.Select(x => x.Name).ToArray());
                Assert.AreEqual("Packet *", callback.Parameters[0].Type.GetDisplayName());
                Assert.AreEqual("Status", callback.Parameters[1].Type.GetDisplayName());

                var registerCallback = compilation.Functions.Single(x => x.Name == "register_callback");
                Assert.AreEqual(2, registerCallback.Parameters.Count);
                Assert.AreEqual("Callback", registerCallback.Parameters[0].Type.GetDisplayName());
                Assert.IsInstanceOf<CppArrayType>(registerCallback.Parameters[1].Type);
                Assert.AreEqual(-1, ((CppArrayType)registerCallback.Parameters[1].Type).Size);
            },
            new CppParserOptions
            {
                ParserKind = CppParserKind.C,
                AdditionalArguments = { "-std=c11" },
            });
    }
}
