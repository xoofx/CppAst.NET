// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Linq;

namespace CppAst.Tests;

public class TestCppClassesAdvanced : InlineTestBase
{
    [Test]
    public void TestClassDefaultsInheritanceNestedMembersOperatorsAndFriends()
    {
        ParseAssert(@"
namespace cpp
{
struct DefaultsStruct
{
    int publicField;
};

class DefaultsClass
{
    int privateField;
};

struct EmptyBase
{
};

class PrivateBase
{
};

class Interface
{
public:
    virtual void Run() = 0;
};

class Derived : public EmptyBase, private PrivateBase, public virtual Interface
{
    enum Kind
    {
        Kind_A = 1,
    };
    typedef int Size;
    class Nested
    {
    public:
        int value;
    };

public:
    Derived() = default;
    explicit Derived(int value) noexcept;
    Derived(const Derived&) = delete;
    ~Derived();
    int operator()(int value) const;
    operator bool() const;
    friend bool operator==(const Derived& lhs, const Derived& rhs);

private:
    int value_;
};

inline Derived::~Derived()
{
}

inline int Derived::operator()(int value) const
{
    return value + value_;
}
}
",
            compilation =>
            {
                Assert.False(compilation.HasErrors);

                var cpp = compilation.Namespaces.Single(x => x.Name == "cpp");
                var defaultsStruct = cpp.Classes.Single(x => x.Name == "DefaultsStruct");
                Assert.AreEqual(CppVisibility.Public, defaultsStruct.Fields.Single(x => x.Name == "publicField").Visibility);

                var defaultsClass = cpp.Classes.Single(x => x.Name == "DefaultsClass");
                Assert.AreEqual(CppVisibility.Private, defaultsClass.Fields.Single(x => x.Name == "privateField").Visibility);

                var derived = cpp.Classes.Single(x => x.Name == "Derived");
                Assert.AreEqual(3, derived.BaseTypes.Count);
                Assert.AreEqual(CppVisibility.Public, derived.BaseTypes[0].Visibility);
                Assert.AreEqual("EmptyBase", derived.BaseTypes[0].Type.GetDisplayName());
                Assert.AreEqual(CppVisibility.Private, derived.BaseTypes[1].Visibility);
                Assert.AreEqual("PrivateBase", derived.BaseTypes[1].Type.GetDisplayName());
                Assert.AreEqual(CppVisibility.Public, derived.BaseTypes[2].Visibility);
                Assert.True(derived.BaseTypes[2].IsVirtual);
                Assert.AreEqual("Interface", derived.BaseTypes[2].Type.GetDisplayName());

                Assert.AreEqual(1, derived.Enums.Count);
                Assert.AreEqual("Kind", derived.Enums[0].Name);
                Assert.AreEqual(1, derived.Typedefs.Count);
                Assert.AreEqual("Size", derived.Typedefs[0].Name);
                Assert.AreEqual(1, derived.Classes.Count);
                Assert.AreEqual("Nested", derived.Classes[0].Name);

                Assert.AreEqual(3, derived.Constructors.Count);
                Assert.True(derived.Constructors[0].Flags.HasFlag(CppFunctionFlags.Defaulted));
                Assert.AreEqual(1, derived.Constructors[1].Parameters.Count);
                Assert.True(derived.Constructors[2].Flags.HasFlag(CppFunctionFlags.Deleted));

                Assert.AreEqual(1, derived.Destructors.Count);
                Assert.True(derived.Destructors[0].Flags.HasFlag(CppFunctionFlags.Destructor));
                Assert.NotNull(derived.Destructors[0].BodySpan);

                var callOperator = derived.Functions.Single(x => x.Name == "operator()");
                Assert.True(callOperator.IsConst);
                Assert.True(callOperator.Flags.HasFlag(CppFunctionFlags.Inline));
                Assert.NotNull(callOperator.BodySpan);

                var conversionOperator = derived.Functions.Single(x => x.Name.Contains("operator bool"));
                Assert.True(conversionOperator.IsConst);
                Assert.AreEqual(CppPrimitiveType.Bool, conversionOperator.ReturnType);

                var friendOperator = cpp.Functions.Single(x => x.Name == "operator==");
                Assert.AreEqual(CppPrimitiveType.Bool, friendOperator.ReturnType);
                Assert.AreEqual(new[] { "lhs", "rhs" }, friendOperator.Parameters.Select(x => x.Name).ToArray());
                Assert.AreEqual("Derived const&", friendOperator.Parameters[0].Type.GetDisplayName());
            },
            new CppParserOptions { AdditionalArguments = { "-std=c++17" }, ParseFunctionBodies = true });
    }
}
