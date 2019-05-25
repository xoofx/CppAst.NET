// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClangSharp;

namespace CppAst
{
    /// <summary>
    /// C/C++ Parser entry point functions.
    /// </summary>
    public static class CppParser
    {
        /// <summary>
        /// Parse the specified single file.
        /// </summary>
        /// <param name="cppFile">A path to a C/C++ file on the disk to parse</param>
        /// <param name="options">Options used for parsing this file (e.g include folders...)</param>
        /// <returns>The result of the compilation</returns>
        public static CppCompilation Parse(string cppFile, CppParserOptions options = null)
        {
            var files = new List<string>() {cppFile};
            return Parse(files, options);
        }

        /// <summary>
        /// Parse the specified single file.
        /// </summary>
        /// <param name="cppFiles">A list of path to C/C++ header files on the disk to parse</param>
        /// <param name="options">Options used for parsing this file (e.g include folders...)</param>
        /// <returns>The result of the compilation</returns>
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

            if (options.IsWindowsPlatform)
            {
                arguments.Add($"-DWIN32");
                arguments.Add($"-D_WIN32");
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