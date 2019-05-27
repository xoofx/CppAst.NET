﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ClangSharp;

namespace CppAst
{
    /// <summary>
    /// Internal class used to build the entire C++ model from the libclang representation.
    /// </summary>
    internal class CppModelBuilder
    {
        private readonly CppCompilation _rootCompilation;
        private readonly CppContainerContext _rootContainerContext;
        private readonly Dictionary<string, CppContainerContext> _containers;
        private readonly Dictionary<string, CppType> _typedefs;

        public CppModelBuilder()
        {
            _containers = new Dictionary<string, CppContainerContext>();
            _rootCompilation = new CppCompilation();
            _typedefs = new Dictionary<string, CppType>();
            _rootContainerContext = new CppContainerContext(_rootCompilation);
        }

        public bool AutoSquashTypedef { get; set; }

        public CppCompilation RootCompilation => _rootCompilation;

        public CXChildVisitResult VisitTranslationUnit(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            Debug.Assert(parent.Kind == CXCursorKind.CXCursor_TranslationUnit || parent.Kind == CXCursorKind.CXCursor_UnexposedDecl);

            _rootContainerContext.Container = _rootCompilation;

            if (cursor.Location.IsInSystemHeader)
            {
                _rootContainerContext.Container = _rootCompilation.System;
            }
            return VisitMember(cursor, parent, data);
        }

        private CppContainerContext GetOrCreateDeclarationContainer(CXCursor cursor, CXClientData data)
        {
            CppContainerContext containerContext;
            var fullName = cursor.UnifiedSymbolResolution.CString;
            if (_containers.TryGetValue(fullName, out containerContext))
            {
                return containerContext;
            }

            ICppContainer symbol = null;

            ICppContainer parent = null;
            if (cursor.Kind != CXCursorKind.CXCursor_TranslationUnit && cursor.Kind != CXCursorKind.CXCursor_UnexposedDecl)
            {
                parent = GetOrCreateDeclarationContainer(cursor.SemanticParent, data).Container;
            }

            ICppDeclarationContainer parentDeclarationContainer = (ICppDeclarationContainer) parent;
            var parentGlobalDeclarationContainer = parent as ICppGlobalDeclarationContainer;

            var defaultContainerVisibility = CppVisibility.Default;
            switch (cursor.Kind)
            {
                case CXCursorKind.CXCursor_Namespace:
                    Debug.Assert(parentGlobalDeclarationContainer != null);
                    var ns = new CppNamespace(GetCursorSpelling(cursor));
                    symbol = ns;
                    defaultContainerVisibility = CppVisibility.Default;
                    parentGlobalDeclarationContainer.Namespaces.Add(ns);
                    break;

                case CXCursorKind.CXCursor_EnumDecl:
                    Debug.Assert(parent != null);
                    var cppEnum = new CppEnum(GetCursorSpelling(cursor));
                    parentDeclarationContainer.Enums.Add(cppEnum);
                    symbol = cppEnum;
                    break;

                case CXCursorKind.CXCursor_ClassTemplate:
                case CXCursorKind.CXCursor_ClassDecl:
                case CXCursorKind.CXCursor_StructDecl:
                case CXCursorKind.CXCursor_UnionDecl:
                    Debug.Assert(parent != null);
                    var cppClass = new CppClass(GetCursorSpelling(cursor));
                    parentDeclarationContainer.Classes.Add(cppClass);
                    symbol = cppClass;
                    switch (cursor.Kind)
                    {
                        case CXCursorKind.CXCursor_ClassDecl:
                            cppClass.ClassKind = CppClassKind.Class;
                            break;
                        case CXCursorKind.CXCursor_StructDecl:
                            cppClass.ClassKind = CppClassKind.Struct;
                            break;
                        case CXCursorKind.CXCursor_UnionDecl:
                            cppClass.ClassKind = CppClassKind.Union;
                            break;
                    }

                    if (cursor.Kind == CXCursorKind.CXCursor_ClassTemplate)
                    {
                        cursor.VisitChildren((childCursor, classCursor, clientData) =>
                        {
                            switch (childCursor.Kind)
                            {
                                case CXCursorKind.CXCursor_TemplateTypeParameter:
                                    var parameterTypeName = new CppTemplateParameterType(GetCursorSpelling(childCursor));
                                    cppClass.TemplateParameters.Add(parameterTypeName);
                                    break;
                            }

                            return CXChildVisitResult.CXChildVisit_Continue;
                        }, data);
                    }

                    defaultContainerVisibility = cursor.Kind == CXCursorKind.CXCursor_ClassDecl ? CppVisibility.Private :  CppVisibility.Public;
                    break;
                case CXCursorKind.CXCursor_TranslationUnit:
                case CXCursorKind.CXCursor_UnexposedDecl:
                    return _rootContainerContext;
                default:
                    Unhandled(cursor);
                    break;
            }

            containerContext = new CppContainerContext(symbol) {CurrentVisibility = defaultContainerVisibility};

            _containers.Add(fullName, containerContext);
            return containerContext;
        }

        private TCppElement GetOrCreateDeclarationContainer<TCppElement>(CXCursor cursor, CXClientData data, out CppContainerContext context) where TCppElement : CppElement, ICppContainer
        {
            context = GetOrCreateDeclarationContainer(cursor, data);
            if (context.Container is TCppElement typedCppElement)
            {
                return typedCppElement;
            }
            throw new InvalidOperationException($"The element `{context.Container}` doesn't match the expected type `{typeof(TCppElement)}");
        }

        private CppNamespace VisitNamespace(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            // Create the container if not already created
            var ns = GetOrCreateDeclarationContainer<CppNamespace>(cursor, data, out var context);
            cursor.VisitChildren(VisitMember, data);
            return ns;
        }

        private CppClass VisitClassDecl(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            var cppStruct = GetOrCreateDeclarationContainer<CppClass>(cursor, data, out var context);
            if (cursor.IsDefinition && !cppStruct.IsDefinition)
            {
                cppStruct.IsDefinition = true;
                context.IsChildrenVisited = true;
                cursor.VisitChildren(VisitMember, data);
            }
            return cppStruct;
        }

        private CXChildVisitResult VisitMember(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            CppElement element = null;

            switch (cursor.Kind)
            {
                case CXCursorKind.CXCursor_FieldDecl:
                case CXCursorKind.CXCursor_VarDecl:
                {
                    var containerContext = GetOrCreateDeclarationContainer(parent, data);
                    element = VisitFieldOrVariable(containerContext, cursor, parent, data);
                    break;
                }

                case CXCursorKind.CXCursor_EnumConstantDecl:
                {
                    var containerContext = GetOrCreateDeclarationContainer(parent, data);
                    var cppEnum = (CppEnum) containerContext.Container;
                    var enumItem = new CppEnumItem(GetCursorSpelling(cursor), cursor.EnumConstantDeclValue);
                    cppEnum.Items.Add(enumItem);
                    element = enumItem;
                    break;
                }

                case CXCursorKind.CXCursor_Namespace:
                    element = VisitNamespace(cursor, parent, data);
                    break;

                case CXCursorKind.CXCursor_ClassTemplate:
                case CXCursorKind.CXCursor_ClassDecl:
                case CXCursorKind.CXCursor_StructDecl:
                case CXCursorKind.CXCursor_UnionDecl:
                    element = VisitClassDecl(cursor, parent, data);
                    break;

                case CXCursorKind.CXCursor_EnumDecl:
                    element = VisitEnumDecl(cursor, parent, data);
                    break;

                case CXCursorKind.CXCursor_TypedefDecl:
                    element = VisitTypeDefDecl(cursor, parent, data);
                    break;

                case CXCursorKind.CXCursor_FunctionTemplate:
                case CXCursorKind.CXCursor_FunctionDecl:
                case CXCursorKind.CXCursor_Constructor:
                case CXCursorKind.CXCursor_CXXMethod:
                    element = VisitFunctionDecl(cursor, parent, data);
                    break;

                case CXCursorKind.CXCursor_UsingDirective:
                    // We don't visit directive
                    break;
                case CXCursorKind.CXCursor_UnexposedDecl:
                    return CXChildVisitResult.CXChildVisit_Recurse;

                case CXCursorKind.CXCursor_CXXBaseSpecifier:
                {
                    var cppClass = (CppClass)GetOrCreateDeclarationContainer(parent, data).Container;
                    var baseType = GetCppType(cursor.Type.Declaration, cursor.Type, cursor, data);
                    var cppBaseType = new CppBaseType(baseType)
                    {
                        Visibility = GetVisibility(cursor.CXXAccessSpecifier),
                        IsVirtual = cursor.IsVirtualBase
                    };
                    cppClass.BaseTypes.Add(cppBaseType);
                    break;
                }

                case CXCursorKind.CXCursor_CXXAccessSpecifier:
                {
                    var containerContext = GetOrCreateDeclarationContainer(parent, data);
                    containerContext.CurrentVisibility = GetVisibility(cursor.CXXAccessSpecifier);
                }

                    break;

                case CXCursorKind.CXCursor_MacroDefinition:
                    element = ParseMacro(cursor);
                    break;
                case CXCursorKind.CXCursor_MacroExpansion:
                    break;

                default:
                    WarningUnhandled(cursor, parent);
                    break;
            }
            // Assign a comment
            var comment = cursor.BriefCommentText.CString;
            if (element != null && comment != null && element.Comment == null)
            {
                element.Comment = comment;
            }

            if (element != null)
            {
                AssignSourceSpan(cursor, element);
            }

            return CXChildVisitResult.CXChildVisit_Continue;
        }

        private CppMacro ParseMacro(CXCursor cursor)
        {
            // TODO: reuse internal class Tokenizer

            // As we don't have an API to check macros, we are 
            var range = cursor.Extent;
            CXToken[] tokens = null;

            var tu = cursor.TranslationUnit;

            tu.Tokenize(range, out tokens);

            var name = GetCursorSpelling(cursor);
            var cppMacro = new CppMacro(name);

            uint previousLine = 0;
            uint previousColumn = 0;
            bool parsingMacroParameters = false;
            List<string> macroParameters = null;

            // Loop decoding tokens for the value
            // We need to parse 
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                var tokenRange = token.GetExtent(tu);
                tokenRange.Start.GetFileLocation(out var file, out var line,  out var column, out var offset);
                var tokenStr = token.GetSpelling(tu).CString;

                // If we are parsing the token right after the MACRO name token
                // if the `(` is right after the name without 
                if (i == 1 && tokenStr == "(" && (previousLine == line && previousColumn == column))
                {
                    parsingMacroParameters = true;
                    macroParameters = new List<string>();
                }

                tokenRange.End.GetFileLocation(out file, out previousLine, out previousColumn, out offset);

                if (parsingMacroParameters)
                {
                    if (tokenStr == ")")
                    {
                        parsingMacroParameters = false;
                    }
                    else if (token.Kind != CXTokenKind.CXToken_Punctuation)
                    {
                        macroParameters.Add(tokenStr);
                    }
                }
                else if (i > 0)
                {
                    CppTokenKind cppTokenKind = 0;
                    switch (token.Kind)
                    {
                        case CXTokenKind.CXToken_Punctuation:
                            cppTokenKind = CppTokenKind.Punctuation;
                            break;
                        case CXTokenKind.CXToken_Keyword:
                            cppTokenKind = CppTokenKind.Keyword;
                            break;
                        case CXTokenKind.CXToken_Identifier:
                            cppTokenKind = CppTokenKind.Identifier;
                            break;
                        case CXTokenKind.CXToken_Literal:
                            cppTokenKind = CppTokenKind.Literal;
                            break;
                        case CXTokenKind.CXToken_Comment:
                            cppTokenKind = CppTokenKind.Comment;
                            break;
                        default:
                            _rootCompilation.Diagnostics.Warning($"Token kind {tokenStr} is not supported for macros", GetSourceLocation(token.GetLocation(tu)));
                            break;
                    }

                    var cppToken = new CppToken(cppTokenKind, tokenStr)
                    {
                        Span = new CppSourceSpan(GetSourceLocation(tokenRange.Start), GetSourceLocation(tokenRange.End))
                    };

                    cppMacro.Tokens.Add(cppToken);
                }
            }

            // Update the value from the tokens
            cppMacro.UpdateValueFromTokens();
            cppMacro.Parameters = macroParameters;

            var globalContainer = (CppGlobalDeclarationContainer) _rootContainerContext.DeclarationContainer;
            globalContainer.Macros.Add(cppMacro);

            return cppMacro;
        }

        private static CppVisibility GetVisibility(CX_CXXAccessSpecifier accessSpecifier)
        {
            switch (accessSpecifier)
            {
                case CX_CXXAccessSpecifier.CX_CXXProtected:
                    return CppVisibility.Protected;
                case CX_CXXAccessSpecifier.CX_CXXPrivate:
                    return CppVisibility.Private;
                default:
                    return CppVisibility.Public;
            }
        }

        private static void AssignSourceSpan(CXCursor cursor, CppElement element)
        {
            var start = cursor.Extent.Start;
            var end = cursor.Extent.End;
            element.Span = new CppSourceSpan(GetSourceLocation(start), GetSourceLocation(end));
        }

        public static CppSourceLocation GetSourceLocation(CXSourceLocation start)
        {
            CXFile file;
            uint line;
            uint column;
            uint offset;
            start.GetFileLocation(out file, out line, out column, out offset);
            return new CppSourceLocation(file.Name.CString, (int)offset, (int)line, (int)column);
        }

        private CppField VisitFieldOrVariable(CppContainerContext containerContext, CXCursor cursor, CXCursor parent, CXClientData data)
        {
            var fieldName = GetCursorSpelling(cursor);
            var type = GetCppType(cursor.Type.Declaration, cursor.Type, parent, data);

            var cppField = new CppField(type, fieldName)
            {
                Visibility = containerContext.CurrentVisibility,
                StorageQualifier = GetStorageQualifier(cursor)
            };
            containerContext.DeclarationContainer.Fields.Add(cppField);

            cppField.Attributes = ParseAttributes(cursor);

            if (cursor.Kind == CXCursorKind.CXCursor_VarDecl)
            {
                var resultEval = clang.Cursor_Evaluate(cursor);

                switch (resultEval.Kind)
                {
                    case CXEvalResultKind.CXEval_Int:
                        cppField.DefaultValue = new CppValue(resultEval.AsLongLong);
                        break;
                    case CXEvalResultKind.CXEval_Float:
                        cppField.DefaultValue = new CppValue(resultEval.AsDouble);
                        break;
                    case CXEvalResultKind.CXEval_ObjCStrLiteral:
                    case CXEvalResultKind.CXEval_StrLiteral:
                    case CXEvalResultKind.CXEval_CFStr:
                        cppField.DefaultValue = new CppValue(resultEval.AsStr);
                        break;
                    case CXEvalResultKind.CXEval_UnExposed:
                        break;
                    default:
                        _rootCompilation.Diagnostics.Warning($"Not supported field default value {cursor}", GetSourceLocation(cursor.Location));
                        break;
                }
            }

            return cppField;
        }

        private CppEnum VisitEnumDecl(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            var cppEnum = GetOrCreateDeclarationContainer<CppEnum>(cursor, data, out var context);
            if (cursor.IsDefinition && !context.IsChildrenVisited)
            {
                var integralType = cursor.EnumDecl_IntegerType;
                cppEnum.IntegerType = GetCppType(integralType.Declaration, integralType, cursor, data);
                cppEnum.IsScoped = cursor.EnumDecl_IsScoped;

                context.IsChildrenVisited = true;
                cursor.VisitChildren(VisitMember, data);
            }
            return cppEnum;
        }

        private static CppStorageQualifier GetStorageQualifier(CXCursor cursor)
        {
            switch (cursor.StorageClass)
            {
                case CX_StorageClass.CX_SC_Extern:
                case CX_StorageClass.CX_SC_PrivateExtern:
                    return CppStorageQualifier.Extern;
                case CX_StorageClass.CX_SC_Static:
                    return CppStorageQualifier.Static;
            }

            return CppStorageQualifier.None;
        }


        private CppFunction VisitFunctionDecl(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            var contextContainer = GetOrCreateDeclarationContainer(cursor.SemanticParent, data);
            var container = contextContainer.DeclarationContainer;

            if (container == null)
            {
                return null;
            }

            var cppFunction = new CppFunction(GetCursorSpelling(cursor))
            {
                Visibility = contextContainer.CurrentVisibility,
                StorageQualifier = GetStorageQualifier(cursor)
            };

            if (cursor.Kind == CXCursorKind.CXCursor_Constructor)
            {
                var cppClass = (CppClass) container;
                cppFunction.IsConstructor = true;
                cppClass.Constructors.Add(cppFunction);
            }
            else
            {
                container.Functions.Add(cppFunction);
            }

            if (cursor.CXXMethod_IsConst)
            {
                cppFunction.Flags |= CppFunctionFlags.Const;
            }
            if (cursor.CXXMethod_IsDefaulted)
            {
                cppFunction.Flags |= CppFunctionFlags.Defaulted;
            }
            if (cursor.CXXMethod_IsPureVirtual)
            {
                cppFunction.Flags |= CppFunctionFlags.Pure | CppFunctionFlags.Virtual;
            }

            // Gets the return type
            var returnType = GetCppType(cursor.ResultType.Declaration, cursor.ResultType, cursor, data);
            cppFunction.ReturnType = returnType;

            cppFunction.Attributes = ParseFunctionAttributes(cursor, cppFunction.Name);
            cppFunction.CallingConvention = GetCallingConvention(cursor.Type);

            int i = 0;
            cursor.VisitChildren((argCursor, functionCursor, clientData) =>
            {
                switch (argCursor.Kind)
                {
                    case CXCursorKind.CXCursor_ParmDecl:
                        var argName = GetCursorSpelling(argCursor);

                        if (string.IsNullOrEmpty(argName))
                            argName = "__arg" + i;

                        var parameter = new CppParameter(GetCppType(argCursor.Type.Declaration, argCursor.Type, functionCursor, clientData), argName);

                        cppFunction.Parameters.Add(parameter);

                        i++;
                        break;

                    default:
                        // Attributes should be parsed by ParseAttributes()
                        if (!(argCursor.Kind >= CXCursorKind.CXCursor_FirstAttr && argCursor.Kind <= CXCursorKind.CXCursor_LastAttr))
                        {
                            WarningUnhandled(cursor, parent);
                        }
                        break;
                }

                return CXChildVisitResult.CXChildVisit_Continue;

            }, data);

            return cppFunction;
        }

        private CppCallingConvention GetCallingConvention(CXType type)
        {
            var callingConv = type.FunctionTypeCallingConv;
            switch (callingConv)
            {
                case CXCallingConv.CXCallingConv_Default:
                    return CppCallingConvention.Default;
                case CXCallingConv.CXCallingConv_C:
                    return CppCallingConvention.C;
                case CXCallingConv.CXCallingConv_X86StdCall:
                    return CppCallingConvention.X86StdCall;
                case CXCallingConv.CXCallingConv_X86FastCall:
                    return CppCallingConvention.X86FastCall;
                case CXCallingConv.CXCallingConv_X86ThisCall:
                    return CppCallingConvention.X86ThisCall;
                case CXCallingConv.CXCallingConv_X86Pascal:
                    return CppCallingConvention.X86Pascal;
                case CXCallingConv.CXCallingConv_AAPCS:
                    return CppCallingConvention.AAPCS;
                case CXCallingConv.CXCallingConv_AAPCS_VFP:
                    return CppCallingConvention.AAPCS_VFP;
                case CXCallingConv.CXCallingConv_X86RegCall:
                    return CppCallingConvention.X86RegCall;
                case CXCallingConv.CXCallingConv_IntelOclBicc:
                    return CppCallingConvention.IntelOclBicc;
                case CXCallingConv.CXCallingConv_Win64:
                    return CppCallingConvention.Win64;
                case CXCallingConv.CXCallingConv_X86_64SysV:
                    return CppCallingConvention.X86_64SysV;
                case CXCallingConv.CXCallingConv_X86VectorCall:
                    return CppCallingConvention.X86VectorCall;
                case CXCallingConv.CXCallingConv_Swift:
                    return CppCallingConvention.Swift;
                case CXCallingConv.CXCallingConv_PreserveMost:
                    return CppCallingConvention.PreserveMost;
                case CXCallingConv.CXCallingConv_PreserveAll:
                    return CppCallingConvention.PreserveAll;
                case CXCallingConv.CXCallingConv_AArch64VectorCall:
                    return CppCallingConvention.AArch64VectorCall;
                case CXCallingConv.CXCallingConv_Invalid:
                    return CppCallingConvention.Invalid;
                case CXCallingConv.CXCallingConv_Unexposed:
                    return CppCallingConvention.Unexposed;
                default:
                    return CppCallingConvention.Unexposed;
            }
        }

        private List<CppAttribute> ParseAttributes(CXCursor cursor)
        {
            var tokenizer = new Tokenizer(cursor);
            var tokenIt = new TokenIterator(tokenizer);

            List<CppAttribute> attributes = null;
            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(tokenIt, ref attributes))
                {
                    continue;
                }
                break;
            }
            return attributes;
        }

        private List<CppAttribute> ParseFunctionAttributes(CXCursor cursor, string functionName)
        {
            // TODO: This function is not 100% correct when parsing tokens up to the function name
            // we assume to find the function name immediately followed by a `(`
            // but some return type parameter could actually interfere with that
            // Ideally we would need to parse more properly return type and skip parenthesis for example
            var tokenizer = new Tokenizer(cursor);
            var tokenIt = new TokenIterator(tokenizer);

            // Parse leading attributes
            List<CppAttribute> attributes = null;
            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(tokenIt, ref attributes))
                {
                    continue;
                }
                break;
            }

            if (!tokenIt.CanPeek)
            {
                return attributes;
            }

            // Find function name (We only support simple function name declaration)
            if (!tokenIt.Find(functionName, "("))
            {
                return attributes;
            }

            Debug.Assert(tokenIt.PeekText() == functionName);
            tokenIt.Next();
            Debug.Assert(tokenIt.PeekText() == "(");
            tokenIt.Next();

            int parentCount = 1;
            while (parentCount > 0 && tokenIt.CanPeek)
            {
                var text = tokenIt.PeekText();
                if (text == "(")
                {
                    parentCount++;
                }
                else if (text == ")")
                {
                    parentCount--;
                }
                tokenIt.Next();
            }

            if (parentCount != 0)
            {
                return attributes;
            }

            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(tokenIt, ref attributes))
                {
                    continue;
                }
                // Skip the token if we can parse it.
                tokenIt.Next();
            }

            return attributes;
        }

        private bool ParseAttributes(TokenIterator tokenIt, ref List<CppAttribute> attributes)
        {
            // Parse C++ attributes
            // [[<attribute>]]
            if (tokenIt.Skip("[", "["))
            {
                CppAttribute attribute;
                while (ParseAttribute(tokenIt, out attribute))
                {
                    if (attributes == null)
                    {
                        attributes = new List<CppAttribute>();
                    }
                    attributes.Add(attribute);

                    tokenIt.Skip(",");
                }

                return tokenIt.Skip("]", "]");
            }
            
            // Parse GCC or clang attributes
            // __attribute__((<attribute>))
            if (tokenIt.Skip("__attribute__", "(", "("))
            {
                CppAttribute attribute;
                while (ParseAttribute(tokenIt, out attribute))
                {
                    if (attributes == null)
                    {
                        attributes = new List<CppAttribute>();
                    }
                    attributes.Add(attribute);

                    tokenIt.Skip(",");
                }

                return tokenIt.Skip(")", ")");
            }

            // Parse MSVC attributes
            // __declspec(<attribute>)
            if (tokenIt.Skip("__declspec", "("))
            {
                CppAttribute attribute;
                while (ParseAttribute(tokenIt, out attribute))
                {
                    if (attributes == null)
                    {
                        attributes = new List<CppAttribute>();
                    }
                    attributes.Add(attribute);

                    tokenIt.Skip(",");
                }
                return tokenIt.Skip(")");
            }

            return false;
        }

        private bool ParseDirectAttribute(CXCursor cursor, ref List<CppAttribute> attributes)
        {
            var tokenizer = new Tokenizer(cursor);
            var tokenIt = new TokenIterator(tokenizer);
            if (ParseAttribute(tokenIt, out var attribute))
            {
                if (attributes == null)
                {
                    attributes = new List<CppAttribute>();
                }
                attributes.Add(attribute);
                return true;
            }

            return false;
        }

        private bool ParseAttribute(TokenIterator tokenIt, out CppAttribute attribute)
        {
            // (identifier ::)? identifier ('(' tokens ')' )? (...)?
            attribute = null;
            var token = tokenIt.Peek();
            if (token == null || !token.Kind.IsIdentifierOrKeyword())
            {
                return false;
            }
            tokenIt.Next(out token);

            var firstToken = token;

            // try (identifier ::)?
            string scope = null;
            if (tokenIt.Skip("::"))
            {
                scope = token.Text;

                token = tokenIt.Peek();
                if (token == null || !token.Kind.IsIdentifierOrKeyword())
                {
                    return false;
                }
                tokenIt.Next(out token);
            }

            // identifier
            string tokenIdentifier = token.Text;

            string arguments = null;

            // ('(' tokens ')' )?
            if (tokenIt.Skip("("))
            {
                var builder = new StringBuilder();
                var previousTokenKind = CppTokenKind.Punctuation;
                while (tokenIt.PeekText() != ")" && tokenIt.Next(out token))
                {
                    if (token.Kind.IsIdentifierOrKeyword() && previousTokenKind.IsIdentifierOrKeyword())
                    {
                        builder.Append(" ");
                    }
                    previousTokenKind = token.Kind;
                    builder.Append(token.Text);
                }

                if (!tokenIt.Skip(")"))
                {
                    return false;
                }
                arguments = builder.ToString();
            }

            var isVariadic = tokenIt.Skip("...");

            var previousToken = tokenIt.PreviousToken();

            attribute = new CppAttribute(tokenIdentifier)
            {
                Span = new CppSourceSpan(firstToken.Span.Start, previousToken.Span.End),
                Scope = scope,
                Arguments = arguments,
                IsVariadic = isVariadic,
            };
            return true;
        }

        private CppType VisitTypeDefDecl(CXCursor cursor, CXCursor parent, CXClientData data)
        {
            var fulltypeDefName = cursor.UnifiedSymbolResolution.CString;
            CppType type;
            if (_typedefs.TryGetValue(fulltypeDefName, out type))
            {
                return type;
            }

            var contextContainer = GetOrCreateDeclarationContainer(cursor.SemanticParent, data);
            var underlyingTypeDefType = GetCppType(cursor.TypedefDeclUnderlyingType.Declaration, cursor.TypedefDeclUnderlyingType, parent, data);

            var typedefName = GetCursorSpelling(cursor);

            if (AutoSquashTypedef && underlyingTypeDefType is ICppMember cppMember && (string.IsNullOrEmpty(cppMember.Name) || typedefName == cppMember.Name))
            {
                cppMember.Name = typedefName;
                type = (CppType)cppMember;
            }
            else
            {
                var typedef = new CppTypedef(GetCursorSpelling(cursor), underlyingTypeDefType) { Visibility = contextContainer.CurrentVisibility };
                contextContainer.DeclarationContainer.Typedefs.Add(typedef);
                type = typedef;
            }
            
            _typedefs.Add(fulltypeDefName, type);
            return type;
        }

        private string GetCursorSpelling(CXCursor cursor)
        {
            var name = cursor.Spelling.ToString();

            if (string.IsNullOrWhiteSpace(name) && cursor.IsAnonymous)
            {
                cursor.Location.GetFileLocation(out var file, out var _, out var _, out var offset);
                var fileName = Path.GetFileNameWithoutExtension(file.Name.ToString());
                name = $"__Anonymous{cursor.Type.KindSpelling}_{fileName}_{offset}";
            }
            return name;
        }

        private CppType GetCppType(CXCursor cursor, CXType type, CXCursor parent, CXClientData data)
        {
            var cppType = GetCppTypeInternal(cursor, type, parent, data);

            if (type.IsConstQualified)
            {
                return new CppQualifiedType(CppTypeQualifier.Const, cppType);
            }
            if (type.IsVolatileQualified)
            {
                return new CppQualifiedType(CppTypeQualifier.Volatile, cppType);
            }

            return cppType;
        }

        private CppType GetCppTypeInternal(CXCursor cursor, CXType type, CXCursor parent, CXClientData data)
        {
            switch (type.kind)
            {
                case CXTypeKind.CXType_Void:
                    return CppPrimitiveType.Void;

                case CXTypeKind.CXType_Bool:
                    return CppPrimitiveType.Bool;

                case CXTypeKind.CXType_UChar:
                    return CppPrimitiveType.UnsignedChar;

                case CXTypeKind.CXType_UShort:
                    return CppPrimitiveType.UnsignedShort;

                case CXTypeKind.CXType_UInt:
                    return CppPrimitiveType.UnsignedInt;

                case CXTypeKind.CXType_ULong:
                    return CppPrimitiveType.UnsignedInt;

                case CXTypeKind.CXType_ULongLong:
                    return CppPrimitiveType.UnsignedLongLong;

                case CXTypeKind.CXType_SChar:
                    return CppPrimitiveType.Char;

                case CXTypeKind.CXType_Char_S:
                    return CppPrimitiveType.Char;

                case CXTypeKind.CXType_WChar:
                    return CppPrimitiveType.WChar;

                case CXTypeKind.CXType_Short:
                    return CppPrimitiveType.Short;

                case CXTypeKind.CXType_Int:
                    return CppPrimitiveType.Int;

                case CXTypeKind.CXType_Long:
                    return CppPrimitiveType.Int;

                case CXTypeKind.CXType_LongLong:
                    return CppPrimitiveType.LongLong;

                case CXTypeKind.CXType_Float:
                    return CppPrimitiveType.Float;

                case CXTypeKind.CXType_Double:
                    return CppPrimitiveType.Double;

                case CXTypeKind.CXType_LongDouble:
                    return CppPrimitiveType.LongDouble;

                case CXTypeKind.CXType_Pointer:
                    return new CppPointerType(GetCppType(type.PointeeType.Declaration, type.PointeeType, parent, data));

                case CXTypeKind.CXType_LValueReference:
                    return new CppReferenceType(GetCppType(type.PointeeType.Declaration, type.PointeeType, parent, data));

                case CXTypeKind.CXType_Record:
                    return VisitClassDecl(cursor, parent, data);

                case CXTypeKind.CXType_Enum:
                    return VisitEnumDecl(cursor, parent, data);

                case CXTypeKind.CXType_FunctionProto:
                    return VisitFunctionType(cursor, type, parent, data);

                case CXTypeKind.CXType_Typedef:
                    return VisitTypeDefDecl(cursor, parent, data);

                case CXTypeKind.CXType_Elaborated:
                    {
                        return GetCppType(type.CanonicalType.Declaration, type.CanonicalType, parent, data);
                    }

                case CXTypeKind.CXType_ConstantArray:
                case CXTypeKind.CXType_IncompleteArray:
                {
                    var elementType = GetCppType(type.ArrayElementType.Declaration, type.ArrayElementType, parent, data);
                    return new CppArrayType(elementType, (int) type.ArraySize);
                }

                case CXTypeKind.CXType_DependentSizedArray:
                {
                    // TODO: this is not yet supported
                    _rootCompilation.Diagnostics.Warning($"Dependent sized arrays `{type}` from `{parent}` is not supported", GetSourceLocation(parent.Location));
                    var elementType = GetCppType(type.ArrayElementType.Declaration, type.ArrayElementType, parent, data);
                    return new CppArrayType(elementType, (int)type.ArraySize);
                }

                case CXTypeKind.CXType_Unexposed:
                {
                    return new CppUnexposedType(type.ToString());
                }

                case CXTypeKind.CXType_Attributed:
                    return GetCppType(type.CanonicalType.Declaration, type.CanonicalType, parent, data);

                default:
                {
                    Unhandled(cursor, type);
                    return CppPrimitiveType.Void;
                }
            }
        }

        private CppFunctionType VisitFunctionType(CXCursor cursor, CXType type, CXCursor parent, CXClientData data)
        {
            // Gets the return type
            var returnType = GetCppType(type.ResultType.Declaration, type.ResultType, cursor, data);

            var cppFunction = new CppFunctionType(returnType)
            {
                CallingConvention = GetCallingConvention(type)
            };

            for (uint i = 0; i < type.NumArgTypes; i++)
            {
                var argType = type.GetArgType(i);
                var cppType = GetCppType(argType.Declaration, argType, type.Declaration, data);
                cppFunction.ParameterTypes.Add(cppType);
            }

            return cppFunction;
        }

        private void Unhandled(CXCursor cursor)
        {
            _rootCompilation.Diagnostics.Error($"Unhandled cursor kind: {cursor.KindSpelling}.", GetSourceLocation(cursor.Location));
        }

        private void Unhandled(CXCursor cursor, CXType type)
        {
            _rootCompilation.Diagnostics.Error($"The type `{type}` of kind `{type.KindSpelling} is not supported at `{cursor}`", GetSourceLocation(cursor.Location));
        }

        protected CXChildVisitResult Unhandled(CXCursor cursor, CXCursor parent)
        {
            _rootCompilation.Diagnostics.Error($"Unhandled cursor kind: {cursor.KindSpelling} in {parent.KindSpelling}.", GetSourceLocation(cursor.Location));
            return CXChildVisitResult.CXChildVisit_Break;
        }

        protected void WarningUnhandled(CXCursor cursor, CXCursor parent)
        {
            _rootCompilation.Diagnostics.Warning($"Unhandled cursor kind: {cursor.KindSpelling} in {parent.KindSpelling}.", GetSourceLocation(cursor.Location));
        }
        
        /// <summary>
        /// Internal class to iterate on tokens
        /// </summary>
        private class TokenIterator
        {
            private readonly Tokenizer _tokens;
            private int _index;

            public TokenIterator(Tokenizer tokens)
            {
                _tokens = tokens;
            }

            public bool Skip(string expectedText)
            {
                if (_index < _tokens.Count)
                {
                    if (_tokens.GetString(_index) == expectedText)
                    {
                        _index++;
                        return true;
                    }
                }

                return false;
            }

            public CppToken PreviousToken()
            {
                if (_index > 0)
                {
                    return _tokens[_index - 1];
                }

                return null;
            }

            public bool Skip(params string[] expectedTokens)
            {
                var startIndex = _index;
                foreach (var expectedToken in expectedTokens)
                {
                    if (startIndex < _tokens.Count)
                    {
                        if (_tokens.GetString(startIndex) == expectedToken)
                        {
                            startIndex++;
                            continue;
                        }
                    }
                    return false;
                }
                _index = startIndex;
                return true;
            }

            public bool Find(params string[] expectedTokens)
            {
                var startIndex = _index;
                restart:
                while (startIndex < _tokens.Count)
                {
                    var firstIndex = startIndex;
                    foreach (var expectedToken in expectedTokens)
                    {
                        if (startIndex < _tokens.Count)
                        {
                            if (_tokens.GetString(startIndex) == expectedToken)
                            {
                                startIndex++;
                                continue;
                            }
                        }
                        startIndex = firstIndex + 1;
                        goto restart;
                    }
                    _index = firstIndex;
                    return true;
                }
                return false;
            }

            public bool Next(out CppToken token)
            {
                token = null;
                if (_index < _tokens.Count)
                {
                    token = _tokens[_index];
                    _index++;
                    return true;
                }
                return false;
            }

            public bool CanPeek => _index < _tokens.Count;

            public bool Next()
            {
                if (_index < _tokens.Count)
                {
                    _index++;
                    return true;
                }
                return false;
            }

            public CppToken Peek()
            {
                if (_index < _tokens.Count)
                {
                    return _tokens[_index];
                }
                return null;
            }

            public string PeekText()
            {
                if (_index < _tokens.Count)
                {
                    return _tokens.GetString(_index);
                }
                return null;
            }
        }

        /// <summary>
        /// Internal class to tokenize
        /// </summary>
        [DebuggerTypeProxy(typeof(TokenizerDebuggerType))]
        private class Tokenizer
        {
            private readonly CXToken[] _tokens;
            private CppToken[] _cppTokens;
            private readonly CXTranslationUnit _tu;

            public Tokenizer(CXCursor cursor)
            {
                var range = cursor.Extent;
                _tokens = null;
                var tu = cursor.TranslationUnit;
                tu.Tokenize(range, out _tokens);
                _tu = tu;
            }

            public int Count => _tokens?.Length ?? 0;

            public CppToken this[int i]
            {
                get
                {
                    // Only create a tokenizer if necessary
                    if (_cppTokens == null)
                    {
                        _cppTokens = new CppToken[_tokens.Length];
                    }

                    ref var cppToken = ref _cppTokens[i];
                    if (cppToken != null)
                    {
                        return cppToken;
                    }

                    var token = _tokens[i];
                    CppTokenKind cppTokenKind = 0;
                    switch (token.Kind)
                    {
                        case CXTokenKind.CXToken_Punctuation:
                            cppTokenKind = CppTokenKind.Punctuation;
                            break;
                        case CXTokenKind.CXToken_Keyword:
                            cppTokenKind = CppTokenKind.Keyword;
                            break;
                        case CXTokenKind.CXToken_Identifier:
                            cppTokenKind = CppTokenKind.Identifier;
                            break;
                        case CXTokenKind.CXToken_Literal:
                            cppTokenKind = CppTokenKind.Literal;
                            break;
                        case CXTokenKind.CXToken_Comment:
                            cppTokenKind = CppTokenKind.Comment;
                            break;
                        default:
                            break;
                    }

                    var tokenStr = token.GetSpelling(_tu).CString;

                    var tokenRange = token.GetExtent(_tu);
                    cppToken = new CppToken(cppTokenKind, tokenStr)
                    {
                        Span = new CppSourceSpan(GetSourceLocation(tokenRange.Start), GetSourceLocation(tokenRange.End))
                    };
                    return cppToken;
                }
            }

            public string GetString(int i)
            {
                var token = _tokens[i];
                return token.GetSpelling(_tu).CString;
            }
        }

        private class TokenizerDebuggerType
        {
            private readonly Tokenizer _tokenizer;

            public TokenizerDebuggerType(Tokenizer tokenizer)
            {
                _tokenizer = tokenizer;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items
            {
                get
                {
                    var array = new object[_tokenizer.Count];
                    for (int i = 0; i < _tokenizer.Count; i++)
                    {
                        array[i] = _tokenizer[i];
                    }
                    return array;
                }
            }
        }

        private class CppContainerContext
        {
            public CppContainerContext(ICppContainer container)
            {
                Container = container;
            }

            public ICppContainer Container;

            public ICppDeclarationContainer DeclarationContainer => Container as ICppDeclarationContainer;

            public CppVisibility CurrentVisibility;

            public bool IsChildrenVisited;
        }
    }
}