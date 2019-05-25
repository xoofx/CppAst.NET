using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ClangSharp;

namespace CppAst
{
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

            int i = 0;
            cursor.VisitChildren((argCursor, functionCursor, clientData) =>
            {
                switch (argCursor.Kind)
                {
                    case CXCursorKind.CXCursor_ParmDecl:
                        var argName = GetCursorSpelling(argCursor);

                        if (string.IsNullOrEmpty(argName))
                            argName = "param" + i;

                        var parameter = new CppParameter(GetCppType(argCursor.Type.Declaration, argCursor.Type, functionCursor, clientData), argName);

                        cppFunction.Parameters.Add(parameter);

                        i++;
                        break;
                    case CXCursorKind.CXCursor_FirstAttr:
                    case CXCursorKind.CXCursor_VisibilityAttr:
                        // TODO
                        break;
                    case CXCursorKind.CXCursor_TypeRef:
                        break;

                    case CXCursorKind.CXCursor_DLLExport:
                        cppFunction.AttributeFlags |= CppAttributeFlags.DllExport;
                        break;

                    case CXCursorKind.CXCursor_DLLImport:
                        cppFunction.AttributeFlags |= CppAttributeFlags.DllImport;
                        break;

                    default:
                        WarningUnhandled(cursor, parent);
                        break;
                }

                return CXChildVisitResult.CXChildVisit_Continue;

            }, data);

            return cppFunction;
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

            var cppFunction = new CppFunctionType(returnType);

            for (uint i = 0; i < type.NumArgTypes; i++)
            {
                var argType = type.GetArgType(i);
                var argName = "param" + i;

                var parameter = new CppParameter(GetCppType(argType.Declaration, argType, type.Declaration, data), argName);
                cppFunction.ParameterTypes.Add(parameter);
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