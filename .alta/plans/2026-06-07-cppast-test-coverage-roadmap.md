# CppAst C/C++ test coverage roadmap

- Status: Complete
- Plan file: `.alta/plans/2026-06-07-cppast-test-coverage-roadmap.md`
- Created: 2026-06-07
- Task: Deeply review the current CppAst.NET test suite and plan higher-value C and C++ header coverage improvements.
- Git: `.alta/` is not ignored by `.gitignore`; commit this plan with the related test/fix work unless the user says otherwise.

## Objective

- Improve confidence in C and C++ header parsing by adding deterministic, high-signal tests that exercise currently weak parser/model paths.
- Prioritize C/C++ declarations, types, templates, expressions/defaults, preprocessor/comments/includes, parser options, diagnostics, and model invariants.
- Do not add coverage tooling, dependencies, broad refactors, or platform-SDK-dependent tests unless they are guarded/skipped and justified.
- If new tests expose real parser/model bugs, add focused regressions and fix them in separate, issue-referenced commits where applicable.

## Context and evidence

- Test infrastructure: `src/CppAst.Tests/InlineTestBase.cs:9-31` parses each inline snippet both in memory and from a temporary header file, so new inline tests should generally use `ParseAssert(...)` for both paths.
- Tracked test inventory currently has about 104 `[Test]` methods across `src/CppAst.Tests/**/*.cs`; the last full local run after the previous commit passed as `110 passed, 3 skipped` because pre-existing untracked tests are also compiled locally.
- Preserve pre-existing untracked local artifacts and do not edit them: `src/CppAst.Tests/DoNotCommit.cs`, `src/CppAst.Tests/Properties/`, `src/CppAst.Tests/TestIncludes.cs`, `src/CppAst.Tests/test.h`.
- Stronger existing areas include function signatures/body spans in `TestFunctions.cs`, Objective-C in `TestObjectiveC.cs`, attributes under `AttributesTest/`, basic templates/types in `TestTypes.cs`, namespace lookup in `TestNamespaces.cs`, and recent regressions in `TestRegressionIssues.cs` plus `TestCoverageGaps.cs`.
- Weak or shallow areas by current tests:
  - `src/CppAst.Tests/TestGlobalVariables.cs` has one simple global variable test.
  - `src/CppAst.Tests/TestMacros.cs` covers only simple macro definitions and token kinds.
  - `src/CppAst.Tests/TestComments.cs` has three comment extraction scenarios and little field/typedef/parameter/trailing coverage.
  - `src/CppAst.Tests/TestEnums.cs` has one basic unscoped/scoped enum test.
  - `src/CppAst.Tests/TestTypedefs.cs` and `src/CppAst.Tests/TestTypeAliases.cs` overlap on primitive aliases and typedef squashing, with limited C-vs-C++ differentiation.
  - `src/CppAst.Tests/TestMethods.cs` has only basic method/final coverage for C++ methods.
- Parser/model surface to cover is broader than the tests: `src/CppAst/CppModelBuilder.cs` handles many `CXCursorKind` declaration/member/expression branches (`Namespace`, `EnumDecl`, class/struct/union, templates, fields/vars, functions/constructors/destructors/methods, using directives, base specifiers, macros, inclusions, expressions) and many `CXTypeKind` cases (`Pointer`, references, records/enums/functions, arrays, attributed/auto/unexposed, 128-bit/floating variants).
- Parser options needing direct tests are visible in `src/CppAst/CppParserOptions.cs:20-40` and `:73-108`: `ParserKind`, include folders, defines, `ParseMacros`, `ParseComments`, `ParseSystemIncludes`, token/comment attributes, `ParseFunctionBodies`, targets, `PreHeaderText`, and `PostHeaderText`.
- Parser behavior worth asserting is in `src/CppAst/CppParser.cs:97-136` (language kind, target, comments, macro/preprocessing flags), `:156-177` (pre/post headers and files), and `:198-235` (diagnostics and abort-on-error behavior).
- Relevant open issue evidence from GitHub HTML pages:
  - `#117` duplicate inclusion directives can throw when adding `CppInclusionDirective`.
  - `#119` reports questionable `size_t` mapping on Windows; expected behavior should be confirmed against target ABI before fixing.
  - `#106` and `#116` report parent/container null exceptions while parsing MSVC/STL/system headers.
  - `#88` asks about comments on function parameters; `CppParameter` inherits comment support via `CppDeclaration`, but current tests do not cover whether libclang supplies these comments.
  - `#86` asks about friend `operator==` declarations inside classes; `CppModelBuilder` does not show explicit `CXCursor_FriendDecl` handling in the reviewed cursor list.
  - `#95` requested function-body parsing; the suite now has body-span tests but not body-content/macro-selector-style scenarios.
  - `#121` final specifier is already covered by `TestMethods.TestFinal`, so it is not a top-priority new gap.

## Assumptions and open decisions

- Assumption: Start with deterministic tracked tests that run on Windows without requiring installed Visual Studio headers beyond what the current environment already has.
- Assumption: C and C++ are the requested focus; Objective-C tests are not expanded unless a shared parser-option or model-invariant case naturally touches them.
- Assumption: Use focused new test files rather than growing large monolithic files, while leaving small existing files intact unless adding an obviously related case.
- Assumption: Behavior changes require tests and, for public API changes, XML docs and readme/doc updates per `AGENTS.md`.
- Resolved: User approved implementing the full roadmap in execution mode.
- Resolved: Do not change `size_t` mapping behavior for `#119`; treat it as characterization/target-ABI coverage unless a separate explicit decision is made.

## Design notes

- Prefer small inline headers that encode real C/C++ constructs rather than testing only counts; assert model shape, `CppType` graph, display names, flags, source spans, parent/container relationships, and diagnostics.
- Use `CppParserOptions { ParserKind = CppParserKind.C, AdditionalArguments = { "-std=c11" } }` for C-only syntax and `-std=c++17` for modern C++ tests unless a feature requires a different standard.
- Use temporary files under `TestContext.CurrentContext.WorkDirectory` for include/import tests instead of adding loose untracked headers beside the project.
- Avoid assertions that depend on platform SDK layouts. For MSVC/STL issue reproduction, first reduce to a synthetic header; only add environment-dependent tests behind explicit OS/path guards and `Assert.Ignore`.
- When a proposed test exposes unsupported behavior rather than a quick bug fix, either fix the model intentionally or add a characterization test/documentation update that states the limitation.

## Risks and challenges

- Some C/C++ constructs are represented by libclang as unexposed/attributed/dependent types, so tests should assert stable public behavior rather than fragile cursor internals.
- System-header parsing issues (`#106`, `#116`) may depend on local Visual Studio/Windows SDK versions; minimize before committing a regression.
- `size_t` expectations vary by target triple/ABI (`x86` vs `x86_64`, MSVC vs non-MSVC); confirm expected sizes/display names before declaring `#119` a bug.
- Function parameter comments (`#88`) may not be recoverable from libclang in all placements; discovery should avoid overpromising API support.
- Pre-existing untracked test files are compiled by the SDK-style test project in the local workspace; do not edit/delete them, and mention them if full test results differ from tracked baseline.

## Implementation checklist

- [x] Before editing, run `git status --short` from `C:\code\CppAst` and confirm only the saved plan plus known pre-existing untracked files are present.
- [x] Add `src/CppAst.Tests/TestParserOptionsAndDiagnostics.cs` covering `ParserKind` C vs C++, `Defines`, `PreHeaderText`, `PostHeaderText`, `ParseMacros` off/on, `ParseComments` false, syntax-error diagnostics (`HasErrors`, abort warning, no thrown exception), and `ConfigureForWindowsMsvc` smoke behavior.
- [x] Add `src/CppAst.Tests/TestIncludesAdvanced.cs` using temporary headers to cover duplicate includes/`#pragma once` and nested includes (`#117`), include folder resolution, `InclusionDirectives` source/file assertions, and `ParseSystemIncludes` filtering with synthetic system include folders.
- [x] Add `src/CppAst.Tests/TestModelInvariants.cs` with a mixed header and assertions for non-null `Parent` on declarations, stable `Children()` enumeration, source spans/source files, `FindByName`, `FindListByName`, `FindByFullName`, namespace/inline namespace lookup, and no duplicate container ownership exceptions.
- [x] Add `src/CppAst.Tests/TestCDeclarationsAdvanced.cs` focused on `ParserKind.C`: storage qualifiers (`extern`, `static`, C11 `_Thread_local` if supported), top-level `const`/`volatile`/`restrict` pointer forms, named and anonymous `struct`/`union`, nested anonymous unions, bitfields including zero-width/alignment cases, flexible array members, incomplete arrays, and C function pointer typedefs/parameters.
- [x] Expand enum coverage in `TestEnums.cs` or a new C/C++ enum file with negative values, implicit values after explicit values, bit-shift/OR expressions, large unsigned/64-bit underlying values, anonymous typedef enums, fixed underlying types, and scoped enum references in initializers.
- [x] Expand global declaration coverage in `TestGlobalVariables.cs` with arrays, pointers, function pointers, string/char initializers, enum initializers, static/extern storage, `constexpr`/`inline` variables where C++17 is enabled, and source-location assertions.
- [x] Differentiate `TestTypedefs.cs` and `TestTypeAliases.cs`: keep C typedef tests in `TestTypedefs` (anonymous typedef squashing, forward declarations, function-pointer/array-pointer typedefs, enum typedefs) and C++ `using` tests in `TestTypeAliases` (template aliases, alias chains, namespace aliases/types, dependent aliases).
- [x] Add `src/CppAst.Tests/TestCppClassesAdvanced.cs` covering class-vs-struct default visibility, multiple and virtual inheritance with access specifiers, nested classes/enums/typedefs, constructors/destructors including defaulted/deleted/explicit/noexcept forms, overloaded operators, conversion operators, friend functions/operators (`#86`), and out-of-line definitions in namespaces.
- [x] Add `src/CppAst.Tests/TestTemplatesAdvanced.cs` covering type, non-type, default, template-template, and variadic template parameters; member templates; full and partial specialization; dependent qualified names; nested template arguments; aliases to specializations; and display/full-name stability under `-std=c++17`.
- [x] Expand `TestExpressions.cs` with conditional expressions, casts (`static_cast`, C-style, functional), bool/nullptr/string/char literals, enum/scoped-enum references, initializer lists for arrays/classes, macro-backed constants, default parameters with references/pointers/strings, and ToString/value assertions.
- [x] Add `src/CppAst.Tests/TestPreprocessorAdvanced.cs` for function-like/variadic macros, stringizing (`#`), token pasting (`##`), multiline macro continuations, comments inside macro values, macro enable/disable behavior, and include-guard-like patterns.
- [x] Add `src/CppAst.Tests/TestCommentsAdvanced.cs` for block comments, Doxygen `///`/`/** */`, `@brief`, `@param`, `@return`, comments on fields/typedefs/enums/enum items/classes/namespaces, trailing comments, and a focused `#88` parameter-comment discovery/regression case.
- [x] Add issue-focused regression tests before production fixes when feasible: duplicate inclusion (`#117`), friend operator (`#86`), parameter comments (`#88`), reduced MSVC parent-null repro (`#106`/`#116`), and target-specific `size_t` characterization (`#119`) without changing current `size_t` behavior.
- [x] If any issue-focused test fails, implement the smallest corresponding fix in `src/CppAst/` with public XML docs/doc updates if public behavior changes; fixes were kept in the validated roadmap change because the tests and fixes are interdependent.
- [x] Keep all new tests tracked under `src/CppAst.Tests/`; do not modify `DoNotCommit.cs`, `TestIncludes.cs`, `Properties/`, or `test.h` unless the user explicitly reassigns ownership.

## Verification checklist

- [x] From `C:\code\CppAst\src`, run targeted tests while developing, for example `dotnet test -c Release --no-restore --filter "FullyQualifiedName~CppAst.Tests.TestParserOptionsAndDiagnostics"`.
- [x] From `C:\code\CppAst\src`, run each new/expanded test class with a targeted `--filter` before the full suite.
- [x] From `C:\code\CppAst\src`, run `dotnet test -c Release --no-restore` after each coherent phase.
- [x] If a full run includes the known untracked local tests, report that explicitly and distinguish failures in tracked new tests from failures in pre-existing untracked files.
- [x] Review `git diff -- src/CppAst.Tests src/CppAst readme.md doc .alta/plans/2026-06-07-cppast-test-coverage-roadmap.md` for focused changes, no unrelated formatting churn, XML docs for any public API changes, and no accidental edits to untracked user files.
- [x] If public behavior changes, update `readme.md` or `doc/**/*.md` where the changed behavior is documented (no existing docs covered the fixed internals).

## Handoff notes

- User approved executing the full roadmap in Default mode; still keep work organized into separate logical commits/slices so failures can be bisected.
- Do not change `size_t` mapping behavior for `#119`; add characterization/target-ABI coverage only unless the user later asks for a behavior change.
- Treat newly failing tests as design feedback: do not weaken assertions just to pass; either fix the parser/model or record a clear limitation.
- Keep commits focused and use the repository's dotnet-releaser prefix rules from `AGENTS.md`; issue-linked fixes should include the issue number in the subject/body as appropriate.

## Execution results

- Completed on 2026-06-07.
- Added deterministic roadmap coverage across parser options/diagnostics, includes, invariants, C declarations, enums, globals, typedefs/type aliases, C++ classes, templates, expressions, preprocessor macros, comments, and open-issue characterizations.
- Implemented focused parser/model fixes for macro collection gating, token-attribute macro compatibility, parameter `@param` comment assignment, friend/conversion function cursors, and variadic template-pack specialization arguments.
- Preserved the known pre-existing untracked local files: `DoNotCommit.cs`, `Properties/`, `TestIncludes.cs`, and `test.h`.
- Verification passed from `src`: targeted roadmap/modified tests and full `dotnet test -c Release --no-restore` (`133 passed`, `3 skipped`).