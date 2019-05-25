using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestTypedefs : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
typedef char Type_char;
typedef unsigned char Type_unsigned_char;

typedef short Type_short;
typedef unsigned short Type_unsigned_short;

typedef int Type_int;
typedef unsigned int Type_unsigned_int;

typedef long long Type_long_long;
typedef unsigned long long Type_unsigned_long_long;

typedef float Type_float;
typedef double Type_double;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(10, compilation.Typedefs.Count);

                    var primitives = new CppPrimitiveType[]
                    {
                        CppPrimitiveType.Char,
                        CppPrimitiveType.UnsignedChar,

                        CppPrimitiveType.Short,
                        CppPrimitiveType.UnsignedShort,

                        CppPrimitiveType.Int,
                        CppPrimitiveType.UnsignedInt,

                        CppPrimitiveType.LongLong,
                        CppPrimitiveType.UnsignedLongLong,

                        CppPrimitiveType.Float,
                        CppPrimitiveType.Double,
                    };


                    for (int i = 0; i < primitives.Length; i++)
                    {
                        var typedef = compilation.Typedefs[i];
                        var expectedType = primitives[i];
                        Assert.AreEqual(expectedType, typedef.Type);
                        Assert.AreEqual("Type_" + expectedType.ToString().Replace(" ", "_") , typedef.Name);
                    }
                }
                , GetDefaultOptions());
        }
    }
}