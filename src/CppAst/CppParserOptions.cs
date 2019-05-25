// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

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

        
        /// <summary>
        /// List of the include folders.
        /// </summary>
        public List<string> IncludeFolders { get; }

        /// <summary>
        /// List of the defines.
        /// </summary>
        public List<string> Defines { get; }

        /// <summary>
        /// List of the additional arguments passed directly to the C++ Clang compiler.
        /// </summary>
        public List<string> AdditionalArguments { get; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the files will be parser as C++. Default is <c>true</c>.
        /// </summary>
        public bool IsCplusplus { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether un-named enum/struct referenced by a typedef will be renamed directly to the typedef name. Default is <c>true</c>
        /// </summary>
        public bool AutoSquashTypedef { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether to parser comments. Default is <c>true</c>
        /// </summary>
        public bool ParseComments { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating to compile header files for the windows platforms (e.g will define `WIN32` and clang <see cref="DefaultWindowsCompatibility"/> mode for example )
        /// </summary>
        public bool IsWindowsPlatform { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether to parse macros. Default is <c>false</c>.
        /// </summary>
        public bool ParseMacros { get; set; }

        /// <summary>
        /// Gets or sets the Windows platform compatibility mode. Default is `19.0`
        /// </summary>
        public string DefaultWindowsCompatibility { get; set; }

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
        /// Sets <see cref="IsWindowsPlatform"/> to <c>true</c> and return this instance.
        /// </summary>
        /// <returns>This instance</returns>
        public CppParserOptions EnableWindowsPlatform()
        {
            IsWindowsPlatform = true;
            return this;
        }
    }
}