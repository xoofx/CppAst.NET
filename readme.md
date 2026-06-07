# CppAst.NET [![ci](https://github.com/xoofx/CppAst.NET/actions/workflows/ci.yml/badge.svg)](https://github.com/xoofx/CppAst.NET/actions/workflows/ci.yml) [![Coverage Status](https://coveralls.io/repos/github/xoofx/CppAst.NET/badge.svg?branch=main)](https://coveralls.io/github/xoofx/CppAst.NET?branch=main) [![NuGet](https://img.shields.io/nuget/v/CppAst.svg)](https://www.nuget.org/packages/CppAst/)

<img align="right" width="160px" height="160px" src="https://raw.githubusercontent.com/xoofx/CppAst.NET/main/img/cppast.png">

CppAst provides a C/C++ parser for header files with access to a managed AST model, comments and macros for .NET.

## Purpose

> The target primary usage of this library is to serve as a simple foundation for domain oriented PInvoke/Interop codegen

## Features

- Compatible with `net8.0`
    - For `netstandard2.0` use `0.14.0` version.
- Uses `Clang/libclang 21.1.8.x` through ClangSharp
- Parses *in-memory* C/C++ text and C/C++ files from disk
- Supports C, C++ and Objective-C language modes via `CppParserOptions.ParserKind`
- Simple managed AST model with declarations for namespaces, classes/structs/unions, Objective-C interfaces, enums, fields/global variables, functions/methods, typedefs/using aliases and include directives
- Broad type system for primitive, record, enum, typedef, pointer, reference, array, function, block-function, qualified, template and unexposed/dependent types
- C++ support for common class, inheritance, constructor/destructor, operator, conversion, friend, inline/final/defaulted/deleted, template and specialization metadata
- Provides access to system/annotate attributes, optional token-level attributes (`CppParserOptions.ParseTokenAttributes`) and comment-based attributes (`CppParserOptions.ParseCommentAttribute`)
- Provides access to attached comments, including Doxygen comments and parameter comment commands
- Provides access to expressions for variable initializers and default parameter values (e.g. `const int x = (1 + 2) << 1` exposes the initializer expression)
- Provides function body source spans with `CppParserOptions.ParseFunctionBodies` (not a full statement-body AST)
- Provides access to macro definitions, parameters and tokens via `CppParserOptions.ParseMacros` (default is `false`)
- Provides target configuration helpers such as `ConfigureForWindowsMsvc(...)` and `ParseSystemIncludes` filtering for system headers

## Documentation

Check the [user guide](doc/readme.md) documentation from the `doc/` folder.

## Usage Example

### Setup
After installing the NuGet package, configure your project to select a platform RID via the `RuntimeIdentifier` property so the native libclang asset is restored:

```xml
  <PropertyGroup>
    <!-- Workaround for issue https://github.com/microsoft/ClangSharp/issues/129 -->
    <RuntimeIdentifier Condition="'$(RuntimeIdentifier)' == '' AND '$(PackAsTool)' != 'true'">$(NETCoreSdkRuntimeIdentifier)</RuntimeIdentifier>
  </PropertyGroup>
```

### Code

You can jump-start with the `CppParser.Parse` method:

```C#
// Parse C++ files
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

CppAst is a lightweight model over libclang and intentionally does not expose every Clang cursor or statement node. Some known limitations are:

- Function bodies are exposed as source spans when requested, not as a full statement AST.
- Template/dependent/unexposed types are represented on a best-effort basis and may require inspecting `CppUnexposedType`, display names or diagnostics for advanced C++ constructs.
- Token-level attributes are still available for compatibility but are obsolete; prefer system/annotate attributes when possible.
- Type sizes and built-in aliases such as `size_t` follow the configured target triple/ABI.

## License

This software is released under the [BSD-Clause 2 license](https://opensource.org/licenses/BSD-2-Clause). 

## Credits

* [ClangSharp](https://github.com/microsoft/ClangSharp): .NET managed wrapper around Clang/libclang

## Related

The C++ project [cppast](https://github.com/foonathan/cppast) serves similar purpose although CppAst.NET does not share API or any implementation details.

## Author

Alexandre Mutel aka [xoofx](https://xoofx.github.io).
