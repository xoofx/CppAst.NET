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
float function2(int);
char function3(char);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(4, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual("function0", cppFunction.Name);
                        Assert.AreEqual(0, cppFunction.Parameters.Count);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());

                        var cppFunction1 = compilation.FindByName<CppFunction>("function0");
                        Assert.AreEqual(cppFunction, cppFunction1);
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

                        var cppFunction1 = compilation.FindByName<CppFunction>("function1");
                        Assert.AreEqual(cppFunction, cppFunction1);
                    }
                    {
                        var cppFunction = compilation.Functions[2];
                        Assert.AreEqual("function2", cppFunction.Name);
                        Assert.AreEqual(1, cppFunction.Parameters.Count);
                        Assert.AreEqual(string.Empty, cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual("float", cppFunction.ReturnType.ToString());

                        var cppFunction1 = compilation.FindByName<CppFunction>("function2");
                        Assert.AreEqual(cppFunction, cppFunction1);
                    }
                    {
                        var cppFunction = compilation.Functions[3];
                        Assert.AreEqual("function3", cppFunction.Name);
                        Assert.AreEqual(1, cppFunction.Parameters.Count);
                        Assert.AreEqual(string.Empty, cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Char, ((CppPrimitiveType)cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.ReturnType.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Char, ((CppPrimitiveType)cppFunction.ReturnType).Kind);
                    }
                }
            );
        }

        [Test]
        public void TestSimpleArm()
        {
             var options = new CppParserOptions();

            options.TargetCpu = CppTargetCpu.ARM64;
            options.TargetCpuSub = string.Empty;
            options.TargetVendor = "arm";
            options.TargetSystem = "linux";
            options.TargetAbi = "aarch64-linux-gnu";
            options.AdditionalArguments.Add("-m64");
            options.AdditionalArguments.Add("-O0");

            ParseAssert(@"
void function0();
int function1(int a, float b);
float function2(int);
char function3(char);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(4, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual("function0", cppFunction.Name);
                        Assert.AreEqual(0, cppFunction.Parameters.Count);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());
                        
                        var cppFunction1 = compilation.FindByName<CppFunction>("function0");
                        Assert.AreEqual(cppFunction, cppFunction1);
                    }

                    {
                        var cppFunction = compilation.Functions[1];
                        Assert.AreEqual("function1", cppFunction.Name);
                        Assert.AreEqual(2, cppFunction.Parameters.Count);
                        Assert.AreEqual("a", cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType) cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual("b", cppFunction.Parameters[1].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[1].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Float, ((CppPrimitiveType) cppFunction.Parameters[1].Type).Kind);
                        Assert.AreEqual("int", cppFunction.ReturnType.ToString());

                        var cppFunction1 = compilation.FindByName<CppFunction>("function1");
                        Assert.AreEqual(cppFunction, cppFunction1);
                    }
                    {
                        var cppFunction = compilation.Functions[2];
                        Assert.AreEqual("function2", cppFunction.Name);
                        Assert.AreEqual(1, cppFunction.Parameters.Count);
                        Assert.AreEqual(string.Empty, cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual("float", cppFunction.ReturnType.ToString());

                        var cppFunction1 = compilation.FindByName<CppFunction>("function2");
                        Assert.AreEqual(cppFunction, cppFunction1);
                    }
                    {
                        var cppFunction = compilation.Functions[3];
                        Assert.AreEqual("function3", cppFunction.Name);
                        Assert.AreEqual(1, cppFunction.Parameters.Count);
                        Assert.AreEqual(string.Empty, cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.UnsignedChar, ((CppPrimitiveType)cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.ReturnType.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.UnsignedChar, ((CppPrimitiveType)cppFunction.ReturnType).Kind);
                    }
                },
                options
            );
        }        
        
        [Test]
        public void TestFunctionPrototype()
        {
            ParseAssert(@"
typedef void (*function0)(int a, float b);
typedef void (*function1)(int, float);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Typedefs.Count);

                    {
                        var cppType = compilation.Typedefs[0].ElementType;
                        Assert.AreEqual(CppTypeKind.Pointer, cppType.TypeKind);
                        var cppPointerType = (CppPointerType)cppType;
                        Assert.AreEqual(CppTypeKind.Function, cppPointerType.ElementType.TypeKind);
                        var cppFunctionType = (CppFunctionType)cppPointerType.ElementType;
                        Assert.AreEqual(2, cppFunctionType.Parameters.Count);

                        Assert.AreEqual("a", cppFunctionType.Parameters[0].Name);
                        Assert.AreEqual(CppPrimitiveType.Int, cppFunctionType.Parameters[0].Type);

                        Assert.AreEqual("b", cppFunctionType.Parameters[1].Name);
                        Assert.AreEqual(CppPrimitiveType.Float, cppFunctionType.Parameters[1].Type);
                    }

                    {
                        var cppType = compilation.Typedefs[1].ElementType;
                        Assert.AreEqual(CppTypeKind.Pointer, cppType.TypeKind);
                        var cppPointerType = (CppPointerType)cppType;
                        Assert.AreEqual(CppTypeKind.Function, cppPointerType.ElementType.TypeKind);
                        var cppFunctionType = (CppFunctionType)cppPointerType.ElementType;
                        Assert.AreEqual(2, cppFunctionType.Parameters.Count);

                        Assert.AreEqual(string.Empty, cppFunctionType.Parameters[0].Name);
                        Assert.AreEqual(CppPrimitiveType.Int, cppFunctionType.Parameters[0].Type);

                        Assert.AreEqual(string.Empty, cppFunctionType.Parameters[1].Name);
                        Assert.AreEqual(CppPrimitiveType.Float, cppFunctionType.Parameters[1].Type);
                    }

                }
            );
        }

        [Test]
        public void TestFunctionFields()
        {
            ParseAssert(@"
typedef struct struct0 {
    void (*function0)(int a, float b);
    void (*function1)(char, int);
} struct0;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var cls = compilation.Classes[0];
                    Assert.AreEqual(2, cls.Fields.Count);

                    {
                        var cppType = cls.Fields[0].Type;
                        Assert.AreEqual(CppTypeKind.Pointer, cppType.TypeKind);
                        var cppPointerType = (CppPointerType)cppType;
                        Assert.AreEqual(CppTypeKind.Function, cppPointerType.ElementType.TypeKind);
                        var cppFunctionType = (CppFunctionType)cppPointerType.ElementType;
                        Assert.AreEqual(2, cppFunctionType.Parameters.Count);

                        Assert.AreEqual("a", cppFunctionType.Parameters[0].Name);
                        Assert.AreEqual(CppPrimitiveType.Int, cppFunctionType.Parameters[0].Type);

                        Assert.AreEqual("b", cppFunctionType.Parameters[1].Name);
                        Assert.AreEqual(CppPrimitiveType.Float, cppFunctionType.Parameters[1].Type);
                    }

                    {
                        var cppType = cls.Fields[1].Type;
                        Assert.AreEqual(CppTypeKind.Pointer, cppType.TypeKind);
                        var cppPointerType = (CppPointerType)cppType;
                        Assert.AreEqual(CppTypeKind.Function, cppPointerType.ElementType.TypeKind);
                        var cppFunctionType = (CppFunctionType)cppPointerType.ElementType;
                        Assert.AreEqual(2, cppFunctionType.Parameters.Count);

                        Assert.AreEqual(string.Empty, cppFunctionType.Parameters[0].Name);
                        Assert.AreEqual(CppPrimitiveType.Char, cppFunctionType.Parameters[0].Type);

                        Assert.AreEqual(string.Empty, cppFunctionType.Parameters[1].Name);
                        Assert.AreEqual(CppPrimitiveType.Int, cppFunctionType.Parameters[1].Type);
                    }

                }
            );
        }


        [Test]
        public void TestFunctionExport()
        {
            var text = @"
#ifdef WIN32
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API __attribute__((visibility(""default"")))
#endif
EXPORT_API int function0();
int function1();
";

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual(1, cppFunction.Attributes.Count);
                        Assert.True(cppFunction.IsPublicExport());
                    }
                    {
                        var cppFunction = compilation.Functions[1];
                        Assert.AreEqual(0, cppFunction.Attributes.Count);
                        Assert.True(cppFunction.IsPublicExport());
                    }
                },
                new CppParserOptions() {  }
            );

            ParseAssert(text,
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual(1, cppFunction.Attributes.Count);
                        Assert.True(cppFunction.IsPublicExport());
                    }
                    {
                        var cppFunction = compilation.Functions[1];
                        Assert.AreEqual(0, cppFunction.Attributes.Count);
                        Assert.True(cppFunction.IsPublicExport());
                    }
                }, new CppParserOptions() { }.ConfigureForWindowsMsvc()
            );
        }

        [Test]
        public void TestFunctionVariadic()
        {
            ParseAssert(@"
void function0();
void function1(...);
void function2(int, ...);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual(0, cppFunction.Parameters.Count);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());
                        Assert.AreEqual(CppFunctionFlags.None, cppFunction.Flags & CppFunctionFlags.Variadic);
                    }

                    {
                        var cppFunction = compilation.Functions[1];
                        Assert.AreEqual(0, cppFunction.Parameters.Count);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());
                        Assert.AreEqual(CppFunctionFlags.Variadic, cppFunction.Flags & CppFunctionFlags.Variadic);
                    }

                    {
                        var cppFunction = compilation.Functions[2];
                        Assert.AreEqual(1, cppFunction.Parameters.Count);
                        Assert.AreEqual(string.Empty, cppFunction.Parameters[0].Name);
                        Assert.AreEqual(CppTypeKind.Primitive, cppFunction.Parameters[0].Type.TypeKind);
                        Assert.AreEqual(CppPrimitiveKind.Int, ((CppPrimitiveType)cppFunction.Parameters[0].Type).Kind);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());
                        Assert.AreEqual(CppFunctionFlags.Variadic, cppFunction.Flags & CppFunctionFlags.Variadic);
                    }
                }
            );
        }



        [Test]
        public void TestFunctionTemplate()
        {
            ParseAssert(@"
template<class T>
void function0(T t);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual(1, cppFunction.Parameters.Count);
                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());
                        Assert.AreEqual(cppFunction.IsFunctionTemplate, true);
                        Assert.AreEqual(cppFunction.TemplateParameters.Count, 1);
                    }

                }
            );
        }


        [Test]
        public void TestFunctionTemplateClass()
        {
            ParseAssert(@"

template<typename T, int S>
class Test
{
public:
  Test(T t) : t_{t} {}
private:
  T t_;
};

static Test<int,2> aa{4};
static Test<double,3> bb{6.0};

void functionZ(int a = 5, double b = 6.5);
void functionY(Test<int,2> a, Test<double,3> b);
void functionX(Test<int,2> a = aa, Test<double,3> b = bb);
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Functions.Count);

                    {
                        Assert.AreEqual(2, compilation.Functions[0].DefaultParamCount);
                        Assert.True(compilation.Functions[0].Parameters[0].InitExpression is CppLiteralExpression);
                        Assert.AreEqual("5", (compilation.Functions[0].Parameters[0].InitExpression as CppLiteralExpression).Value);
                        Assert.True(compilation.Functions[0].Parameters[1].InitExpression is CppLiteralExpression);
                        Assert.AreEqual("6.5", (compilation.Functions[0].Parameters[1].InitExpression as CppLiteralExpression).Value);
                        Assert.AreEqual(0, compilation.Functions[1].DefaultParamCount);
                        Assert.AreEqual(2, compilation.Functions[2].DefaultParamCount);
                        Assert.True(compilation.Functions[2].Parameters[0].InitExpression is CppRawExpression);
                        Assert.AreEqual("aa", (compilation.Functions[2].Parameters[0].InitExpression as CppRawExpression).Text);
                        Assert.True(compilation.Functions[2].Parameters[1].InitExpression is CppRawExpression);
                        Assert.AreEqual("bb", (compilation.Functions[2].Parameters[1].InitExpression as CppRawExpression).Text);
                    }

                }
            );
        }


        [Test]
        public void TestFunctionPointersByParam()
        {
            ParseAssert(@"
void function0(int a, int b, float (*callback)(void*, double));
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(1, compilation.Functions.Count);

                    {
                        var cppFunction = compilation.Functions[0];
                        Assert.AreEqual("function0", cppFunction.Name);
                        Assert.AreEqual(3, cppFunction.Parameters.Count);

                        Assert.IsInstanceOf<CppPointerType>(cppFunction.Parameters[2].Type);
                        var pointerType = (CppPointerType)cppFunction.Parameters[2].Type;
                        Assert.IsInstanceOf<CppFunctionType>(pointerType.ElementType);
                        var functionType = (CppFunctionType)pointerType.ElementType;
                        Assert.AreEqual(2, functionType.Parameters.Count);
                        Assert.AreEqual("float", functionType.ReturnType.ToString());
                        Assert.AreEqual("void *", functionType.Parameters[0].Type.ToString());
                        Assert.AreEqual("double", functionType.Parameters[1].Type.ToString());


                        Assert.AreEqual("void", cppFunction.ReturnType.ToString());

                        var cppFunction1 = compilation.FindByName<CppFunction>("function0");
                        Assert.AreEqual(cppFunction, cppFunction1);
                    }
                }
            );
        }



    }
}