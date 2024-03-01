

## 1. `cppast.net 0.12` 对 `attributes` 支持情况
`cppast.net` 原有的对各类 `attribute` 的支持, 包括 `c++17` 的 `meta attribute` 的支持, 由于 `libclang` 本身 `Api` 的限制, 我们需要借助 token 层级的解析, 才能够完成相关的功能实现. 在 `cppast.net 0.12` 版本及以前的实现中, 我们都使用了解析 `token` 的方式来实现相关的功能, 甚至连一些 `libclang` 中能够很好的支持的 `attribute`, 如 `dllexport`, `dllimport` 等, `cppast.net` 大部分时候也是使用 token 解析来实现的. 这样做虽然灵活, 我们始终能够从 `token` 层级尝试解析相关的 `attribute`, 但也带来了一些问题和限制, 社区中比较多反馈的问题:
1. `ParseAttributes()` 耗时巨大, 所以导致了后来的版本中加入了`ParseAttributes` 参数来控制是否解析 `attributes`, 但某些场合, 我们需要依赖 `attributes` 才能完成相关的功能实现. 这显然带来了不便.
2. 对 `meta attribute` - `[[]]` 的解析存在缺陷, 像 `Function` 和 `Field` 上方定义的 `meta attribute`, 在语义层面, 显然是合法的, 但 `cppast.net` 并不能很好的支持这种在对象上方定义的`meta attribute` (这里存在一些例外情况, 像 `namespace`, `class`, `enum` 这些的 `attribute` 声明, attribute定义本身就不能位于上方, 相关的用法编译器会直接报错, 只能在相关的关键字后面, 如 `class [[deprecated]] Abc{};` 这种 ). 
3. `meta attribute` 个别参数使用宏的情况. 因为我们原有的实现是基于 `token` 解析来实现的, 编译期的宏显然不能很好的在这种情况下被正确处理.

---
## 2. 需要用到 `attribute` 的情况简介

---
### 2.1 系统级的 `attribute`
以 `cppast.net` 测试用例中的代码段为例:
```cpp
#ifdef WIN32
#define EXPORT_API __declspec(dllexport)
#else
#define EXPORT_API __attribute__((visibility(""default"")))
#endif
```
 像 `dllexport` 和 `visibility` 这种控制接口可见性的 `attribute`, 我们肯定是比较常用的, 而不仅仅是只有在打开 `ParseAttributes` 的时候才让它能够工作, 我们需要为这些基础的系统属性提供高性能的解决方案, 而且相关的实现应该是不受开关影响的.

---
### 2.2 导出工具等工具额外的信息注入
&emsp;&emsp;我们以下面的类定义为例:
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
为了让 `TestPBMessage` 更好的支持序列化反序列化, 并且有一定的容错性, 我们在原有的结构体定义的基础上添加了一些额外的信息:
1. `TestPBMessage` 的 msgid, 这里直接指定为整形的 `1` 了.
2. x, y, z各自的`id`, 这里直接使用 `1`, `2`, `3`
这样如果我们将 `cppast.net` 用于制作我们的离线处理工具, 我们肯定需要在工具中能够方便的读出 `__cppast()` 所注入的各种对原始代码编译不产生直接影响的 `meta attribute`, 并在适当的场合使用. 但 `cppast.net 0.12` 和之前的版本这部分实现的性能表现比较糟糕, 也存在限制, 像上面这样, 直接将`attribute` 定义在 `Field` 上的情况, 是没有办法支持的.

---
## 3. 新的实现方式和调整
&emsp;&emsp;新的实现方式主要是基于前面提到的当前实现的限制, 以前上一章上提到的几种应用场景来重新思考的, 我们重新将 `attribute` 分为了三类:
1. `AttributeKind.CxxSystemAttribute` - 对应的是 `libclang` 本身就能很好的解析的各类系统 `attribute`, 如上面提到的 `visibility`, 以及 `[[deprecated]]`, `[[noreturn]]` 等. 借助`ClangSharp` 就能够高效的完成对它们的解析和处理, 也就不需要考虑开关的问题了.
2. `AttributeKind.TokenAttribute` - 从名字上我们能猜出, 这对应的是`cppast.net`原来版本中的 `attribute`, 已经标记为 `deprecated` 了, 但`token` 解析始终是一种保底实现机制, 我们会保留相关的 `Tokenizer` 的代码, 在一些 `ClangSharp` 没有办法实现相关功能的情况谨慎的使用它们来实现一些复杂功能.
3. `AttributeKind.AnnotateAttribute` - 用来取代原先基于 `token` 解析实现的 `meta attribute`, 以高性能低限制的实现前面介绍的为类和成员注入的方式.

下面我们简单介绍一下各种类型的 `attribute` 的实现思路和使用介绍.

---
### 3.1 `AttributeKind.CxxSystemAttribute`
&emsp;&emsp;我们新增了一个函数用于处理 `ClangSharp` 本身就支持的各类 `attribute`:
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
利用 `ClangSharp` 已有的功能, 如 `visibility attribute` 等 `attribute` 可以在此处高效的被处理. 注意这里的 `AnnotateAttr`, `meta attribute`部分我们会介绍它的使用, 它也是我们使用高性能`meta attribute`的关键所在, 我们可以直接在 `libclang` 的 `AST` 上直接访问相关的 `cursor`, 这样就避免了在性能开销过高的 `token` 层级去处理相关的数据了. 

### 3.2 `AttributeKind.TokenAttribute`
&emsp;&emsp;原有基于 `token` 解析实现的 `attribute`, 为了老版本的兼容性, 暂时将其由原来的 `Attributes` 属性改为存入 `TokenAttributes` 属性中了, 而新的 `CxxSystemAttribute` 和 `AnnotateAttribute` 则被存入原来的 `Attributes` 属性中, 可以参考相关的测试用例了解具体的使用.  

### 3.3 `AttributeKind.AnnotateAttribute`
&emsp;&emsp;我们需要一种绕开`token` 解析的机制来实现 `meta attribute`, 这里我们巧妙的使用了 `annotate` 属性来完成这一项操作, 从新增的几个内置宏我们可以看出它是如何起作用的:
```cs
            //Add a default macro here for CppAst.Net
            Defines = new List<string>() { 
                "__cppast_run__",                                     //Help us for identify the CppAst.Net handler
                @"__cppast_impl(...)=__attribute__((annotate(#__VA_ARGS__)))",          //Help us for use annotate attribute convenience
                @"__cppast(...)=__cppast_impl(__VA_ARGS__)",                         //Add a macro wrapper here, so the argument with macro can be handle right for compiler.
            };
```
> [!note] 
> 此处的三个系统宏不会被解析为 `CppMacro` 加入最终的解析结果中, 避免污染输出结果. 

最终我们其实只是将 `__VA_ARGS__` 这个可变参数转为字符串并利用 `__attribute__((annotate(???)))` 来完成信息的注入, 这样如果我们像测试代码中一样, 在合适的地方加入:
```cpp
#if !defined(__cppast)
#define __cppast(...)
#endif
```
当代码被 `cppast.net` 解析时, 相关的输入会被当成`annotate attribute` 被正确的识别并读取, 而在非 `cppast.net` 的情况下, 代码也能正确的忽略`__cppast()` 中注入的数据, 避免干扰实际代码的编译执行, 这样我们就间接的完成了 `meta attribute` 注入和读取的目的了.

对于宏的情况:
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
相关的测试代码:
```cs
//annotate attribute support on namespace
var func = compilation.Functions[0];
                    Assert.AreEqual(1, func.Attributes.Count);
                    Assert.AreEqual(func.Attributes[0].Kind, AttributeKind.AnnotateAttribute);
                    Assert.AreEqual(func.Attributes[0].Arguments, "id=12345, desc=\"a function with macro\"");
```
因为我们定义`__cppast` 的时候, 做了一次 `wrapper` 包装, 我们发现宏也能很好的在`meta attribute` 状态下工作了.

而对于`outline attribute` 的情况, 像`Function`, `Field`, 本身就能很好的支持, 甚至你可以在一个对象上定义多个`attribute`, 同样也是合法的:
```cpp
__cppast(id = 1)
__cppast(name = "x")
__cppast(desc = "???")
float x;
```

## 4. 小结
&emsp;&emsp;本文我们主要介绍了新的 `cppast.net` 支持的 `attribute`, 主要有三类:
1. CxxSystemAttribute
2. TokenAttribute
3. AnnotateAttribute
推荐使用不需要开关控制的 `CxxSystemAttribute` 和 `AnnotateAttribute`, `TokenAttribute` 的存在主要为了兼容老的实现, 相关的属性为了跟前两者区分, 已经移入了单独的 `TokenAttributes`, 并且 `CppParserOptions` 对应的开关调整为了 `ParseTokenAttributes`, 因为性能和使用上存在的限制, 不推荐继续使用.

