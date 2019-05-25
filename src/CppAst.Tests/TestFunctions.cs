using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestFunctions : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
void function0();
int function1(int a, float b);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual("function0", cppFunction.Name);
                        Assert.AreEqual(0, cppFunction.Parameters.Count);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());
                    }

                    {
                        var cppFunction = compilation.Functions[1];
                        Assert.AreEqual("function1", cppFunction.Name);
                        Assert.AreEqual(2, cppFunction.Parameters.Count);
                        Assert.AreEqual("a", cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual("b", cppFunction.Parameters[1].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[1].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Float, ((CppPrimitiveType)cppFunction.Parameters[1].Type).Kind);
                        Assert.AreEqual("int", cppFunction.ReturnType.ToString());
                    }
                }
                , GetDefaultOptions());
        }
    }
}