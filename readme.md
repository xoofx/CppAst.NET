# CppAst.NET [![Build Status](https://github.com/xoofx/CppAst/workflows/ci/badge.svg?branch=main)](https://github.com/xoofx/CppAst/actions) [![Coverage Status](https://coveralls.io/repos/github/xoofx/CppAst.NET/badge.svg?branch=main)](https://coveralls.io/github/xoofx/CppAst.NET?branch=main) [![NuGet](https://img.shields.io/nuget/v/CppAst.svg)](https://www.nuget.org/packages/CppAst/)

<img align="right" width="160px" height="160px" src="https://raw.githubusercontent.com/xoofx/CppAst.NET/main/img/cppast.png">

CppAst provides a C/C++ parser for header files with access to the full AST, comments and macros for .NET Framework and .NET Core

## Purpose

> The target primary usage of this library is to serve as a simple foundation for domain oriented PInvoke/Interop codegen

## Features

- Compatible with `net8.0`
    - For `netstandard2.0` use `0.14.0` version.
- Using `Clang/libclang 17.0.1`
- Allow to parse *in-memory* C/C++ text and C/C++ files from the disk
- Simple AST model
- Full type system
- Provides basic access to attributes (`_declspec(...)` or `__attribute__((...))`)
- Provides access to attached comments
- Provides access to expressions for variable and parameter init value (e.g `const int x = (1 + 2) << 1` the `(1 + 2) << 1` will be retrievable as a binary expression from the AST)
- Provides access to macro definitions, including tokens via the option `CppParserOptions.ParseMacros` (default is `false`)

## Documentation

Check the [user guide](doc/readme.md) documentation from the `doc/` folder.

## Usage Example

### Setup
After installing the NuGet package, you need to modify your csproj to select a Platform RID via the `RuntimeIdentifier` property:

```xml
  <PropertyGroup>
    <!-- Workaround for issue https://github.com/microsoft/ClangSharp/issues/129 -->
    <RuntimeIdentifier Condition="'$(RuntimeIdentifier)' == '' AND '$(PackAsTool)' != 'true'">$(NETCoreSdkRuntimeIdentifier)</RuntimeIdentifier>
  </PropertyGroup>
```

### Code

You can jump-start with the `CppParser.Parse` method:

```C#
// Parse a C++ files
var compilation = CppParser.Parse(@"
enum MyEnum { MyEnum_0, MyEnum_1 };
void function0(int a, int b);
struct MyStruct { int field0; int field1;};
typedef MyStruct* MyStructPtr;
"
);
// Print diagnostic messages
foreach (var message in compilation.Diagnostics.Messages)
    Console.WriteLine(message);

// Print All enums
foreach (var cppEnum in compilation.Enums)
    Console.WriteLine(cppEnum);

// Print All functions
foreach (var cppFunction in compilation.Functions)
    Console.WriteLine(cppFunction);

// Print All classes, structs
foreach (var cppClass in compilation.Classes)
    Console.WriteLine(cppClass);

// Print All typedefs
foreach (var cppTypedef in compilation.Typedefs)
    Console.WriteLine(cppTypedef);
```

Prints the following result:

```
enum MyEnum {...}
void function0(int a, int b)
struct MyStruct { ... }
typedef MyStruct* MyStructPtr
```

## Binaries

This library is distributed as a NuGet package [![NuGet](https://img.shields.io/nuget/v/CppAst.svg)](https://www.nuget.org/packages/CppAst/)

## Known issues

The library libclang used by this project has some known issues and limitations:

- Attributes are not fully exposed (e.g in function parameters, on typedefs...)
- Generic instance types are not fully exposed (e.g used as parameters, or as base types...) 

## License

This software is released under the [BSD-Clause 2 license](https://opensource.org/licenses/BSD-2-Clause). 

## Credits

* [ClangSharp](https://github.com/microsoft/ClangSharp): .NET managed wrapper around Clang/libclang

## Related

The C++ project [cppast](https://github.com/foonathan/cppast) serves similar purpose although CppAst.NET does not share API or any implementation details.

## Author

Alexandre Mutel aka [xoofx](https://xoofx.github.io).
