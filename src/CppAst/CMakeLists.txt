file(GLOB_RECURSE SOURCE_FILES ${CMAKE_CURRENT_SOURCE_DIR}/*.cs)

set(CMAKE_CSharp_FLAGS "${CMAKE_CSharp_FLAGS} /langversion:7.3")

add_library(CppAst SHARED ${SOURCE_FILES})

set_target_properties(CppAst PROPERTIES
    VS_PACKAGE_REFERENCES "ClangSharp_17.0.1;Irony_1.2.0"
)

target_compile_options(CppAst PUBLIC "/unsafe")