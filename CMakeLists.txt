# This file is not used for compiling but used
# when generating project using CMake generator.
# If you are not using CMake then just open .sln file

cmake_minimum_required(VERSION 3.22)

project(CppAst.NET CSharp)

set(CMAKE_CSharp_FLAGS "${CMAKE_CSharp_FLAGS} /langversion:latest")

set(CMAKE_DOTNET_TARGET_FRAMEWORK net8.0)
set(CMAKE_DOTNET_SDK "Microsoft.NET.Sdk")

add_subdirectory(src)