using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestGlobalVariables : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
int var0;
int var1;
extern int var2;
const int var3 = 123;
const unsigned int var4 = (unsigned int) 125;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(5, compilation.Fields.Count);

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
                        Assert.NotNull(cppField.InitExpression);
                        Assert.AreEqual("123", cppField.InitExpression.ToString());
                    }

                    {
                        var cppField = compilation.Fields[4];
                        Assert.AreEqual("var4", cppField.Name);
                        Assert.AreEqual(CppTypeKind.Qualified, cppField.Type.TypeKind);
                        Assert.AreEqual(CppTypeQualifier.Const, ((CppQualifiedType)cppField.Type).Qualifier);
                        Assert.NotNull(cppField.InitExpression);
                        Assert.AreEqual("(unsigned int)125", cppField.InitExpression.ToString());
                    }
                }
            );
        }
    }
}