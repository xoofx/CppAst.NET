// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ClangSharp;
using ClangSharp.Interop;

namespace CppAst
{
    /// <summary>
    /// C/C++ Parser entry point functions.
    /// </summary>
    public static class CppParser
    {
        public const string CppAstRootFileName = "cppast.input";

        /// <summary>
        /// Parse the specified C++ text in-memory.
        /// </summary>
        /// <param name="cppText">A string with a C/C++ text</param>
        /// <param name="options">Options used for parsing this file (e.g include folders...)</param>
        /// <param name="cppFilename">Optional path to a file only used for reporting errors. Default is 'content'</param>
        /// <returns>The result of the compilation</returns>
        public static CppCompilation Parse(string cppText, CppParserOptions options = null, string cppFilename = "content")
        {
            if (cppText == null) throw new ArgumentNullException(nameof(cppText));
            var cppFiles = new List<CppFileOrString> { new CppFileOrString() { Filename = cppFilename, Content = cppText, } };
            return ParseInternal(cppFiles, options);
        }

        /// <summary>
        /// Parse the specified single file.
        /// </summary>
        /// <param name="cppFilename">A path to a C/C++ file on the disk to parse</param>
        /// <param name="options">Options used for parsing this file (e.g include folders...)</param>
        /// <returns>The result of the compilation</returns>
        public static CppCompilation ParseFile(string cppFilename, CppParserOptions options = null)
        {
            if (cppFilename == null) throw new ArgumentNullException(nameof(cppFilename));
            var files = new List<string>() { cppFilename };
            return ParseFiles(files, options);
        }

        /// <summary>
        /// Parse the specified single file.
        /// </summary>
        /// <param name="cppFilenameList">A list of path to C/C++ header files on the disk to parse</param>
        /// <param name="options">Options used for parsing this file (e.g include folders...)</param>
        /// <returns>The result of the compilation</returns>
        public static CppCompilation ParseFiles(List<string> cppFilenameList, CppParserOptions options = null)
        {
            if (cppFilenameList == null) throw new ArgumentNullException(nameof(cppFilenameList));

            var cppFiles = new List<CppFileOrString>();
            foreach (var cppFilepath in cppFilenameList)
            {
                if (string.IsNullOrEmpty(cppFilepath)) throw new InvalidOperationException("A null or empty filename is invalid in the list");
                cppFiles.Add(new CppFileOrString() { Filename = cppFilepath });
            }
            return ParseInternal(cppFiles, options);
        }

        /// <summary>
        /// Private method parsing file or content.
        /// </summary>
        /// <param name="cppFiles">A list of path to C/C++ header files on the disk to parse</param>
        /// <param name="options">Options used for parsing this file (e.g include folders...)</param>
        /// <returns>The result of the compilation</returns>
        private static unsafe CppCompilation ParseInternal(List<CppFileOrString> cppFiles, CppParserOptions options = null)
        {
            if (cppFiles == null) throw new ArgumentNullException(nameof(cppFiles));

            options = options ?? new CppParserOptions();

            var arguments = new List<string>();

            // Make sure that paths are absolute
            var normalizedIncludePaths = new List<string>();
            normalizedIncludePaths.AddRange(options.IncludeFolders.Select(x => Path.Combine(Environment.CurrentDirectory, x)));

            var normalizedSystemIncludePaths = new List<string>();
            normalizedSystemIncludePaths.AddRange(options.SystemIncludeFolders.Select(x => Path.Combine(Environment.CurrentDirectory, x)));

            arguments.AddRange(options.AdditionalArguments);
            arguments.AddRange(normalizedIncludePaths.Select(x => $"-I{x}"));
            arguments.AddRange(normalizedSystemIncludePaths.Select(x => $"-isystem{x}"));
            arguments.AddRange(options.Defines.Select(x => $"-D{x}"));

            arguments.Add("-dM");
            arguments.Add("-E");

            if (options.ParseAsCpp && !arguments.Contains("-xc++"))
            {
                arguments.Add("-xc++");
            }

            if (!arguments.Any(x => x.StartsWith("--target=")))
            {
                arguments.Add($"--target={GetTripleFromOptions(options)}");
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
            translationFlags |= CXTranslationUnit_Flags.CXTranslationUnit_DetailedPreprocessingRecord;

            var argumentsArray = arguments.ToArray();

            using (var createIndex = CXIndex.Create())
            {
                var builder = new CppModelBuilder
                {
                    AutoSquashTypedef = options.AutoSquashTypedef,
                    ParseSystemIncludes = options.ParseSystemIncludes,
                    ParseAttributeEnabled = options.ParseAttributes,
                };
                var compilation = builder.RootCompilation;

                string rootFileName = CppAstRootFileName;
                string rootFileContent = null;

                // Build the root input source file
                var tempBuilder = new StringBuilder();
                if (options.PreHeaderText != null)
                {
                    tempBuilder.AppendLine(options.PreHeaderText);
                }

                foreach (var file in cppFiles)
                {
                    if (file.Content != null)
                    {
                        tempBuilder.AppendLine(file.Content);
                    }
                    else
                    {
                        var filePath = Path.Combine(Environment.CurrentDirectory, file.Filename);
                        tempBuilder.AppendLine($"#include \"{filePath}\"");
                    }
                }

                if (options.PostHeaderText != null)
                {
                    tempBuilder.AppendLine(options.PostHeaderText);
                }

                // TODO: Add debug
                rootFileContent = tempBuilder.ToString();

                var rootFileContentUTF8 = Encoding.UTF8.GetBytes(rootFileContent);
                compilation.InputText = rootFileContent;

                fixed (void* rootFileContentUTF8Ptr = rootFileContentUTF8)
                {
                    CXTranslationUnit translationUnit;

                    var rootFileNameUTF8 = Marshal.StringToHGlobalAnsi(rootFileName);

                    translationUnit = CXTranslationUnit.Parse(createIndex, rootFileName, argumentsArray,new CXUnsavedFile[]
                    {
                        new CXUnsavedFile()
                        {
                            Contents = (sbyte*) rootFileContentUTF8Ptr,
                            Filename = (sbyte*) rootFileNameUTF8,
                            Length = new UIntPtr((uint)rootFileContentUTF8.Length)

                        }
                    }, translationFlags);

                    bool skipProcessing = false;

                    if (translationUnit.NumDiagnostics != 0)
                    {
                        for (uint i = 0; i < translationUnit.NumDiagnostics; ++i)
                        {
                            using (var diagnostic = translationUnit.GetDiagnostic(i))
                            {

                                CppSourceLocation location;
                                var message = GetMessageAndLocation(rootFileContent, diagnostic, out location);

                                switch (diagnostic.Severity)
                                {
                                    case CXDiagnosticSeverity.CXDiagnostic_Ignored:
                                    case CXDiagnosticSeverity.CXDiagnostic_Note:
                                        compilation.Diagnostics.Info(message, location);
                                        break;
                                    case CXDiagnosticSeverity.CXDiagnostic_Warning:
                                        compilation.Diagnostics.Warning(message, location);
                                        break;
                                    case CXDiagnosticSeverity.CXDiagnostic_Error:
                                    case CXDiagnosticSeverity.CXDiagnostic_Fatal:
                                        compilation.Diagnostics.Error(message, location);
                                        skipProcessing = true;
                                        break;
                                }
                            }
                        }
                    }

                    if (skipProcessing)
                    {
                        compilation.Diagnostics.Warning($"Compilation aborted due to one or more errors listed above.", new CppSourceLocation(rootFileName, 0, 1, 1));
                    }
                    else
                    {
                        using (translationUnit)
                        {
                            translationUnit.Cursor.VisitChildren(builder.VisitTranslationUnit, clientData: default);
                        }
                    }
                }

                return compilation;
            }
        }

        private static string GetMessageAndLocation(string rootContent, CXDiagnostic diagnostic, out CppSourceLocation location)
        {
            var builder = new StringBuilder();
            builder.Append(diagnostic.ToString());
            location = CppModelBuilder.GetSourceLocation(diagnostic.Location);
            if (location.File == CppAstRootFileName)
            {
                var reader = new StringReader(rootContent);
                var lines = new List<string>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }

                var lineIndex = location.Line - 1;
                if (lineIndex < lines.Count)
                {
                    builder.AppendLine();
                    builder.AppendLine(lines[lineIndex]);
                    for (int i = 0; i < location.Column - 1; i++)
                    {
                        builder.Append(i + 1 == location.Column - 1 ? "-" : " ");
                    }

                    builder.AppendLine("^-");
                }
            }

            return builder.ToString();
        }

        private static string GetTripleFromOptions(CppParserOptions options)
        {
            // From https://clang.llvm.org/docs/CrossCompilation.html
            // <arch><sub>-<vendor>-<sys>-<abi>
            var targetCpu = GetTargetCpuAsString(options.TargetCpu);
            var targetCpuSub = options.TargetCpuSub ?? string.Empty;
            var targetVendor = options.TargetVendor ?? "pc";
            var targetSystem = options.TargetSystem ?? "windows";
            var targetAbi = options.TargetAbi ?? "";

            return $"{targetCpu}{targetCpuSub}-{targetVendor}-{targetSystem}-{targetAbi}";
        }

        private static string GetTargetCpuAsString(CppTargetCpu targetCpu)
        {
            switch (targetCpu)
            {
                case CppTargetCpu.X86:
                    return "i686";
                case CppTargetCpu.X86_64:
                    return "x86_64";
                case CppTargetCpu.ARM:
                    return "arm";
                case CppTargetCpu.ARM64:
                    return "aarch64";
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetCpu), targetCpu, null);
            }
        }

        private struct CppFileOrString
        {
            public string Filename;

            public string Content;

            public override string ToString()
            {
                return $"{nameof(Filename)}: {Filename}, {nameof(Content)}: {Content}";
            }
        }
    }
}