// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CppAst
{
    /// <summary>
    /// Defines the options used by the <see cref="CppParser"/>
    /// </summary>
    public class CppParserOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CppParserOptions()
        {
            ParseAsCpp = true;
            SystemIncludeFolders = new List<string>();
            IncludeFolders = new List<string>();
            Defines = new List<string>();
            AdditionalArguments = new List<string>()
            {
                "-Wno-pragma-once-outside-header"
            };
            AutoSquashTypedef = true;
            ParseMacros = false;
            ParseComments = true;

            // Default triple targets
            TargetCpu = CppTargetCpu.X86;
            TargetCpuSub = string.Empty;
            TargetVendor = "pc";
            TargetSystem = "windows";
            TargetAbi = "";
        }
        
        /// <summary>
        /// List of the include folders.
        /// </summary>
        public List<string> IncludeFolders { get; }

        /// <summary>
        /// List of the system include folders.
        /// </summary>
        public List<string> SystemIncludeFolders { get; }

        /// <summary>
        /// List of the defines.
        /// </summary>
        public List<string> Defines { get; }

        /// <summary>
        /// List of the additional arguments passed directly to the C++ Clang compiler.
        /// </summary>
        public List<string> AdditionalArguments { get; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the files will be parser as C++. Default is <c>true</c>. Otherwise parse as C.
        /// </summary>
        public bool ParseAsCpp { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether to parser comments. Default is <c>true</c>
        /// </summary>
        public bool ParseComments { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether to parse macros. Default is <c>false</c>.
        /// </summary>
        public bool ParseMacros { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether un-named enum/struct referenced by a typedef will be renamed directly to the typedef name. Default is <c>true</c>
        /// </summary>
        public bool AutoSquashTypedef { get; set; }
        
        /// <summary>
        /// Sets <see cref="ParseMacros"/> to <c>true</c> and return this instance.
        /// </summary>
        /// <returns>This instance</returns>
        public CppParserOptions EnableMacros()
        {
            ParseMacros = true;
            return this;
        }

        /// <summary>
        /// Cpu Clang target. Default is <see cref="CppTargetCpu.X86"/>
        /// </summary>
        public CppTargetCpu TargetCpu { get; set; }

        /// <summary>
        /// Cpu sub Clang target. Default is ""
        /// </summary>
        public string TargetCpuSub { get; set; }

        /// <summary>
        /// Vendor Clang target. Default is "pc"
        /// </summary>
        public string TargetVendor { get; set; }

        /// <summary>
        /// System Clang target. Default is "windows"
        /// </summary>
        public string TargetSystem { get; set; }

        /// <summary>
        /// Abi Clang target. Default is ""
        /// </summary>
        public string TargetAbi { get; set; }

        /// <summary>
        /// Configure this instance with Windows and MSVC.
        /// </summary>
        /// <returns>This instance</returns>
        public CppParserOptions ConfigureForWindowsMsvc(CppTargetCpu targetCpu = CppTargetCpu.X86, CppVisualStudioVersion vsVersion = CppVisualStudioVersion.VS2019)
        {
            // 1920
            var highVersion = ((int) vsVersion) / 100;  // => 19
            var lowVersion = ((int) vsVersion) % 100;   // => 20

            var versionAsString = $"{highVersion}.{lowVersion}";

            TargetCpu = targetCpu;
            TargetCpuSub = string.Empty;
            TargetVendor = "pc";
            TargetSystem = "windows";
            TargetAbi = $"msvc{versionAsString}";

            // See https://docs.microsoft.com/en-us/cpp/preprocessor/predefined-macros?view=vs-2019

            Defines.Add($"_MSC_VER={(int)vsVersion}");
            Defines.Add("_WIN32=1");

            switch (targetCpu)
            {
                case CppTargetCpu.X86:
                    Defines.Add("_M_IX86=600");
                    break;
                case CppTargetCpu.X86_64:
                    Defines.Add("_M_AMD64=100");
                    Defines.Add("_M_X64=100");
                    Defines.Add("_WIN64=1");
                    break;
                case CppTargetCpu.ARM:
                    Defines.Add("_M_ARM=7");
                    break;
                case CppTargetCpu.ARM64:
                    Defines.Add("_M_ARM64=1");
                    Defines.Add("_WIN64=1");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetCpu), targetCpu, null);
            }
            
            AdditionalArguments.Add("-fms-extensions");
            AdditionalArguments.Add("-fms-compatibility");
            AdditionalArguments.Add($"-fms-compatibility-version={versionAsString}");
            return this;
        }
    }
}