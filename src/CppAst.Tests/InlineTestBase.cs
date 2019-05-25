using System;
using System.IO;
using NUnit.Framework;

namespace CppAst.Tests
{
    public class InlineTestBase
    {
        public CppParserOptions GetDefaultOptions()
        {
            return new CppParserOptions();
        }
        
        public CppCompilation Parse(string text, CppParserOptions options = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            options = options ?? GetDefaultOptions();

            var currentDirectory = Environment.CurrentDirectory;
            var headerFilename = $"{TestContext.CurrentContext.Test.FullName}-{TestContext.CurrentContext.Test.ID}.h";
            var headerFile = Path.Combine(currentDirectory, headerFilename);

            File.WriteAllText(headerFile, text);

            options.Files.Add(headerFile);

            return CppParser.Parse(options);
        }

        public void ParseAssert(string text, Action<CppCompilation> assertCompilation, CppParserOptions options = null)
        {
            if (assertCompilation == null) throw new ArgumentNullException(nameof(assertCompilation));
            var compilation = Parse(text, options);
            assertCompilation(compilation);
        }
    }
}