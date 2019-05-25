using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClangSharp;

namespace CppAst
{
    public class CppParserOptions
    {
        public CppParserOptions()
        {
            IsCplusplus = true;
            IncludeFolders = new List<string>();
            Defines = new List<string>();
            AdditionalArguments = new List<string>()
            {
                "-Wno-pragma-once-outside-header"
            };
            AutoSquashTypedef = true;
            ParseMacros = false;
            ParseComments = true;
            DefaultWindowsCompatibility = "19.0";
        }
        
        public List<string> IncludeFolders { get; }

        public List<string> Defines { get; }

        public List<string> AdditionalArguments { get; }

        public bool IsCplusplus { get; set; }

        public bool AutoSquashTypedef { get; set; }

        public bool ParseComments { get; set; }

        public bool IsWindows { get; set; }

        public bool ParseMacros { get; set; }

        public string DefaultWindowsCompatibility { get; set; }

        public CppParserOptions EnableMacros()
        {
            ParseMacros = true;
            return this;
        }

        public CppParserOptions EnableWindows()
        {
            IsWindows = true;
            return this;
        }
    }

    public static class CppParser
    {
        public static CppCompilation Parse(string cppFile, CppParserOptions options = null)
        {
            var files = new List<string>() {cppFile};
            return Parse(files, options);
        }

        public static CppCompilation Parse(List<string> cppFiles, CppParserOptions options = null)
        {
            if (cppFiles == null) throw new ArgumentNullException(nameof(cppFiles));

            options = options ?? new CppParserOptions();

            var arguments = new List<string>();

            // Make sure that paths are absolute
            var normalizedIncludePaths = new List<string>();
            normalizedIncludePaths.AddRange(options.IncludeFolders.Select(x => Path.Combine(Environment.CurrentDirectory, x)));

            arguments.AddRange(options.AdditionalArguments);
            arguments.AddRange(normalizedIncludePaths.Select(x => $"-I{x}"));
            arguments.AddRange(options.Defines.Select(x => $"-D{x}"));

            if (options.IsCplusplus && !arguments.Contains("-xc++"))
            {
                arguments.Add("-xc++");
            }

            if (options.IsWindows)
            {
                arguments.Add($"-DWIN32");
                arguments.Add($"-D_WIN32");
                arguments.Add("-Wno-microsoft-enum-value");
                arguments.Add($"-fms-compatibility-version={options.DefaultWindowsCompatibility}");
            }

            if (options.ParseComments)
            {
                arguments.Add("-fparse-all-comments");
            }

            var translationFlags = CXTranslationUnit_Flags.CXTranslationUnit_None;
            translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_SkipFunctionBodies;                   // Don't traverse function bodies
            translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_IncludeAttributedTypes;               // Include attributed types in CXType
            translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_VisitImplicitAttributes;              // Implicit attributes should be visited

            if (options.ParseMacros)
            {
                translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_DetailedPreprocessingRecord;
            }

            var argumentsArray = arguments.ToArray();

            using (var createIndex = CXIndex.Create())
            {
                var builder = new CppModelBuilder {AutoSquashTypedef = options.AutoSquashTypedef};
                var compilation = builder.RootCompilation;

                foreach (var file in cppFiles)
                {
                    var filePath = Path.Combine(Environment.CurrentDirectory, file);

                    var translationUnitError = CXTranslationUnit.Parse(createIndex, filePath, argumentsArray, Array.Empty<CXUnsavedFile>(), translationFlags, out CXTranslationUnit translationUnit);
                    bool skipProcessing = false;

                    if (translationUnitError != CXErrorCode.CXError_Success)
                    {
                        compilation.Diagnostics.Error($"Parsing failed due to '{translationUnitError}'", new CppSourceLocation(filePath, 0, 1, 1));
                        skipProcessing = true;
                    }
                    else if (translationUnit.NumDiagnostics != 0)
                    {
                        for (uint i = 0; i < translationUnit.NumDiagnostics; ++i)
                        {
                            using (var diagnostic = translationUnit.GetDiagnostic(i))
                            {
                                switch (diagnostic.Severity)
                                {
                                    case CXDiagnosticSeverity.CXDiagnostic_Ignored:
                                    case CXDiagnosticSeverity.CXDiagnostic_Note:
                                        compilation.Diagnostics.Info(diagnostic.ToString(), CppModelBuilder.GetSourceLocation(diagnostic.Location));
                                        break;
                                    case CXDiagnosticSeverity.CXDiagnostic_Warning:
                                        compilation.Diagnostics.Warning(diagnostic.ToString(), CppModelBuilder.GetSourceLocation(diagnostic.Location));
                                        break;
                                    case CXDiagnosticSeverity.CXDiagnostic_Error:
                                    case CXDiagnosticSeverity.CXDiagnostic_Fatal:
                                        compilation.Diagnostics.Error(diagnostic.ToString(), CppModelBuilder.GetSourceLocation(diagnostic.Location));
                                        skipProcessing = true;
                                        break;
                                }
                            }
                        }
                    }

                    if (skipProcessing)
                    {
                        compilation.Diagnostics.Warning($"Skipping '{file}' due to one or more errors listed above.", new CppSourceLocation(filePath, 0, 1, 1));
                        continue;
                    }

                    using (translationUnit)
                    {
                        translationUnit.Cursor.VisitChildren(builder.VisitTranslationUnit, clientData: default);
                    }
                }

                return compilation;
            }
        }
    }
}