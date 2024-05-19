using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestStructs : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
struct Struct0
{
};

struct Struct1 : Struct0
{
};

struct Struct2
{
    int field0;
};

struct Struct3
{
private:
    int field0;
public:
    float field1;
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(4, compilation.Classes.Count);

                    {
                        var cppStruct = compilation.Classes[0];
                        Assert.AreEqual("Struct0", cppStruct.Name);
                        Assert.AreEqual(0, cppStruct.Fields.Count);
                        Assert.AreEqual(sizeof(byte), cppStruct.SizeOf);
                        Assert.AreEqual(1, cppStruct.AlignOf);
                    }

                    {
                        var cppStruct = compilation.Classes[1];
                        Assert.AreEqual("Struct1", cppStruct.Name);
                        Assert.AreEqual(0, cppStruct.Fields.Count);
                        Assert.AreEqual(1, cppStruct.BaseTypes.Count);
                        Assert.True(cppStruct.BaseTypes[0].Type is CppClass);
                        Assert.True(ReferenceEquals(compilation.Classes[0], cppStruct.BaseTypes[0].Type));
                        Assert.AreEqual(sizeof(byte), cppStruct.SizeOf);
                        Assert.AreEqual(1, cppStruct.AlignOf);
                    }

                    {
                        var cppStruct = compilation.Classes[2];
                        Assert.AreEqual("Struct2", cppStruct.Name);
                        Assert.AreEqual(1, cppStruct.Fields.Count);
                        Assert.AreEqual("field0", cppStruct.Fields[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppStruct.Fields[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType) cppStruct.Fields[0].Type).Kind);
                        Assert.AreEqual(sizeof(int), cppStruct.SizeOf);
                        Assert.AreEqual(4, cppStruct.AlignOf);
                    }

                    {
                        var cppStruct = compilation.Classes[3];
                        Assert.AreEqual(2, cppStruct.Fields.Count);
                        Assert.AreEqual("field0", cppStruct.Fields[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppStruct.Fields[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType) cppStruct.Fields[0].Type).Kind);
                        Assert.AreEqual(CppVisibility.Private, cppStruct.Fields[0].Visibility);

                        Assert.AreEqual("field1", cppStruct.Fields[1].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppStruct.Fields[1].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Float, ((CppPrimitiveType) cppStruct.Fields[1].Type).Kind);
                        Assert.AreEqual(CppVisibility.Public, cppStruct.Fields[1].Visibility);
                        Assert.AreEqual(sizeof(int), cppStruct.Fields[1].Offset);
                        Assert.AreEqual(sizeof(int) + sizeof(float), cppStruct.SizeOf);
                        Assert.AreEqual(4, cppStruct.AlignOf);
                    }
                }
            );
        }


        [Test]
        public void TestAnonymous()
        {
            ParseAssert(@"
struct
{
    int a;
    int b;
} c;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);

                    {
                        var cppStruct = compilation.Classes[0];
                        Assert.AreEqual(string.Empty, cppStruct.Name);
                        Assert.AreEqual(2, cppStruct.Fields.Count);
                        Assert.AreEqual(sizeof(int), cppStruct.Fields[1].Offset);
                        Assert.AreEqual(sizeof(int) + sizeof(int), cppStruct.SizeOf);
                        Assert.AreEqual(4, cppStruct.AlignOf);
                    }
                }
            );
        }


        [Test]
        public void TestAnonymousUnion()
        {
            ParseAssert(@"
struct HelloWorld
{
    int a;
    union {
        int c;
        int d;
    };
    int b;
    union {
        int e;
        int f;
    };
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);

                    {
                        var cppStruct = compilation.Classes[0];
                        Assert.AreEqual(4, cppStruct.Fields.Count);

                        for (int i = 0; i < 4; i++)
                        {
                            Assert.AreEqual(i * 4, cppStruct.Fields[i].Offset);
                            Assert.AreEqual(4, cppStruct.Fields[i].Type.SizeOf);
                        }

                        // Check first union
                        Assert.AreEqual(string.Empty, cppStruct.Fields[1].Name);
                        Assert.IsInstanceOf<CppClass>(cppStruct.Fields[1].Type);
                        var cppUnion = ((CppClass)cppStruct.Fields[1].Type);
                        Assert.AreEqual(CppClassKind.Union, ((CppClass)cppStruct.Fields[1].Type).ClassKind);
                        Assert.AreEqual(2, cppUnion.Fields.Count);

                        // Check 2nd union
                        Assert.AreEqual(string.Empty, cppStruct.Fields[3].Name);
                        Assert.IsInstanceOf<CppClass>(cppStruct.Fields[3].Type);
                        cppUnion = ((CppClass)cppStruct.Fields[3].Type);
                        Assert.AreEqual(CppClassKind.Union, ((CppClass)cppStruct.Fields[3].Type).ClassKind);
                        Assert.AreEqual(2, cppUnion.Fields.Count);
                    }
                }
            );
        }

        [Test]
        public void TestAnonymousUnionWithField()
        {
            ParseAssert(@"
struct HelloWorld
{
    int a;
    union {
        int c;
        int d;
    } e;
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);

                    {
                        var cppStruct = compilation.Classes[0];

                        // Only one union
                        Assert.AreEqual(1, cppStruct.Classes.Count);

                        // Only 2 fields
                        Assert.AreEqual(2, cppStruct.Fields.Count);

                        // Check the union
                        Assert.AreEqual("e", cppStruct.Fields[1].Name);
                        Assert.IsInstanceOf<CppClass>(cppStruct.Fields[1].Type);
                        var cppUnion = ((CppClass)cppStruct.Fields[1].Type);
                        Assert.AreEqual(CppClassKind.Union, ((CppClass)cppStruct.Fields[1].Type).ClassKind);
                        Assert.AreEqual(2, cppUnion.Fields.Count);
                    }
                }
            );
        }

        [Test]
        public void TestAnonymousUnionWithField2()
        {
            ParseAssert(@"
struct HelloWorld
{
    int a;
    union {
        int c;
        int d;
    } e[4];
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);

                    {
                        var cppStruct = compilation.Classes[0];

                        // Only one union
                        Assert.AreEqual(1, cppStruct.Classes.Count);

                        // Only 2 fields
                        Assert.AreEqual(2, cppStruct.Fields.Count);

                        // Check the union
                        Assert.AreEqual("e", cppStruct.Fields[1].Name);
                        Assert.IsInstanceOf<CppArrayType>(cppStruct.Fields[1].Type);
                        var cppArrayType = ((CppArrayType)cppStruct.Fields[1].Type);
                        Assert.IsInstanceOf<CppClass>(cppArrayType.ElementType);
                        var cppUnion = ((CppClass)cppArrayType.ElementType);
                        Assert.AreEqual(CppClassKind.Union, cppUnion.ClassKind);
                        Assert.AreEqual(2, cppUnion.Fields.Count);
                    }
                }
            );
        }
    }
}
