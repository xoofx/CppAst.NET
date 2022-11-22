// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace CppAst
{
    /// <summary>
    /// Enumeration used to define the VS version used when parsing. Used via <see cref="CppParserOptions.ConfigureForWindowsMsvc"/>
    /// </summary>
    /// <remarks>
    /// From https://docs.microsoft.com/en-us/cpp/preprocessor/predefined-macros?view=vs-2019
    /// </remarks>
    public enum CppVisualStudioVersion
    {
        /// <summary>
        /// Visual Studio 6.0
        /// </summary>
        VS6 = 1200,

        /// <summary>
        /// Visual Studio .NET 2002 (7.0)
        /// </summary>
        VSNET2002 = 1300,

        /// <summary>
        /// Visual Studio .NET 2003 (7.1)
        /// </summary>
        VSNET2003 = 1310,

        /// <summary>
        /// Visual Studio 2005 (8.0)
        /// </summary>
        VS2005 = 1400,

        /// <summary>
        /// Visual Studio 2008 (9.0)
        /// </summary>
        VS2008 = 1500,

        /// <summary>
        /// Visual Studio 2010 (10.0)
        /// </summary>
        VS2010 = 1600,

        /// <summary>
        /// Visual Studio 2012 (11.0)
        /// </summary>
        VS2012 = 1700,

        /// <summary>
        /// Visual Studio 2013 (12.0)
        /// </summary>
        VS2013 = 1800,

        /// <summary>
        /// Visual Studio 2015 (14.0)
        /// </summary>
        VS2015 = 1900,

        /// <summary>
        /// Visual Studio 2017 RTW (15.0)
        /// </summary>
        VS2017_15_0 = 1910,

        /// <summary>
        /// Visual Studio 2017 version 15.3
        /// </summary>
        VS2017_15_3 = 1911,

        /// <summary>
        /// Visual Studio 2017 version 15.5
        /// </summary>
        VS2017_15_5 = 1912,

        /// <summary>
        /// Visual Studio 2017 version 15.6
        /// </summary>
        VS2017_15_6 = 1913,

        /// <summary>
        /// Visual Studio 2017 version 15.7
        /// </summary>
        VS2017_15_7 = 1914,

        /// <summary>
        /// Visual Studio 2017 version 15.8
        /// </summary>
        VS2017_15_8 = 1915,

        /// <summary>
        /// Visual Studio 2017 version 15.9
        /// </summary>
        VS2017_15_9 = 1916,

        /// <summary>
        /// Visual Studio 2019 RTW (16.0)
        /// </summary>
        VS2019 = 1920,

        ///// <summary>
        ///// Visual Studio 2022 RTW (17.0)
        ///// </summary>
        VS2022 = 1930
    }
}