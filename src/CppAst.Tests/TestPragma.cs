using System;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class TestPragma : InlineTestBase
    {
        [Test]
        public void TestPragmaOnce()
        {
            ParseAssert(@"
#include ""test_pragma_root.h""
",
                compilation =>
                {
                    Assert.False(compilation.HasErrors);
                    foreach (var message in compilation.Diagnostics.Messages)
                    {
                        Console.WriteLine(message);
                    }
                    Assert.AreEqual(1, compilation.Classes.Count);
                }
            );
        }
    }
}