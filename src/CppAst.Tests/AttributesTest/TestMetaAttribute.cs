using System;

namespace CppAst.Tests
{

    public class TestMetaAttribute : InlineTestBase
    {
        [Test]
        public void TestNamespaceMetaAttribute()
        {
            ParseAssert( @"

#if !defined(__cppast)
#define __cppast(...)
#endif

namespace __cppast(script, is_browsable=true, desc=""a namespace test"") TestNs{

}

", compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    //annotate attribute support on namespace
                    var ns = compilation.Namespaces[0];

                    Assert.IsTrue(ns.MetaAttributes.QueryArgumentAsBool("script", false));
                    Assert.IsFalse(!ns.MetaAttributes.QueryArgumentAsBool("is_browsable", false));
                    Assert.AreEqual("a namespace test", ns.MetaAttributes.QueryArgumentAsString("desc", ""));

                }
            );
        }
        
        [Test]
        public void TestClassMetaAttribute()
        {
            
            ParseAssert( @"

#if !defined(__cppast)
#define __cppast(...)
#endif

class __cppast(script, is_browsable=true, desc=""a class"") TestClass
{
  public:
    __cppast(desc=""a member function"")
    __cppast(desc2=""a member function 2"")
    void TestMemberFunc();

    __cppast(desc=""a member field"")
    __cppast(desc2=""a member field 2"")
    int X;
};

", compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var cppClass = compilation.Classes[0];
                    Assert.IsTrue(cppClass.MetaAttributes.QueryArgumentAsBool("script", false));
                    Assert.IsFalse(!cppClass.MetaAttributes.QueryArgumentAsBool("is_browsable", false));
                    Assert.AreEqual("a class", cppClass.MetaAttributes.QueryArgumentAsString("desc", ""));
                    
                    Assert.AreEqual(1, cppClass.Functions.Count);
                    Assert.AreEqual("a member function", cppClass.Functions[0].MetaAttributes.QueryArgumentAsString("desc", ""));
                    Assert.AreEqual("a member function 2", cppClass.Functions[0].MetaAttributes.QueryArgumentAsString("desc2", ""));
                    
                    Assert.AreEqual(1, cppClass.Fields.Count);
                    Assert.AreEqual("a member field", cppClass.Fields[0].MetaAttributes.QueryArgumentAsString("desc", ""));
                    Assert.AreEqual("a member field 2", cppClass.Fields[0].MetaAttributes.QueryArgumentAsString("desc2", ""));

                }
            );
        }
        
        [Test]
        public void TestTemplateMetaAttribute()
        {
            
            ParseAssert( @"

#if !defined(__cppast)
#define __cppast(...)
#endif

template <typename T>
class TestTemplateClass
{
  public:

    __cppast(desc=""a template member field"")
    T X;
};

using IntClass __cppast(desc=""a template class for int"") = TestTemplateClass<int>;
using DoubleClass __cppast(desc=""a template class for double"") = TestTemplateClass<double>;

typedef TestTemplateClass<float> __cppast(desc=""a template class for float"") FloatClass;
", compilation =>
                {
                    Assert.False(compilation.HasErrors);

                    var templateClass = compilation.Classes[0];
                    Assert.AreEqual("a template member field", templateClass.Fields[0].MetaAttributes.QueryArgumentAsString("desc", ""));
                    
                    var intClass = compilation.Classes[1];
                    var doubleClass = compilation.Classes[2];
                    var floatClass = compilation.Classes[3];
                    Assert.AreEqual("a template class for int", intClass.MetaAttributes.QueryArgumentAsString("desc", ""));
                    Assert.AreEqual("a template class for double", doubleClass.MetaAttributes.QueryArgumentAsString("desc", ""));
                    Assert.AreEqual("a template class for float", floatClass.MetaAttributes.QueryArgumentAsString("desc", ""));
                }
            );
        }
    }
}