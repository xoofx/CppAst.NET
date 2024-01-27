// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Linq;
using NUnit.Framework;
using static CppAst.CppTemplateArgument;

namespace CppAst.Tests
{
    public class TestTypes : InlineTestBase
    {
        [Test]
        public void TestSimple()
        {
            ParseAssert(@"
typedef int& t0; // reference type
typedef const float t1;
char* f0; // pointer type
const int f1 = 5; // qualified type
int f2[5]; // array type
void (*f3)(int arg1, float arg2); // function type
t1* f4;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(5, compilation.Fields.Count);
                    Assert.AreEqual(2, compilation.Typedefs.Count);

                    var types = new CppType[]
                    {
                        new CppReferenceType(CppPrimitiveType.Int) ,
                        new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Float),

                        new CppPointerType(CppPrimitiveType.Char),
                        new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Int),
                        new CppArrayType(CppPrimitiveType.Int, 5),
                        new CppPointerType(new CppFunctionType(CppPrimitiveType.Void)
                        {
                            Parameters =
                            {
                                new CppParameter(CppPrimitiveType.Int, "a"),
                                new CppParameter(CppPrimitiveType.Float, "b"),
                            }
                        }) { SizeOf = IntPtr.Size },
                        new CppPointerType(new CppQualifiedType(CppTypeQualifier.Const, CppPrimitiveType.Float))
                    };

                    var canonicalTypes = compilation.Typedefs.Select(x => x.GetCanonicalType()).Concat(compilation.Fields.Select(x => x.Type.GetCanonicalType())).ToList();
                    Assert.AreEqual(types, canonicalTypes);
                    Assert.AreEqual(types.Select(x => x.SizeOf), canonicalTypes.Select(x => x.SizeOf));
                }
            );
        }

        [Test]
        public void TestTemplateParameters()
        {
            ParseAssert(@"
template <typename T, typename U>
struct TemplateStruct
{
    T field0;
    U field1;
};

struct Struct2
{
};

::TemplateStruct<int, Struct2> exposed;
TemplateStruct<int, Struct2> unexposed;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(2, compilation.Fields.Count);

                    var exposed = compilation.Fields[0].Type as CppClass;
                    Assert.AreEqual("TemplateStruct", exposed.Name);
                    Assert.AreEqual(2, exposed.TemplateParameters.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, exposed.TemplateArguments[0]?.ArgKind);
                    Assert.AreEqual(CppPrimitiveKind.Int, (exposed.TemplateArguments[0]?.ArgAsType as CppPrimitiveType).Kind);
                    Assert.AreEqual("Struct2", (exposed.TemplateArguments[1].ArgAsType as CppClass)?.Name);

                    var specialized = exposed.PrimaryTemplate;
                    Assert.AreEqual("TemplateStruct", specialized.Name);
                    Assert.AreEqual(2, specialized.Fields.Count);
                    Assert.AreEqual("field0", specialized.Fields[0].Name);
                    Assert.AreEqual("T", specialized.Fields[0].Type.GetDisplayName());
                    Assert.AreEqual("field1", specialized.Fields[1].Name);
                    Assert.AreEqual("U", specialized.Fields[1].Type.GetDisplayName());

                    var unexposed = compilation.Fields[1].Type as CppClass;
                    Assert.AreEqual("TemplateStruct", unexposed.Name);
                    Assert.AreEqual(2, unexposed.TemplateParameters.Count);
                    Assert.AreEqual(CppTemplateArgumentKind.AsType, unexposed.TemplateArguments[0]?.ArgKind);
                    Assert.AreEqual(CppPrimitiveKind.Int, (exposed.TemplateArguments[0]?.ArgAsType as CppPrimitiveType).Kind);
                    Assert.AreEqual("Struct2", (unexposed.TemplateArguments[1].ArgAsType as CppClass)?.Name);

                    Assert.AreNotEqual(exposed.GetHashCode(), specialized.GetHashCode());
                    Assert.AreEqual(exposed.GetHashCode(), unexposed.GetHashCode());
                }
            );
        }

        [Test]
        public void TestTemplateInheritance()
        {
            ParseAssert(@"
template <typename T>
class BaseTemplate
{
};

class Derived : public ::BaseTemplate<::Derived>
{
};
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Classes.Count);

                    var baseTemplate = compilation.Classes[0];
                    var derived = compilation.Classes[1];
                    var baseClassSpecialized = compilation.Classes[2];

                    Assert.AreEqual("BaseTemplate", baseTemplate.Name);
                    Assert.AreEqual("Derived", derived.Name);
                    Assert.AreEqual("BaseTemplate", baseClassSpecialized.Name);

                    Assert.AreEqual(1, derived.BaseTypes.Count);
                    Assert.AreEqual(baseClassSpecialized, derived.BaseTypes[0].Type);

                    Assert.AreEqual(1, baseClassSpecialized.TemplateParameters.Count);

                    //Here change to argument as a template deduce instance, not as a Template Parameters~~
                    Assert.AreEqual(derived, baseClassSpecialized.TemplateArguments[0].ArgAsType);
                    Assert.AreEqual(baseTemplate, baseClassSpecialized.PrimaryTemplate);
                }
            );
        }


        [Test]
        public void TestTemplatePartialSpecialization()
        {
            ParseAssert(@"
template<typename A, typename B>
struct foo {};

template<typename B>
struct foo<int, B> {};

foo<int, int> foobar;
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    Assert.AreEqual(3, compilation.Classes.Count);
                    Assert.AreEqual(1, compilation.Fields.Count);

                    var baseTemplate = compilation.Classes[0];
                    var fullSpecializedClass = compilation.Classes[1];
                    var partialSpecializedTemplate = compilation.Classes[2];

                    var field = compilation.Fields[0];
                    Assert.AreEqual(field.Name, "foobar");

                    Assert.AreEqual(baseTemplate.TemplateKind, CppAst.CppTemplateKind.TemplateClass);
                    Assert.AreEqual(fullSpecializedClass.TemplateKind, CppAst.CppTemplateKind.TemplateSpecializedClass);
                    Assert.AreEqual(partialSpecializedTemplate.TemplateKind, CppAst.CppTemplateKind.PartialTemplateClass);

                    //Need be a specialized for partial template here
                    Assert.AreEqual(fullSpecializedClass.PrimaryTemplate, partialSpecializedTemplate);

                    //Need be a full specialized class for this field
                    Assert.AreEqual(field.Type, fullSpecializedClass);

                    Assert.AreEqual(partialSpecializedTemplate.TemplateArguments.Count, 2);
                    //The first argument is integer now
                    Assert.AreEqual(partialSpecializedTemplate.TemplateArguments[0].ArgString, "int");
                    //The second argument is not a specialized argument, we do not specialized a `B` template parameter here(partial specialized template)
                    Assert.AreEqual(partialSpecializedTemplate.TemplateArguments[1].IsSpecializedArgument, false);

                    //The field use type is a full specialized type here~, so we can have two `int` template parmerater here
                    //It's a not template or partial template class, so we can instantiate it, see `foo<int, int> foobar;` before.
                    Assert.AreEqual(fullSpecializedClass.TemplateArguments.Count, 2);
                    //The first argument is integer now
                    Assert.AreEqual(fullSpecializedClass.TemplateArguments[0].ArgString, "int");
                    //The second argument is not a specialized argument
                    Assert.AreEqual(fullSpecializedClass.TemplateArguments[1].ArgString, "int");
                }
            );
        }

        [Test]
        public void TestClassPrototype()
        {
            ParseAssert(@"
namespace ns1 {
class TmpClass;
}

namespace ns2 {
const ns1::TmpClass* tmpClass1;
volatile ns1::TmpClass* tmpClass2;
}

namespace ns1 {
class TmpClass {
};
}
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var tmpClass1 = compilation.Namespaces[1].Fields[0];
                    var tmpClass2 = compilation.Namespaces[1].Fields[1];

                    var hoge = tmpClass1.Type.GetDisplayName();
                    var hoge2 = tmpClass2.Type.GetDisplayName();
                    Assert.AreEqual("const TmpClass*", tmpClass1.Type.GetDisplayName());
                    Assert.AreEqual("volatile TmpClass*", tmpClass2.Type.GetDisplayName());
                }
            );
        }

        [Test]
        public void TestStdVariadicTemplate()
        {
            ParseAssert(@"
#include <tuple>

void function1(std::tuple<double, double, double> input);
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(compilation.Functions.Count, 1);
                var function1 = compilation.Functions[0];
                Assert.AreEqual(function1.Parameters.Count, 1);
                var parameter = function1.Parameters[0];
                Assert.True(parameter.Type is CppClass);
                Assert.AreEqual(parameter.Name, "input");
                var parameterClass = parameter.Type as CppClass;
                Assert.AreEqual(parameterClass.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
                Assert.AreEqual(parameterClass.Name, "tuple");
                Assert.AreEqual(parameterClass.TemplateArguments.Count, 3);
                Assert.AreEqual(parameterClass.TemplateArguments[0].ArgAsType.FullName, "double");
                Assert.AreEqual(parameterClass.TemplateArguments[1].ArgAsType.FullName, "double");
                Assert.AreEqual(parameterClass.TemplateArguments[2].ArgAsType.FullName, "double");
            });
        }

        [Test]
        public void TestOptional()
        {
            var options = new CppParserOptions();
            options.AdditionalArguments.Add("-std=c++17");
            ParseAssert(@"
#include <optional>

void function1(const std::optional<double>& input);
",
        compilation =>
        {
            Assert.False(compilation.HasErrors);
        }, options);
        }

        [Test]
        public void TestTemplateParameterType()
        {
            ParseAssert(@"
template <typename T>
void function1(T input);

template <typename T>
class TemplatedClass
{
public:
  TemplatedClass(T value);
};",
        compilation =>
        {
            Assert.False(compilation.HasErrors);
            Assert.AreEqual(compilation.Classes.Count, 1);
            Assert.AreEqual(compilation.Functions.Count, 1);

            var function1 = compilation.Functions[0];
            Assert.AreEqual(function1.Parameters.Count, 1);
            var input = function1.Parameters[0];
            Assert.AreEqual(input.Name, "input");
            Assert.AreEqual(input.Type.GetDisplayName(), "T");
            Assert.AreEqual(input.Type.TypeKind, CppTypeKind.TemplateParameterType);

            var templatedClass = compilation.Classes[0];
            Assert.AreEqual(templatedClass.Name, "TemplatedClass");
            Assert.AreEqual(templatedClass.Constructors.Count, 1);
            var constructor = templatedClass.Constructors[0];
            Assert.AreEqual(constructor.Parameters.Count, 1);
            var value = constructor.Parameters[0];
            Assert.AreEqual(value.Name, "value");
            Assert.AreEqual(value.Type.GetDisplayName(), "T");
            Assert.AreEqual(value.Type.TypeKind, CppTypeKind.TemplateParameterType);
        });
        }

        [Test]
        public void TestPrimaryTemplateType()
        {
            ParseAssert(@"
template <typename T, unsigned N>
class TemplatedClass
{
public:
  TemplatedClass(T value);

  TemplatedClass<T, N> operator+(const TemplatedClass<T, N>& other) const;

  TemplatedClass operator-(const TemplatedClass& other) const;

private:
    T value_;
};

template <typename T>
class TemplatedClass2D : public virtual TemplatedClass<T, 2>
{
public:
  TemplatedClass2D(T value);

  TemplatedClass2D<T> operator+(const TemplatedClass2D<T>& other) const;

  TemplatedClass2D operator-(const TemplatedClass2D& other) const;
};

",
            compilation =>
            {
                Assert.False(compilation.HasErrors);
                Assert.AreEqual(compilation.Classes.Count, 2);
                {
                    var templatedClass = compilation.Classes[0];
                    Assert.AreEqual(templatedClass.Name, "TemplatedClass");
                    Assert.AreEqual(templatedClass.Constructors.Count, 1);
                    Assert.AreEqual(templatedClass.Functions.Count, 2);
                    Assert.AreEqual(templatedClass.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(templatedClass.TemplateParameters.Count, 2);
                    Assert.AreEqual(templatedClass.TemplateArguments.Count, 2);

                    var T = templatedClass.TemplateParameters[0];
                    Assert.True(T is CppTemplateParameterType);
                    Assert.AreEqual((T as CppTemplateParameterType).Name, "T");

                    var Targ = templatedClass.TemplateArguments[0];
                    Assert.True(Targ is CppTemplateArgument);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsType);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgAsType.FullName, "T");
                    Assert.AreEqual((Targ as CppTemplateArgument).SourceParam.FullName, "T");

                    var N = templatedClass.TemplateParameters[1];
                    Assert.True(N is CppTemplateParameterNonType);
                    Assert.AreEqual((N as CppTemplateParameterNonType).Name, "N");
                    Assert.AreEqual((N as CppTemplateParameterNonType).NoneTemplateType.GetDisplayName(), "unsigned int");

                    var Narg = templatedClass.TemplateArguments[1];
                    Assert.True(Narg is CppTemplateArgument);
                    Assert.AreEqual((Narg as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsExpression);
                    Assert.AreEqual((Narg as CppTemplateArgument).ArgAsExpression.ToString(), "2");
                    Assert.AreEqual((Narg as CppTemplateArgument).SourceParam.FullName, "unsigned int N");

                    var operatorPlus = templatedClass.Functions[0];
                    Assert.AreEqual(operatorPlus.Name, "operator+");

                    var operatorPlusReturnType = operatorPlus.ReturnType as CppClass;
                    Assert.AreEqual(operatorPlusReturnType.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(operatorPlusReturnType.TemplateParameters.Count, 2);
                    Assert.AreEqual(operatorPlusReturnType.GetDisplayName(), "TemplatedClass");

                    Assert.AreEqual(operatorPlus.Parameters.Count, 1);
                    var other = operatorPlus.Parameters[0];
                    Assert.AreEqual(other.Name, "other");
                    Assert.AreEqual(other.Type.TypeKind, CppTypeKind.Reference); // ref
                    Assert.AreEqual((other.Type as CppReferenceType).ElementType.TypeKind, CppTypeKind.Qualified); // const
                    var otherType = ((other.Type as CppReferenceType).ElementType as CppQualifiedType).ElementType as CppClass;
                    Assert.AreEqual(otherType.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(otherType.TemplateParameters.Count, 2);
                    Assert.AreEqual(otherType.TemplateArguments.Count, 2);
                    Assert.AreEqual(otherType.Name, "TemplatedClass");

                    T = otherType.TemplateParameters[0];
                    Assert.True(T is CppTemplateParameterType);
                    Assert.AreEqual((T as CppTemplateParameterType).Name, "T");

                    Targ = otherType.TemplateArguments[0];
                    Assert.True(Targ is CppTemplateArgument);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsType);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgAsType.FullName, "T");
                    Assert.AreEqual((Targ as CppTemplateArgument).SourceParam.FullName, "T");

                    N = otherType.TemplateParameters[1];
                    Assert.True(N is CppTemplateParameterNonType);
                    Assert.AreEqual((N as CppTemplateParameterNonType).Name, "N");
                    Assert.AreEqual((N as CppTemplateParameterNonType).NoneTemplateType.GetDisplayName(), "unsigned int");

                    Narg = otherType.TemplateArguments[1];
                    Assert.True(Narg is CppTemplateArgument);
                    Assert.AreEqual((Narg as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsExpression);
                    Assert.AreEqual((Narg as CppTemplateArgument).ArgAsExpression.ToString(), "2");
                    Assert.AreEqual((Narg as CppTemplateArgument).SourceParam.FullName, "unsigned int N");

                    var operatorMinus = templatedClass.Functions[1];
                    Assert.AreEqual(operatorMinus.Name, "operator-");

                    var operatorMinusReturnType = operatorMinus.ReturnType as CppClass;
                    Assert.AreEqual(operatorMinusReturnType.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(operatorMinusReturnType.TemplateParameters.Count, 2);
                    Assert.AreEqual(operatorMinusReturnType.GetDisplayName(), "TemplatedClass");

                    Assert.AreEqual(operatorMinus.Parameters.Count, 1);
                    var other2 = operatorMinus.Parameters[0];
                    Assert.AreEqual(other2.Name, "other");
                    Assert.AreEqual(other2.Type.TypeKind, CppTypeKind.Reference); // ref
                    Assert.AreEqual((other2.Type as CppReferenceType).ElementType.TypeKind, CppTypeKind.Qualified); // const
                    var other2Type = ((other2.Type as CppReferenceType).ElementType as CppQualifiedType).ElementType as CppClass;
                    Assert.AreEqual(other2Type.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(other2Type.TemplateParameters.Count, 2);
                    Assert.AreEqual(otherType.TemplateArguments.Count, 2);
                    Assert.AreEqual(other2Type.Name, "TemplatedClass");

                    T = other2Type.TemplateParameters[0];
                    Assert.True(T is CppTemplateParameterType);
                    Assert.AreEqual((T as CppTemplateParameterType).Name, "T");

                    Targ = other2Type.TemplateArguments[0];
                    Assert.True(Targ is CppTemplateArgument);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsType);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgAsType.FullName, "T");
                    Assert.AreEqual((Targ as CppTemplateArgument).SourceParam.FullName, "T");

                    N = other2Type.TemplateParameters[1];
                    Assert.True(N is CppTemplateParameterNonType);
                    Assert.AreEqual((N as CppTemplateParameterNonType).Name, "N");
                    Assert.AreEqual((N as CppTemplateParameterNonType).NoneTemplateType.GetDisplayName(), "unsigned int");

                    Narg = other2Type.TemplateArguments[1];
                    Assert.True(Narg is CppTemplateArgument);
                    Assert.AreEqual((Narg as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsExpression);
                    Assert.AreEqual((Narg as CppTemplateArgument).ArgAsExpression.ToString(), "2");
                    Assert.AreEqual((Narg as CppTemplateArgument).SourceParam.FullName, "unsigned int N");
                }

                {
                    var templatedClass2D = compilation.Classes[1];
                    Assert.AreEqual(templatedClass2D.Name, "TemplatedClass2D");
                    Assert.AreEqual(templatedClass2D.Constructors.Count, 1);
                    Assert.AreEqual(templatedClass2D.Functions.Count, 2);
                    Assert.AreEqual(templatedClass2D.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(templatedClass2D.TemplateParameters.Count, 1);
                    Assert.AreEqual(templatedClass2D.TemplateArguments.Count, 1);

                    var T = templatedClass2D.TemplateParameters[0];
                    Assert.True(T is CppTemplateParameterType);
                    Assert.AreEqual((T as CppTemplateParameterType).Name, "T");

                    var Targ = templatedClass2D.TemplateArguments[0];
                    Assert.True(Targ is CppTemplateArgument);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsType);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgAsType.FullName, "T");
                    Assert.AreEqual((Targ as CppTemplateArgument).SourceParam.FullName, "T");

                    var baseClass = templatedClass2D.BaseTypes[0].Type as CppClass;
                    Assert.AreEqual(baseClass, compilation.Classes[0]);

                    var operatorPlus = templatedClass2D.Functions[0];
                    Assert.AreEqual(operatorPlus.Name, "operator+");

                    Assert.AreEqual(operatorPlus.Parameters.Count, 1);
                    var other = operatorPlus.Parameters[0];
                    Assert.AreEqual(other.Name, "other");
                    Assert.AreEqual(other.Type.TypeKind, CppTypeKind.Reference); // ref
                    Assert.AreEqual((other.Type as CppReferenceType).ElementType.TypeKind, CppTypeKind.Qualified); // const
                    var otherType = ((other.Type as CppReferenceType).ElementType as CppQualifiedType).ElementType as CppClass;
                    Assert.AreEqual(otherType.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(otherType.TemplateParameters.Count, 1);
                    Assert.AreEqual(otherType.TemplateArguments.Count, 1);
                    Assert.AreEqual(otherType.Name, "TemplatedClass2D");

                    T = otherType.TemplateParameters[0];
                    Assert.True(T is CppTemplateParameterType);
                    Assert.AreEqual((T as CppTemplateParameterType).Name, "T");

                    Targ = templatedClass2D.TemplateArguments[0];
                    Assert.True(Targ is CppTemplateArgument);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsType);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgAsType.FullName, "T");
                    Assert.AreEqual((Targ as CppTemplateArgument).SourceParam.FullName, "T");

                    var operatorMinus = templatedClass2D.Functions[1];
                    Assert.AreEqual(operatorMinus.Name, "operator-");

                    Assert.AreEqual(operatorMinus.Parameters.Count, 1);
                    var other2 = operatorMinus.Parameters[0];
                    Assert.AreEqual(other2.Name, "other");
                    Assert.AreEqual(other2.Type.TypeKind, CppTypeKind.Reference); // ref
                    Assert.AreEqual((other2.Type as CppReferenceType).ElementType.TypeKind, CppTypeKind.Qualified); // const
                    var other2Type = ((other2.Type as CppReferenceType).ElementType as CppQualifiedType).ElementType as CppClass;
                    Assert.AreEqual(other2Type.TemplateKind, CppTemplateKind.TemplateClass);
                    Assert.AreEqual(other2Type.TemplateParameters.Count, 1);
                    Assert.AreEqual(otherType.TemplateArguments.Count, 1);
                    Assert.AreEqual(other2Type.Name, "TemplatedClass2D");

                    T = other2Type.TemplateParameters[0];
                    Assert.True(T is CppTemplateParameterType);
                    Assert.AreEqual((T as CppTemplateParameterType).Name, "T");

                    Targ = templatedClass2D.TemplateArguments[0];
                    Assert.True(Targ is CppTemplateArgument);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgKind, CppTemplateArgumentKind.AsType);
                    Assert.AreEqual((Targ as CppTemplateArgument).ArgAsType.FullName, "T");
                    Assert.AreEqual((Targ as CppTemplateArgument).SourceParam.FullName, "T");
                }
            });


        }

        [Test]
        public void TestTemplateTypeDef()
        {
            ParseAssert(@"
template <typename T>
class TemplatedClass
{
public:
  TemplatedClass(T value)
      : value_{value}
  {
  }

  [[nodiscard]] T Value() const noexcept { return value_; }

  void Value(T value) { value_ = value; }

private:
  T value_ {};
};

using TemplatedClassDouble = TemplatedClass<double>;
using TemplatedClassInt = TemplatedClass<int>;

",
        compilation =>
        {
            Assert.False(compilation.HasErrors);
            Assert.AreEqual(compilation.Classes.Count, 3);
            Assert.AreEqual(compilation.Typedefs.Count, 2);

            var primaryTemplate = compilation.Classes[0];
            var specializedTemplate1 = compilation.Classes[1];
            var specializedTemplate2 = compilation.Classes[2];

            Assert.AreEqual(primaryTemplate.Name, "TemplatedClass");
            Assert.AreEqual(specializedTemplate1.Name, "TemplatedClass");
            Assert.AreEqual(specializedTemplate1.FullName, "TemplatedClass<double>");
            Assert.AreEqual(specializedTemplate2.Name, "TemplatedClass");
            Assert.AreEqual(specializedTemplate2.FullName, "TemplatedClass<int>");

            Assert.AreEqual(primaryTemplate.TemplateKind, CppTemplateKind.TemplateClass);
            Assert.AreEqual(primaryTemplate.TemplateParameters.Count, 1);

            // Specialized templates should contain the information of their primary template.
            Assert.AreEqual(primaryTemplate.Constructors.Count, specializedTemplate1.PrimaryTemplate.Constructors.Count);
            Assert.AreEqual(primaryTemplate.Constructors.Count, specializedTemplate2.PrimaryTemplate.Constructors.Count);
            Assert.AreEqual(primaryTemplate.Functions.Count, specializedTemplate1.PrimaryTemplate.Functions.Count);
            Assert.AreEqual(primaryTemplate.Functions.Count, specializedTemplate2.PrimaryTemplate.Functions.Count);

            Assert.AreEqual(specializedTemplate1.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
            Assert.AreEqual(specializedTemplate1.TemplateArguments.Count, 1);
            Assert.AreEqual(specializedTemplate1.TemplateArguments[0].ArgAsType.FullName, "double");

            Assert.AreEqual(specializedTemplate2.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
            Assert.AreEqual(specializedTemplate2.TemplateArguments.Count, 1);
            Assert.AreEqual(specializedTemplate2.TemplateArguments[0].ArgAsType.FullName, "int");
        });
        }

        [Test]
        public void TestNestedTemplate()
        {
            ParseAssert(@"
#include <array>
#include <vector>

template <typename T, std::size_t N>
class Vector {
    std::array<T, N> elements;

public:
    // Constructor
    Vector(const std::array<T, N>& elems) : elements(elems) {}

    // Multiply vectors
    Vector operator*(const Vector& other) const {
        std::array<T, N> result;
        VectorMultiplier<T, N>::multiply(result, this->elements, other.elements);
        return Vector(result);
    }

    // Access elements
    T& operator[](int idx) { return elements[idx]; }
    const T& operator[](int idx) const { return elements[idx]; }
};

using Vect2 = Vector<double, 2>;
using Vect2i = Vector<int, 2>;
using Vect3 = Vector<double, 3>;

void function1(std::vector<Vect2> input);

", compilation =>
        {
            Assert.False(compilation.HasErrors);
            Assert.AreEqual(compilation.Classes.Count, 4);
            Assert.AreEqual(compilation.Typedefs.Count, 3);
            Assert.AreEqual(compilation.Functions.Count, 1);

            var baseVector = compilation.Classes[0];
            var vect2 = compilation.Classes[1];
            var vect2i = compilation.Classes[2];
            var vect3 = compilation.Classes[3];

            Assert.AreEqual(baseVector.Name, "Vector");
            Assert.AreEqual(baseVector.Name, vect2.Name);
            Assert.AreEqual(baseVector.Name, vect2i.Name);
            Assert.AreEqual(baseVector.Name, vect3.Name);

            Assert.AreEqual(vect2.FullName, "Vector<double, 2>");
            Assert.AreEqual(vect2i.FullName, "Vector<int, 2>");
            Assert.AreEqual(vect3.FullName, "Vector<double, 3>");

            Assert.AreEqual(baseVector.TemplateKind, CppTemplateKind.TemplateClass);
            Assert.AreEqual(baseVector.TemplateParameters.Count, 2);

            Assert.AreEqual(vect2.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
            Assert.AreEqual(vect2.TemplateArguments.Count, 2);
            Assert.AreEqual(vect2.TemplateArguments[0].ArgAsType.FullName, "double");
            Assert.AreEqual(vect2.TemplateArguments[1].ArgAsInteger, 2);

            Assert.AreEqual(vect2i.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
            Assert.AreEqual(vect2i.TemplateArguments.Count, 2);
            Assert.AreEqual(vect2i.TemplateArguments[0].ArgAsType.FullName, "int");
            Assert.AreEqual(vect2i.TemplateArguments[1].ArgAsInteger, 2);

            Assert.AreEqual(vect3.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
            Assert.AreEqual(vect3.TemplateArguments.Count, 2);
            Assert.AreEqual(vect3.TemplateArguments[0].ArgAsType.FullName, "double");
            Assert.AreEqual(vect3.TemplateArguments[1].ArgAsInteger, 3);


            var function1 = compilation.Functions[0];
            Assert.AreEqual(function1.Parameters.Count, 1);
            var parameter = function1.Parameters[0];
            Assert.True(parameter.Type is CppClass);
            Assert.AreEqual(parameter.Name, "input");
            var parameterClass = parameter.Type as CppClass;
            Assert.AreEqual(parameterClass.TemplateKind, CppTemplateKind.TemplateSpecializedClass);
            // The template argument is also a templated type
            Assert.AreEqual(parameterClass.TemplateArguments.Count, 2);
            Assert.True(parameterClass.TemplateArguments[0].ArgAsType is CppClass);
            var argAsClass = parameterClass.TemplateArguments[0].ArgAsType as CppClass;
            // The argument should be just the same as Vect2
            Assert.AreEqual(argAsClass.FullName, vect2.FullName);
            Assert.AreEqual(argAsClass.TemplateKind, vect2.TemplateKind);
            Assert.AreEqual(argAsClass.TemplateArguments.Count, vect2.TemplateArguments.Count);
            Assert.AreEqual(argAsClass.TemplateArguments[0].ArgAsType.FullName,
                            vect2.TemplateArguments[0].ArgAsType.FullName);
            Assert.AreEqual(argAsClass.TemplateArguments[1].ArgAsInteger,
                            vect2.TemplateArguments[1].ArgAsInteger);

        });
        }
    }
}