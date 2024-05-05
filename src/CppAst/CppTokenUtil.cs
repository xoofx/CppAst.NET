// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using ClangSharp;
using ClangSharp.Interop;

namespace CppAst
{
    static internal unsafe class CppTokenUtil
    {
        public static void ParseCursorAttributs(CppGlobalDeclarationContainer globalContainer, CXCursor cursor, ref List<CppAttribute> attributes)
        {
            var tokenizer = new AttributeTokenizer(cursor);
            var tokenIt = new TokenIterator(tokenizer);

            // if this is a template then we need to skip that ?
            if (tokenIt.CanPeek && tokenIt.PeekText() == "template")
                SkipTemplates(tokenIt);

            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(globalContainer, tokenIt, ref attributes))
                {
                    continue;
                }

                // If we have a keyword, try to skip it and process following elements
                // for example attribute put right after a struct __declspec(uuid("...")) Test {...}
                if (tokenIt.Peek().Kind == CppTokenKind.Keyword)
                {
                    tokenIt.Next();
                    continue;
                }
                break;
            }
        }


        public static void ParseFunctionAttributes(CppGlobalDeclarationContainer globalContainer, CXCursor cursor, string functionName, ref List<CppAttribute> attributes)
        {
            // TODO: This function is not 100% correct when parsing tokens up to the function name
            // we assume to find the function name immediately followed by a `(`
            // but some return type parameter could actually interfere with that
            // Ideally we would need to parse more properly return type and skip parenthesis for example
            var tokenizer = new AttributeTokenizer(cursor);
            var tokenIt = new TokenIterator(tokenizer);

            // if this is a template then we need to skip that ?
            if (tokenIt.CanPeek && tokenIt.PeekText() == "template")
                SkipTemplates(tokenIt);

            // Parse leading attributes
            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(globalContainer, tokenIt, ref attributes))
                {
                    continue;
                }
                break;
            }

            if (!tokenIt.CanPeek)
            {
                return;
            }

            // Find function name (We only support simple function name declaration)
            if (!tokenIt.Find(functionName, "("))
            {
                return;
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
                return;
            }

            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(globalContainer, tokenIt, ref attributes))
                {
                    continue;
                }
                // Skip the token if we can parse it.
                tokenIt.Next();
            }

            return;
        }


        public static void ParseAttributesInRange(CppGlobalDeclarationContainer globalContainer, CXTranslationUnit tu, CXSourceRange range, ref List<CppAttribute> collectAttributes)
        {
            var tokenizer = new AttributeTokenizer(tu, range);
            var tokenIt = new TokenIterator(tokenizer);

            var tokenIt2 = new TokenIterator(tokenizer);
            StringBuilder sb = new StringBuilder();
            while (tokenIt.CanPeek)
            {
                sb.Append(tokenIt.PeekText());
                tokenIt.Next();
            }

            // if this is a template then we need to skip that ?
            if (tokenIt.CanPeek && tokenIt.PeekText() == "template")
                SkipTemplates(tokenIt);

            while (tokenIt.CanPeek)
            {
                if (ParseAttributes(globalContainer, tokenIt, ref collectAttributes))
                {
                    continue;
                }

                // If we have a keyword, try to skip it and process following elements
                // for example attribute put right after a struct __declspec(uuid("...")) Test {...}
                if (tokenIt.Peek().Kind == CppTokenKind.Keyword)
                {
                    tokenIt.Next();
                    continue;
                }
                break;
            }
        }

        public static bool TryToSeekOnlineAttributes(CXCursor cursor, out CXSourceRange range)
        {


            int SkipWhiteSpace(ReadOnlySpan<byte> cnt, int cntOffset)
            {
                while (cntOffset > 0)
                {
                    char ch = (char)cnt[cntOffset];
                    if (ch == ' ' || ch == '\r' || ch == '\n' || ch == '\t')
                    {
                        cntOffset--;
                    }
                    else
                    {
                        break;
                    }
                }

                return cntOffset;
            };

            int ToLineStart(ReadOnlySpan<byte> cnt, int cntOffset)
            {
                for (int i = cntOffset; i >= 0; i--)
                {
                    char ch = (char)cnt[i];
                    if (ch == '\n')
                    {
                        return i + 1;
                    }
                }
                return 0;
            };

            bool IsAttributeEnd(ReadOnlySpan<byte> cnt, int cntOffset)
            {
                if (cntOffset < 1) return false;

                char ch0 = (char)cnt[cntOffset];
                char ch1 = (char)cnt[cntOffset - 1];

                return ch0 == ch1 && ch0 == ']';
            };

            bool IsAttributeStart(ReadOnlySpan<byte> cnt, int cntOffset)
            {
                if (cntOffset < 1) return false;

                char ch0 = (char)cnt[cntOffset];
                char ch1 = (char)cnt[cntOffset - 1];

                return ch0 == ch1 && ch0 == '[';
            };

            bool SeekAttributeStartSingleChar(ReadOnlySpan<byte> cnt, int cntOffset, out int outSeekOffset)
            {
                outSeekOffset = cntOffset;
                while (cntOffset > 0)
                {
                    char ch = (char)cnt[cntOffset];
                    if (ch == '[')
                    {
                        outSeekOffset = cntOffset;
                        return true;
                    }
                    cntOffset--;
                }
                return false;
            };

            int SkipAttributeStartOrEnd(ReadOnlySpan<byte> cnt, int cntOffset)
            {
                cntOffset -= 2;
                return cntOffset;
            };

            string QueryLineContent(ReadOnlySpan<byte> cnt, int startOffset, int endOffset)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = startOffset; i <= endOffset; i++)
                {
                    sb.Append((char)cnt[i]);
                }
                return sb.ToString();
            };

            CXSourceLocation location = cursor.Extent.Start;
            location.GetFileLocation(out var file, out var line, out var column, out var offset);
            var contents = cursor.TranslationUnit.GetFileContents(file, out var fileSize);

            AttributeLexerParseStatus status = AttributeLexerParseStatus.SeekAttributeEnd;
            int offsetStart = (int)offset - 1;   //Try to ignore start char here
            int lastSeekOffset = offsetStart;
            int curOffset = offsetStart;
            while (curOffset > 0)
            {
                curOffset = SkipWhiteSpace(contents, curOffset);

                switch (status)
                {
                    case AttributeLexerParseStatus.SeekAttributeEnd:
                        {
                            if (!IsAttributeEnd(contents, curOffset))
                            {
                                status = AttributeLexerParseStatus.Error;
                            }
                            else
                            {
                                curOffset = SkipAttributeStartOrEnd(contents, curOffset);
                                status = AttributeLexerParseStatus.SeekAttributeStart;
                            }
                        }
                        break;
                    case AttributeLexerParseStatus.SeekAttributeStart:
                        {
                            if (!SeekAttributeStartSingleChar(contents, curOffset, out var queryOffset))
                            {
                                status = AttributeLexerParseStatus.Error;
                            }
                            else
                            {
                                if (IsAttributeStart(contents, queryOffset))
                                {
                                    curOffset = SkipAttributeStartOrEnd(contents, queryOffset);
                                    lastSeekOffset = curOffset + 1;
                                    status = AttributeLexerParseStatus.SeekAttributeEnd;
                                }
                                else
                                {
                                    status = AttributeLexerParseStatus.Error;
                                }
                            }
                        }
                        break;
                }

                if (status == AttributeLexerParseStatus.Error)
                {
                    break;
                }
            }
            if (lastSeekOffset == offsetStart)
            {
                range = new CXSourceRange();
                return false;
            }
            else
            {
                var startLoc = cursor.TranslationUnit.GetLocationForOffset(file, (uint)lastSeekOffset);
                var endLoc = cursor.TranslationUnit.GetLocationForOffset(file, (uint)offsetStart);
                range = clang.getRange(startLoc, endLoc);
                return true;
            }
        }

        #region "Nested Types"

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
        internal class Tokenizer
        {
            private readonly CXSourceRange _range;
            private CppToken[] _cppTokens;
            protected readonly CXTranslationUnit _tu;

            public Tokenizer(CXCursor cursor)
            {
                _tu = cursor.TranslationUnit;
                _range = GetRange(cursor);
            }

            public Tokenizer(CXTranslationUnit tu, CXSourceRange range)
            {
                _tu = tu;
                _range = range;
            }

            public virtual CXSourceRange GetRange(CXCursor cursor)
            {
                return cursor.Extent;
            }

            public int Count
            {
                get
                {
                    var tokens = _tu.Tokenize(_range);
                    int length = tokens.Length;
                    _tu.DisposeTokens(tokens);
                    return length;
                }
            }

            public CppToken this[int i]
            {
                get
                {
                    // Only create a tokenizer if necessary
                    if (_cppTokens == null)
                    {
                        _cppTokens = new CppToken[Count];
                    }

                    ref var cppToken = ref _cppTokens[i];
                    if (cppToken != null)
                    {
                        return cppToken;
                    }
                    var tokens = _tu.Tokenize(_range);
                    var token = tokens[i];

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

                    var tokenStr = CXUtil.GetTokenSpelling(token, _tu);
                    var tokenLocation = token.GetLocation(_tu);

                    var tokenRange = token.GetExtent(_tu);
                    cppToken = new CppToken(cppTokenKind, tokenStr)
                    {
                        Span = new CppSourceSpan(CppModelBuilder.GetSourceLocation(tokenRange.Start), CppModelBuilder.GetSourceLocation(tokenRange.End))
                    };
                    _tu.DisposeTokens(tokens);
                    return cppToken;
                }
            }

            public string GetString(int i)
            {
                var tokens = _tu.Tokenize(_range);
                var TokenSpelling = CXUtil.GetTokenSpelling(tokens[i], _tu);
                _tu.DisposeTokens(tokens);
                return TokenSpelling;
            }

            public string TokensToString()
            {
                int length = Count;
                if (length <= 0)
                {
                    return null;
                }

                var tokens = new List<CppToken>(length);

                for (int i = 0; i < length; i++)
                {
                    tokens.Add(this[i]);
                }

                return CppToken.TokensToString(tokens);
            }

            public string GetStringForLength(int length)
            {
                StringBuilder result = new StringBuilder(length);
                for (var cur = 0; cur < Count; ++cur)
                {
                    result.Append(GetString(cur));
                    if (result.Length >= length)
                        return result.ToString();
                }
                return result.ToString();
            }
        }


        private class AttributeTokenizer : Tokenizer
        {
            public AttributeTokenizer(CXCursor cursor) : base(cursor)
            {
            }

            public AttributeTokenizer(CXTranslationUnit tu, CXSourceRange range) : base(tu, range)
            {

            }

            private uint IncOffset(int inc, uint offset)
            {
                if (inc >= 0)
                    offset += (uint)inc;
                else
                    offset -= (uint)-inc;
                return offset;
            }

            private Tuple<CXSourceRange, CXSourceRange> GetExtent(CXTranslationUnit tu, CXCursor cur)
            {
                var cursorExtend = cur.Extent;
                var begin = cursorExtend.Start;
                var end = cursorExtend.End;

                bool CursorIsFunction(CXCursorKind inKind)
                {
                    return inKind == CXCursorKind.CXCursor_FunctionDecl || inKind == CXCursorKind.CXCursor_CXXMethod
                           || inKind == CXCursorKind.CXCursor_Constructor || inKind == CXCursorKind.CXCursor_Destructor
                           || inKind == CXCursorKind.CXCursor_ConversionFunction;
                }

                bool CursorIsVar(CXCursorKind inKind)
                {
                    return inKind == CXCursorKind.CXCursor_VarDecl || inKind == CXCursorKind.CXCursor_FieldDecl;
                }

                bool IsInRange(CXSourceLocation loc, CXSourceRange range)
                {
                    var xbegin = range.Start;
                    var xend = range.End;

                    loc.GetSpellingLocation(out var fileLocation, out var lineLocation, out var u1, out var u2);
                    xbegin.GetSpellingLocation(out var fileBegin, out var lineBegin, out u1, out u2);
                    xend.GetSpellingLocation(out var fileEnd, out var lineEnd, out u1, out u2);

                    return lineLocation >= lineBegin && lineLocation < lineEnd && (fileLocation.Equals(fileBegin));
                }

                bool HasInlineTypeDefinition(CXCursor varDecl)
                {
                    var typeDecl = varDecl.Type.Declaration;
                    if (typeDecl.IsNull)
                        return false;

                    var typeLocation = typeDecl.Location;
                    var varRange = typeDecl.Extent;
                    return IsInRange(typeLocation, varRange);
                }

                CXSourceLocation GetNextLocation(CXSourceLocation loc, int inc = 1)
                {
                    CXSourceLocation value;
                    loc.GetSpellingLocation(out var file, out var line, out var column, out var originalOffset);
                    var signedOffset = (int)column + inc;
                    var shouldUseLine = (column != 0 && signedOffset > 0);
                    if (shouldUseLine)
                    {
                        value = tu.GetLocation(file, line, (uint)signedOffset);
                    }
                    else
                    {
                        var offset = IncOffset(inc, originalOffset);
                        value = tu.GetLocationForOffset(file, offset);
                    }

                    return value;
                }

                CXSourceLocation GetPrevLocation(CXSourceLocation loc, int tokenLength)
                {
                    var inc = 1;
                    while (true)
                    {
                        var locBefore = GetNextLocation(loc, -inc);
                        CXToken* tokens;
                        uint size;
                        clang.tokenize(tu, clang.getRange(locBefore, loc), &tokens, &size);
                        if (size == 0)
                            return CXSourceLocation.Null;

                        var tokenLocation = tokens[0].GetLocation(tu);
                        if (locBefore.Equals(tokenLocation))
                        {
                            return GetNextLocation(loc, -1 * (inc + tokenLength - 1));
                        }
                        else
                            ++inc;
                    }
                }

                bool TokenIsBefore(CXSourceLocation loc, string tokenString)
                {
                    var length = tokenString.Length;
                    var locBefore = GetPrevLocation(loc, length);

                    var tokenizer = new Tokenizer(tu, clang.getRange(locBefore, loc));
                    if (tokenizer.Count == 0) return false;

                    return tokenizer.GetStringForLength(length) == tokenString;
                }

                bool TokenAtIs(CXSourceLocation loc, string tokenString)
                {
                    var length = tokenString.Length;

                    var locAfter = GetNextLocation(loc, length);
                    var tokenizer = new Tokenizer(tu, clang.getRange(locAfter, loc));

                    return tokenizer.GetStringForLength(length) == tokenString;
                }

                bool ConsumeIfTokenAtIs(ref CXSourceLocation loc, string tokenString)
                {
                    var length = tokenString.Length;

                    var locAfter = GetNextLocation(loc, length);
                    var tokenizer = new Tokenizer(tu, clang.getRange(locAfter, loc));
                    if (tokenizer.Count == 0)
                        return false;

                    if (tokenizer.GetStringForLength(length) == tokenString)
                    {
                        loc = locAfter;
                        return true;
                    }
                    else
                        return false;
                }

                bool ConsumeIfTokenBeforeIs(ref CXSourceLocation loc, string tokenString)
                {
                    var length = tokenString.Length;

                    var locBefore = GetPrevLocation(loc, length);

                    var tokenizer = new Tokenizer(tu, clang.getRange(locBefore, loc));
                    if (tokenizer.GetStringForLength(length) == tokenString)
                    {
                        loc = locBefore;
                        return true;
                    }
                    else
                        return false;
                }

                bool CheckIfValidOrReset(ref CXSourceLocation checkedLocation, CXSourceLocation resetLocation)
                {
                    bool isValid = true;
                    if (checkedLocation.Equals(CXSourceLocation.Null))
                    {
                        checkedLocation = resetLocation;
                        isValid = false;
                    }

                    return isValid;
                }

                var kind = cur.Kind;
                if (CursorIsFunction(kind) || CursorIsFunction(cur.TemplateCursorKind)
                || kind == CXCursorKind.CXCursor_VarDecl || kind == CXCursorKind.CXCursor_FieldDecl || kind == CXCursorKind.CXCursor_ParmDecl
                || kind == CXCursorKind.CXCursor_NonTypeTemplateParameter)
                {
                    while (TokenIsBefore(begin, "]]") || TokenIsBefore(begin, ")"))
                    {
                        var saveBegin = begin;
                        if (ConsumeIfTokenBeforeIs(ref begin, "]]"))
                        {
                            bool isValid = true;
                            while (!ConsumeIfTokenBeforeIs(ref begin, "[[") && isValid)
                            {
                                begin = GetPrevLocation(begin, 1);
                                isValid = CheckIfValidOrReset(ref begin, saveBegin);
                            }

                            if (!isValid)
                            {
                                break;
                            }
                        }
                        else if (ConsumeIfTokenBeforeIs(ref begin, ")"))
                        {
                            var parenCount = 1;
                            for (var lastBegin = begin; parenCount != 0; lastBegin = begin)
                            {
                                if (TokenIsBefore(begin, "("))
                                    --parenCount;
                                else if (TokenIsBefore(begin, ")"))
                                    ++parenCount;

                                begin = GetPrevLocation(begin, 1);

                                // We have reached the end of the source of trying to deal
                                // with the potential of alignas, so we just break, which
                                // will cause ConsumeIfTokenBeforeIs(ref begin, "alignas") to be false
                                // and thus fall back to saveBegin which is the correct behavior
                                if (!CheckIfValidOrReset(ref begin, saveBegin))
                                    break;
                            }

                            if (!ConsumeIfTokenBeforeIs(ref begin, "alignas"))
                            {
                                begin = saveBegin;
                                break;
                            }
                        }
                    }

                    if (CursorIsVar(kind) || CursorIsVar(cur.TemplateCursorKind))
                    {
                        if (HasInlineTypeDefinition(cur))
                        {
                            var typeCursor = clang.getTypeDeclaration(clang.getCursorType(cur));
                            var typeExtent = clang.getCursorExtent(typeCursor);

                            var typeBegin = clang.getRangeStart(typeExtent);
                            var typeEnd = clang.getRangeEnd(typeExtent);

                            return new Tuple<CXSourceRange, CXSourceRange>(clang.getRange(begin, typeBegin), clang.getRange(typeEnd, end));
                        }
                    }
                    else if (kind == CXCursorKind.CXCursor_TemplateTypeParameter && TokenAtIs(end, "("))
                    {
                        var next = GetNextLocation(end, 1);
                        var prev = end;
                        for (var parenCount = 1; parenCount != 0; next = GetNextLocation(next, 1))
                        {
                            if (TokenAtIs(next, "("))
                                ++parenCount;
                            else if (TokenAtIs(next, ")"))
                                --parenCount;
                            prev = next;
                        }
                        end = next;
                    }
                    else if (kind == CXCursorKind.CXCursor_TemplateTemplateParameter && TokenAtIs(end, "<"))
                    {
                        var next = GetNextLocation(end, 1);
                        for (var angleCount = 1; angleCount != 0; next = GetNextLocation(next, 1))
                        {
                            if (TokenAtIs(next, ">"))
                                --angleCount;
                            else if (TokenAtIs(next, ">>"))
                                angleCount -= 2;
                            else if (TokenAtIs(next, "<"))
                                ++angleCount;
                        }

                        while (!TokenAtIs(next, ">") && !TokenAtIs(next, ","))
                            next = GetNextLocation(next, 1);

                        end = GetPrevLocation(next, 1);
                    }
                    else if ((kind == CXCursorKind.CXCursor_TemplateTypeParameter || kind == CXCursorKind.CXCursor_NonTypeTemplateParameter
                        || kind == CXCursorKind.CXCursor_TemplateTemplateParameter))
                    {
                        ConsumeIfTokenAtIs(ref end, "...");
                    }
                    else if (kind == CXCursorKind.CXCursor_EnumConstantDecl && !TokenAtIs(end, ","))
                    {
                        var parent = clang.getCursorLexicalParent(cur);
                        end = clang.getRangeEnd(clang.getCursorExtent(parent));
                    }
                }

                return new Tuple<CXSourceRange, CXSourceRange>(clang.getRange(begin, end), clang.getNullRange());
            }

            public override CXSourceRange GetRange(CXCursor cursor)
            {
                /*  This process is complicated when parsing attributes that use
                    C++11 syntax, essentially even if libClang understands them
                    it doesn't always return them back as parse of the token range.

                    This is kind of frustrating when you want to be able to do something
                    with custom or even compiler attributes in your parsing. Thus we have
                    to do things a little manually in order to make this work. 

                    This code supports stepping back when its valid to parse attributes, it 
                    doesn't currently support all cases but it supports most valid cases.
                */
                var range = GetExtent(_tu, cursor);

                var beg = range.Item1.Start;
                var end = range.Item1.End;
                if (!range.Item2.Equals(CXSourceRange.Null))
                    end = range.Item2.End;

                return clang.getRange(beg, end);
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

        #endregion


        #region "Private Functions"

        private static void SkipTemplates(TokenIterator iter)
        {
            if (iter.CanPeek)
            {
                if (iter.Skip("template"))
                {
                    iter.Next(); // skip the first >
                    int parentCount = 1;
                    while (parentCount > 0 && iter.CanPeek)
                    {
                        var text = iter.PeekText();
                        if (text == ">")
                        {
                            parentCount--;
                        }
                        iter.Next();
                    }
                }
            }
        }

        private enum AttributeLexerParseStatus
        {
            SeekAttributeEnd,
            SeekAttributeStart,
            Error,
        }

        private static (string, string) GetNameSpaceAndAttribute(string fullAttribute)
        {
            string[] colons = { "::" };
            string[] tokens = fullAttribute.Split(colons, System.StringSplitOptions.None);
            if (tokens.Length == 2)
            {
                return (tokens[0], tokens[1]);
            }
            else
            {
                return (null, tokens[0]);
            }
        }


        private static (string, string) GetNameAndArguments(string name)
        {
            if (name.Contains("("))
            {
                Char[] seperator = { '(' };
                var argumentTokens = name.Split(seperator, 2);
                var length = argumentTokens[1].LastIndexOf(')');
                string argument = null;
                if (length > 0)
                {
                    argument = argumentTokens[1].Substring(0, length);
                }
                return (argumentTokens[0], argument);
            }
            else
            {
                return (name, null);
            }
        }

        private static bool ParseAttributes(CppGlobalDeclarationContainer globalContainer, TokenIterator tokenIt, ref List<CppAttribute> attributes)
        {
            // Parse C++ attributes
            // [[<attribute>]]
            if (tokenIt.Skip("[", "["))
            {
                while (ParseAttribute(tokenIt, out var attribute))
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
                while (ParseAttribute(tokenIt, out var attribute))
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
                while (ParseAttribute(tokenIt, out var attribute))
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

            // Parse C++11 alignas attribute
            // alignas(expression)
            if (tokenIt.PeekText() == "alignas")
            {
                while (ParseAttribute(tokenIt, out var attribute))
                {
                    if (attributes == null)
                    {
                        attributes = new List<CppAttribute>();
                    }
                    attributes.Add(attribute);

                    break;
                }

                return tokenIt.Skip(")"); ;
            }

            // See if we have a macro
            var value = tokenIt.PeekText();
            var macro = globalContainer.Macros.Find(v => v.Name == value);
            if (macro != null)
            {
                if (macro.Value.StartsWith("[[") && macro.Value.EndsWith("]]"))
                {
                    CppAttribute attribute = null;
                    var fullAttribute = macro.Value.Substring(2, macro.Value.Length - 4);
                    var (scope, name) = GetNameSpaceAndAttribute(fullAttribute);
                    var (attributeName, arguments) = GetNameAndArguments(name);

                    attribute = new CppAttribute(attributeName, AttributeKind.TokenAttribute);
                    attribute.Scope = scope;
                    attribute.Arguments = arguments;

                    if (attributes == null)
                    {
                        attributes = new List<CppAttribute>();
                    }
                    attributes.Add(attribute);
                    tokenIt.Next();
                    return true;
                }
            }

            return false;
        }

        private static bool ParseAttribute(TokenIterator tokenIt, out CppAttribute attribute)
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

            attribute = new CppAttribute(tokenIdentifier, AttributeKind.TokenAttribute)
            {
                Span = new CppSourceSpan(firstToken.Span.Start, previousToken.Span.End),
                Scope = scope,
                Arguments = arguments,
                IsVariadic = isVariadic,
            };
            return true;
        }


        #endregion
    }

}


