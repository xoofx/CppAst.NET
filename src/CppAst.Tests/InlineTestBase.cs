using System;
using System.IO;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class InlineTestBase
    {
        public void ParseAssert(string text, Action<CppCompilation> assertCompilation, CppParserOptions options = null)
        {
            if (assertCompilation == null) throw new ArgumentNullException(nameof(assertCompilation));

            options ??= new CppParserOptions();
            var currentDirectory = Environment.CurrentDirectory;
            var headerFilename = $"{TestContext.CurrentContext.Test.FullName}-{TestContext.CurrentContext.Test.ID}.h";
            var headerFile = Path.Combine(currentDirectory, headerFilename);

            // Parse in memory
            var compilation = CppParser.Parse(text, options, headerFilename);
            foreach (var diagnosticsMessage in compilation.Diagnostics.Messages)
            {
                Console.WriteLine(diagnosticsMessage);
            }

            assertCompilation(compilation);

            // Parse single file from disk
            File.WriteAllText(headerFile, text);
            compilation = CppParser.ParseFile(headerFile, options);
            assertCompilation(compilation);
        }
    }
}