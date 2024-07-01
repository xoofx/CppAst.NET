file(GLOB_RECURSE SOURCE_FILES ${CMAKE_CURRENT_SOURCE_DIR}/*.cs)

add_library(CppAst.Tests SHARED ${SOURCE_FILES})

set_target_properties(CppAst.Tests PROPERTIES
    VS_PACKAGE_REFERENCES "Microsoft.NET.Test.Sdk_17.8.0;nunit_4.0.1;NUnit3TestAdapter_4.5.0"
)

target_compile_options(CppAst.Tests PUBLIC "/unsafe")
target_link_libraries(CppAst.Tests CppAst)