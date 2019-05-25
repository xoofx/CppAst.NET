using NUnit.Framework;

namespace CppAst.Tests
{
    public class InlineTests : InlineTestBase
    {
        [Test]
        public void TestMacros()
        {
            ParseAssert(@"
#define MACRO0
#define MACRO1 1
#define MACRO2(x)
#define MACRO3(x) x + 1
#define MACRO4 (x)
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(5, compilation.Macros.Count);

                    {
                        var macro = compilation.Macros[0];
                        Assert.AreEqual("MACRO0", macro.Name);
                        Assert.AreEqual("", macro.Value);
                        Assert.AreEqual(0, macro.Tokens.Count);
                        Assert.Null(macro.Parameters);
                    }

                    {
                        var macro = compilation.Macros[1];
                        Assert.AreEqual("MACRO1", macro.Name);
                        Assert.AreEqual("1", macro.Value);
                        Assert.AreEqual(1, macro.Tokens.Count);
                        Assert.AreEqual("1", macro.Tokens[0].Text);
                        Assert.AreEqual(CppTokenKind.Literal, macro.Tokens[0].Kind);
                        Assert.Null(macro.Parameters);
                    }

                    {
                        var macro = compilation.Macros[2];
                        Assert.AreEqual("MACRO2", macro.Name);
                        Assert.AreEqual("", macro.Value);
                        Assert.NotNull(macro.Parameters);
                        Assert.AreEqual(1, macro.Parameters.Count);
                        Assert.AreEqual("x", macro.Parameters[0]);
                    }

                    {
                        var macro = compilation.Macros[3];
                        Assert.AreEqual("MACRO3", macro.Name);
                        Assert.AreEqual("x+1", macro.Value);
                        Assert.NotNull(macro.Parameters);
                        Assert.AreEqual(1, macro.Parameters.Count);
                        Assert.AreEqual("x", macro.Parameters[0]);

                        Assert.AreEqual(3, macro.Tokens.Count);
                        Assert.AreEqual("x", macro.Tokens[0].Text);
                        Assert.AreEqual("+", macro.Tokens[1].Text);
                        Assert.AreEqual("1", macro.Tokens[2].Text);
                        Assert.AreEqual(CppTokenKind.Identifier, macro.Tokens[0].Kind);
                        Assert.AreEqual(CppTokenKind.Punctuation, macro.Tokens[1].Kind);
                        Assert.AreEqual(CppTokenKind.Literal, macro.Tokens[2].Kind);
                    }

                    {
                        var macro = compilation.Macros[4];
                        Assert.AreEqual("MACRO4", macro.Name);
                        Assert.AreEqual("(x)", macro.Value);
                        Assert.Null(macro.Parameters);

                        Assert.AreEqual(3, macro.Tokens.Count);
                        Assert.AreEqual("(", macro.Tokens[0].Text);
                        Assert.AreEqual("x", macro.Tokens[1].Text);
                        Assert.AreEqual(")", macro.Tokens[2].Text);
                        Assert.AreEqual(CppTokenKind.Punctuation, macro.Tokens[0].Kind);
                        Assert.AreEqual(CppTokenKind.Identifier, macro.Tokens[1].Kind);
                        Assert.AreEqual(CppTokenKind.Punctuation, macro.Tokens[2].Kind);
                    }
                }
                , GetDefaultOptions().EnableMacros());
        }

        [Test]
        public void TestGlobalVariables()
        {
            ParseAssert(@"
int var0;
int var1;
extern int var2;
const int var3 = 123;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(4, compilation.Fields.Count);

                    {
                        var cppField = compilation.Fields[0];
                        Assert.AreEqual("var0", cppField.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppField.Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppField.Type).Kind);
                        Assert.AreEqual(CppVisibility.Default, cppField.Visibility);
                        Assert.AreEqual(CppStorageQualifier.None, cppField.StorageQualifier);
                    }

                    {
                        var cppField = compilation.Fields[1];
                        Assert.AreEqual("var1", cppField.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppField.Type.TypeKind);
                        Assert.AreEqual(CppVisibility.Default, cppField.Visibility);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppField.Type).Kind);
                        Assert.AreEqual(CppStorageQualifier.None, cppField.StorageQualifier);
                    }

                    {
                        var cppField = compilation.Fields[2];
                        Assert.AreEqual("var2", cppField.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppField.Type.TypeKind);
                        Assert.AreEqual(CppVisibility.Default, cppField.Visibility);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppField.Type).Kind);
                        Assert.AreEqual(CppStorageQualifier.Extern, cppField.StorageQualifier);
                    }

                    {
                        var cppField = compilation.Fields[3];
                        Assert.AreEqual("var3", cppField.Name);
                        Assert.AreEqual(CppTypeKind.Qualified, cppField.Type.TypeKind);
                        Assert.AreEqual(CppTypeQualifier.Const, ((CppQualifiedType)cppField.Type).Qualifier);
                        Assert.NotNull(cppField.DefaultValue);
                        Assert.AreEqual(123, cppField.DefaultValue.Value);
                    }
                }
                , GetDefaultOptions());
        }


        [Test]
        public void TestStructs()
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
                    }

                    {
                        var cppStruct = compilation.Classes[1];
                        Assert.AreEqual("Struct1", cppStruct.Name);
                        Assert.AreEqual(0, cppStruct.Fields.Count);
                        Assert.AreEqual(1, cppStruct.BaseTypes.Count);
                        Assert.True(cppStruct.BaseTypes[0].Type is CppClass);
                        Assert.True(ReferenceEquals(compilation.Classes[0], cppStruct.BaseTypes[0].Type));
                    }

                    {
                        var cppStruct = compilation.Classes[2];
                        Assert.AreEqual("Struct2", cppStruct.Name);
                        Assert.AreEqual(1, cppStruct.Fields.Count);
                        Assert.AreEqual("field0", cppStruct.Fields[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppStruct.Fields[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppStruct.Fields[0].Type).Kind);
                    }

                    {
                        var cppStruct = compilation.Classes[3];
                        Assert.AreEqual(2, cppStruct.Fields.Count);
                        Assert.AreEqual("field0", cppStruct.Fields[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppStruct.Fields[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppStruct.Fields[0].Type).Kind);
                        Assert.AreEqual(CppVisibility.Private, cppStruct.Fields[0].Visibility);

                        Assert.AreEqual("field1", cppStruct.Fields[1].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppStruct.Fields[1].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Float, ((CppPrimitiveType)cppStruct.Fields[1].Type).Kind);
                        Assert.AreEqual(CppVisibility.Public, cppStruct.Fields[1].Visibility);
                    }
                }
                , GetDefaultOptions());
        }


        [Test]
        public void TestEnums()
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
                        Assert.False(cppEnum.IsScoped);
                        Assert.AreEqual("Enum0_item0", cppEnum.Items[0].Name);
                        Assert.AreEqual("Enum0_item1", cppEnum.Items[1].Name);
                        Assert.AreEqual("Enum0_item2", cppEnum.Items[2].Name);
                        Assert.AreEqual(0, cppEnum.Items[0].Value);
                        Assert.AreEqual(1, cppEnum.Items[1].Value);
                        Assert.AreEqual(2, cppEnum.Items[2].Value);
                    }

                    {
                        var cppEnum = compilation.Enums[1];
                        Assert.AreEqual("Enum1", cppEnum.Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppEnum.IntegerType.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppEnum.IntegerType).Kind);
                        Assert.AreEqual(3, cppEnum.Items.Count);
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
                        Assert.True(cppEnum.IsScoped);
                        Assert.AreEqual("item0", cppEnum.Items[0].Name);
                        Assert.AreEqual("item1", cppEnum.Items[1].Name);
                        Assert.AreEqual("item2", cppEnum.Items[2].Name);
                        Assert.AreEqual(3, cppEnum.Items[0].Value);
                        Assert.AreEqual(4, cppEnum.Items[1].Value);
                        Assert.AreEqual(5, cppEnum.Items[2].Value);
                    }
                }
                , GetDefaultOptions());
        }
    }
}