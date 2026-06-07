using System.Linq;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestTypedefs : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
typedef void Type_void;

typedef bool Type_bool;

typedef wchar_t Type_wchar_t;

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

                    Assert.AreEqual(13, compilation.Typedefs.Count);

                    var primitives = new CppPrimitiveType[]
                    {
                        CppPrimitiveType.Void,

                        CppPrimitiveType.Bool,

                        CppPrimitiveType.WChar,

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
                        Assert.AreEqual(expectedType, typedef.ElementType);
                        Assert.AreEqual("Type_" + expectedType.ToString().Replace(" ", "_"), typedef.Name);
                    }
                }
            );
        }

        [Test]
        public void TestSquash()
        {
            var text = @"
// Test typedef collapsing
typedef struct {
    int field0;
} MyStruct;
";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);
                    Assert.AreEqual("MyStruct", compilation.Classes[0].Name);

                    var cppStruct = compilation.FindByName<CppClass>("MyStruct");
                    Assert.AreEqual(compilation.Classes[0], cppStruct);
                }
            );


            ParseAssert(@text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Classes.Count);
                    Assert.AreEqual(1, compilation.Typedefs.Count);
                    Assert.AreEqual("MyStruct", compilation.Classes[0].Name);
                    Assert.AreEqual("MyStruct", compilation.Typedefs[0].Name);
                },
                new CppParserOptions() { AutoSquashTypedef = false }
            );

        }

        [Test]
        public void TestCAdvancedTypedefs()
        {
            ParseAssert(@"
typedef int IntArray[4];
typedef int (*BinaryOp)(int lhs, int rhs);
typedef struct Named Named;
struct Named
{
    int value;
};
typedef enum Color
{
    Color_Red = 1,
    Color_Blue = 2,
} Color;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var intArray = compilation.Typedefs.Single(x => x.Name == "IntArray");
                    Assert.IsInstanceOf<CppArrayType>(intArray.ElementType);
                    Assert.AreEqual(4, ((CppArrayType)intArray.ElementType).Size);

                    var binaryOp = compilation.Typedefs.Single(x => x.Name == "BinaryOp");
                    Assert.IsInstanceOf<CppPointerType>(binaryOp.ElementType);
                    var pointer = (CppPointerType)binaryOp.ElementType;
                    Assert.IsInstanceOf<CppFunctionType>(pointer.ElementType);
                    var function = (CppFunctionType)pointer.ElementType;
                    Assert.AreEqual(CppPrimitiveType.Int, function.ReturnType);
                    Assert.AreEqual(new[] { "lhs", "rhs" }, function.Parameters.Select(x => x.Name).ToArray());

                    var named = compilation.Classes.Single(x => x.Name == "Named");
                    Assert.AreEqual(CppClassKind.Struct, named.ClassKind);
                    Assert.AreEqual(named, compilation.Typedefs.Single(x => x.Name == "Named").ElementType);

                    var color = compilation.Enums.Single(x => x.Name == "Color");
                    Assert.AreEqual(new[] { "Color_Red", "Color_Blue" }, color.Items.Select(x => x.Name).ToArray());
                    Assert.AreEqual(color, compilation.Typedefs.Single(x => x.Name == "Color").ElementType);
                },
                new CppParserOptions
                {
                    ParserKind = CppParserKind.C,
                    AdditionalArguments = { "-std=c11" },
                    AutoSquashTypedef = false,
                });
        }
    }
}