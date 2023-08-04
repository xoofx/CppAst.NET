## 1. `cppast.net 0.12` Support for `attributes`
The original support of `cppast.net` for various types of `attributes`, including the `meta attribute` of `c++17`, is restricted due to the limitation of `libclang` itself `Api`. We need to rely on token-level parsing to implement related functions. In the implementation of `cppast.net 0.12` and previous versions, we used parsing `token` to implement related functions. Even some `attributes` that `libclang` supports well, such as `dllexport`, `dllimport`, etc., `cppast.net` most of the time also uses token parsing. Although this approach is flexible and we can always try to parse the related `attributes` from the `token` level, it also brings some problems and restrictions, including:
1. `ParseAttributes()` is extremely time-consuming, which led to the addition of the `ParseAttributes` parameter in later versions to control whether to parse `attributes`. However, in some cases, we need to rely on `attributes` to complete the related functions, which is obviously inconvenient.
2. There are defects in the parsing of `meta attribute` - `[[]]`. For `meta attribute` defined above `Function` and `Field`, it is obviously legal at the semantic level, but `cppast.net` does not support this type of `meta attribute` defined above the object very well (there are some exceptions here, like `namespace`, `class`, `enum` these `attribute` declarations, the attribute definition itself cannot be at the top, the compiler will report an error directly for the related usage, it can only be after the related keywords, such as `class [[deprecated]] Abc{};`).
3. Individual parameters of `meta attribute` use macros. Because our original implementation is based on `token` parsing, macros during compilation obviously cannot be correctly handled in this case.

---
## 2. A brief introduction to cases where `attribute` is needed

---
### 2.1 System-level `attribute`
Taking the code segment in the `cppast.net` test case as an example:
```cpp
#ifdef WIN32
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API __attribute__((visibility(""default"")))
#endif
```
For `attributes` like `dllexport` and `visibility` that control interface visibility, we definitely use them more often, not only when `ParseAttributes` is turned on to make it work. We need to provide a high-performance solution for these basic system attributes, and the implementation should not be affected by the switch.

---
### 2.2 Injection of Additional Information by Export Tools and Other Tools
&emsp;&emsp;Let's take the following class definition as an example:
```cpp
#if !defined(__cppast) 
#define __cppast(...)
#endif

struct __cppast(msgid = 1) TestPBMessage {
 public:
  __cppast(id = 1)
  float x;
  __cppast(id = 2)
  double y;
  __cppast(id = 3)
  uint64_t z;
};
```
To better support serialization and deserialization of `TestPBMessage`, and to have a certain degree of fault tolerance, we have added some additional information based on the original struct definition:
1. The msgid of `TestPBMessage`, here it is directly specified as integer `1`.
2. The `id` of x, y, and z, here directly using `1`, `2`, and `3` respectively.
This way, if we use `cppast.net` to create our offline processing tools, we definitely need to conveniently read out the various 'meta attributes' injected by `__cppast()` which do not directly impact the original code compilation in the tool, and use them appropriately. However, the performance of this part in `cppast.net 0.12` and previous versions is rather poor and has limitations. For example, it can't support cases like the one above where the `attribute` is directly defined on the `Field`.

---
## 3. New Implementation and Adjustment
&emsp;&emsp;The new implementation is mainly based on the limitations of the current implementation mentioned earlier, and the various application scenarios mentioned in the previous chapter. We have re-categorized the `attribute` into three types:
1. `AttributeKind.CxxSystemAttribute` - It corresponds to various system `attributes` that `libclang` itself can parse very well, such as `visibility` mentioned above, as well as `[[deprecated]]`, `[[noreturn]]`, etc. With the help of `ClangSharp`, we can efficiently parse and handle them, so there is no need to worry about switch issues.
2. `AttributeKind.TokenAttribute` - As the name suggests, this corresponds to the `attribute` in the original version of `cppast.net`. It has been marked as `deprecated`, but the parsing of `token` is always a fallback implementation mechanism. We will keep the relevant `Tokenizer` code and use them cautiously to implement some complex features when `ClangSharp` is unable to implement related functions.
3. `AttributeKind.AnnotateAttribute` - This is used to replace the original `meta attribute` implemented based on `token` parsing, aiming to inject methods for classes and members with high performance and low restrictions as introduced earlier.

Next, we will briefly introduce the implementation ideas and usage of various types of `attributes`.

---
### 3.1 `AttributeKind.CxxSystemAttribute`
&emsp;&emsp;We added a function to handle various `attributes` that `ClangSharp` itself supports:
```cs
        private List<CppAttribute> ParseSystemAndAnnotateAttributeInCursor(CXCursor cursor)
        {
            List<CppAttribute> collectAttributes = new List<CppAttribute>();
            cursor.VisitChildren((argCursor, parentCursor, clientData) =>
            {
                var sourceSpan = new CppSourceSpan(GetSourceLocation(argCursor.SourceRange.Start), GetSourceLocation(argCursor.SourceRange.End));
                var meta = argCursor.Spelling.CString;
                switch (argCursor.Kind)
                {
                    case CXCursorKind.CXCursor_VisibilityAttr:
						//...
                        break;
                    case CXCursorKind.CXCursor_AnnotateAttr:
                        //...
                        break;
                    case CXCursorKind.CXCursor_AlignedAttr:
                        //...
                        break;
                    //...
                    default:
                        break;
                }

                return CXChildVisitResult.CXChildVisit_Continue;

            }, new CXClientData((IntPtr)0));
            return collectAttributes;
        }
```
With the existing features of `ClangSharp`, such as `visibility attribute`, can be efficiently handled here. Note that here the use of `AnnotateAttr` and `meta attribute` will be introduced. It is also the key to our high-performance `meta attribute` usage. We can directly access the relevant `cursor` on `libclang`'s `AST`, thus avoiding handling related data at the high performance-cost `token` level. 

---
### 3.2 `AttributeKind.TokenAttribute`
&emsp;&emsp;For the original `attribute` implemented based on `token` parsing, for compatibility with older versions, it has temporarily been moved from the original `Attributes` property to the `TokenAttributes` property. The new `CxxSystemAttribute` and `AnnotateAttribute` are stored in the original `Attributes` property. You can refer to the relevant test cases to understand their specific usage.  

---
### 3.3 `AttributeKind.AnnotateAttribute`
&emsp;&emsp;We need a mechanism to implement `meta attribute` that bypasses `token` parsing. Here we cleverly use the `annotate`

 attribute to achieve this. From the several new built-in macros, we can see how it works:
```cs
            //Add a default macro here for CppAst.Net
            Defines = new List<string>() { 
                "__cppast_run__",                                     //Help us for identify the CppAst.Net handler
                @"__cppast_impl(...)=__attribute__((annotate(#__VA_ARGS__)))",          //Help us for use annotate attribute convenience
                @"__cppast(...)=__cppast_impl(__VA_ARGS__)",                         //Add a macro wrapper here, so the argument with macro can be handle right for compiler.
            };
```
> [!note] 
> These three system macros will not be parsed into `CppMacro` and added to the final parsing result to avoid polluting the output.

In the end, we simply convert the variable argument `__VA_ARGS__` to a string and use `__attribute__((annotate(???)))` to inject information. Thus, if we, like the test code, add the following at the right place:
```cpp
#if !defined(__cppast)
#define __cppast(...)
#endif
```
When the code is parsed by `cppast.net`, the relevant input will be correctly identified and read as an `annotate attribute`. In non-`cppast.net` scenarios, the data injected in `__cppast()` will be correctly ignored to avoid interfering with the actual compilation and execution of the code. In this way, we indirectly achieve the purpose of injecting and reading `meta attribute`.

For the macro case:
```cpp
#if !defined(__cppast)
#define __cppast(...)
#endif

#define UUID() 12345

__cppast(id=UUID(), desc=""a function with macro"")
void TestFunc()
{
}
```
Relevant test code:
```cs
//annotate attribute support on namespace
var func = compilation.Functions[0];
                    Assert.AreEqual(1, func.Attributes.Count);
                    Assert.AreEqual(func.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(func.Attributes[0].Arguments, "id=12345, desc=\"a function with macro\"");
```
Because we did a `wrapper` packaging when defining `__cppast`, we find that macros also work well in `meta attribute` state.

As for the case of `outline attribute`, like `Function`, `Field`, it can support well, and even you can define multiple `attributes` on an object, which is also legal:
```cpp
__cppast(id = 1)
__cppast(name = "x")
__cppast(desc = "???")
float x;
```

---
## 4. Conclusion
&emsp;&emsp;This article mainly introduces the new `attributes` supported by `cppast.net`, which are mainly divided into three categories:
1. CxxSystemAttribute
2. TokenAttribute
3. AnnotateAttribute
We recommend using `CxxSystemAttribute` and `AnnotateAttribute`, which do not require switch control. The existence of `TokenAttribute` is mainly for compatibility with old implementations. The related attributes have been moved into a separate `TokenAttributes` to distinguish from the first two. And `CppParserOptions` corresponding switch is adjusted to `ParseTokenAttributes`. Due to performance and usage limitations, it is not recommended to continue to use it.