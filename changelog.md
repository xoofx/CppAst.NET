# Changelog

## 0.5.0 (12 Jun 2019)
- Add support for adding a pre and post header text for parsing
- Add detailed error message with extracted source line for root parser input

## 0.4.0 (08 Jun 2019)
- Add support for parsing parameter names for function prototypes
- Improve ToString of comments with new lines
- Add CppType.GetCanonicalType. Add CppTypeWithElementType
- Add extension method CppAttribute/CppFunction.IsPublicExport
- Make CppFunction and CppFunctionType ICppContainer of CppParameter
- Add ICppDeclaration
- Fix issue with Dictionary key already inserted for typedef (#4)

## 0.3.0 (29 May 2019)
- Add better support for comment with full structured comments (paragraph, block commands, parameters...)
- Fix warning with invalid file/line/column `(0, 0)`
- Remove some unnecessary warnings

## 0.2.0 (27 May 2019)
- Add support for expressions for init value for fields and parameters

## 0.1.3 (27 May 2019)
- Fix exception on ToString if the type is a bool

## 0.1.2 (27 May 2019)
- Change from error to warning in case of non supported features by libclang

## 0.1.1 (27 May 2019)
- Fix NRE with certain C++ template not supported by libclang (#1)
- Fix/improve error messages and source location

## 0.1.0 (27 May 2019)
- Initial version
