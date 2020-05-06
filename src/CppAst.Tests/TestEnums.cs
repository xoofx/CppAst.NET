using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestEnums : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
enum Enum0
{
    Enum0_item0,
    Enum0_item1,
    Enum0_item2
};

enum class Enum1
{
    item0,
    item1,
    item2
};

enum class Enum2 : short
{
    item0 = 3,
    item1 = 4,
    item2 = 5
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Enums.Count);

                    {
                        var cppEnum = compilation.Enums[0];
                        Assert.AreEqual("Enum0", cppEnum.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppEnum.IntegerType.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppEnum.IntegerType).Kind);
                        Assert.AreEqual(3, cppEnum.Items.Count);
                        Assert.AreEqual(sizeof(int), cppEnum.SizeOf);
                        Assert.False(cppEnum.IsScoped);
                        Assert.AreEqual("Enum0_item0", cppEnum.Items[0].Name);
                        Assert.AreEqual("Enum0_item1", cppEnum.Items[1].Name);
                        Assert.AreEqual("Enum0_item2", cppEnum.Items[2].Name);
                        Assert.AreEqual(0, cppEnum.Items[0].Value);
                        Assert.AreEqual(1, cppEnum.Items[1].Value);
                        Assert.AreEqual(2, cppEnum.Items[2].Value);

                        var cppEnum1 = compilation.FindByName<CppEnum>("Enum0");
                        Assert.AreEqual(cppEnum, cppEnum1);
                    }

                    {
                        var cppEnum = compilation.Enums[1];
                        Assert.AreEqual("Enum1", cppEnum.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppEnum.IntegerType.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppEnum.IntegerType).Kind);
                        Assert.AreEqual(3, cppEnum.Items.Count);
                        Assert.AreEqual(sizeof(int), cppEnum.SizeOf);
                        Assert.True(cppEnum.IsScoped);
                        Assert.AreEqual("item0", cppEnum.Items[0].Name);
                        Assert.AreEqual("item1", cppEnum.Items[1].Name);
                        Assert.AreEqual("item2", cppEnum.Items[2].Name);
                        Assert.AreEqual(0, cppEnum.Items[0].Value);
                        Assert.AreEqual(1, cppEnum.Items[1].Value);
                        Assert.AreEqual(2, cppEnum.Items[2].Value);
                    }

                    {
                        var cppEnum = compilation.Enums[2];
                        Assert.AreEqual("Enum2", cppEnum.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppEnum.IntegerType.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Short, ((CppPrimitiveType)cppEnum.IntegerType).Kind);
                        Assert.AreEqual(3, cppEnum.Items.Count);
                        Assert.AreEqual(sizeof(short), cppEnum.SizeOf);
                        Assert.True(cppEnum.IsScoped);
                        Assert.AreEqual("item0", cppEnum.Items[0].Name);
                        Assert.AreEqual("item1", cppEnum.Items[1].Name);
                        Assert.AreEqual("item2", cppEnum.Items[2].Name);
                        Assert.AreEqual(3, cppEnum.Items[0].Value);
                        Assert.AreEqual(4, cppEnum.Items[1].Value);
                        Assert.AreEqual(5, cppEnum.Items[2].Value);
                    }
                }
            );
        }
    }
}