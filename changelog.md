# Changelog

## 0.8.0-alpha-001 (2 May 2020)
- Use CLangSharp - libclang 10.0

## 0.7.3 (8 Mar 2020)
- Optimize attribute parsing.

## 0.7.2 (27 Feb 2020)
- Make attribute parsing optional by default.

## 0.7.1 (15 Feb 2020)
- Fix infinite loop issue caused by attribute parsing.

## 0.7.0 (12 Feb 2020)
- Add support TypeAliases as Typedefs
- Add the support for skipping the parsing of SystemInclude Headers
- Improve Attribute parsing

## 0.6.0 (08 Sep 2019)
- Add CppClass.IsAnonymous
- Add comments to CppMacro

## 0.5.9 (08 Sep 2019)
- Add CppField.IsAnonymous
- Add bitfield information to CppField
- Fix enum canonical type to return the integer type
- Add more flags to CppFunctionFlags to detect a C++ method/inline/constructor/destructor 

## 0.5.8 (16 July 2019)
- Add SizeOf

## 0.5.7 (18 Jun 2019)
- Fix the type of fields with function pointers

## 0.5.6 (16 Jun 2019)
- Fix tokenization with consecutive identifiers/keywords

## 0.5.5 (15 Jun 2019)
- Add `CppGlobalDeclarationContainer.FindByName` methods

## 0.5.4 (15 Jun 2019)
- Add CppComment.ChildrenToString

## 0.5.3 (14 Jun 2019)
- Add CppFunction.LinkageKind and CppLinkageKind

## 0.5.2 (14 Jun 2019)
- Use empty string for anonymous name (e.g structs, parameter names) instead of filling with a predefined name

## 0.5.1 (13 Jun 2019)
- Make CppField.Type and CppParameter.Type writeable

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
