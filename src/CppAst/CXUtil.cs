using ClangSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CppAst
{
    static internal unsafe class CXUtil
    {
        #region Cursor
        public static string GetCursorSpelling(CXCursor cursor)
        {
            var cursorSpelling = cursor.Spelling;
            string cursorSpellingStr = cursorSpelling.ToString();
            cursorSpelling.Dispose();
            return cursorSpellingStr.StartsWith("(") ? string.Empty : cursorSpellingStr;
        }

        public static string GetCursorUsrString(CXCursor cursor)
        {
            var cursorUsr = cursor.Usr;
            string cursorUsrStr = cursorUsr.ToString();
            cursorUsr.Dispose();
            return cursorUsrStr;
        }

        public static string GetCursorDisplayName(CXCursor cursor)
        {
            var cursorDisplayName = cursor.DisplayName;
            string cursorDisplayNameStr = cursorDisplayName.ToString();
            cursorDisplayName.Dispose();
            return cursorDisplayNameStr;
        }
        #endregion Cursor

        #region Comment
        public static string GetComment_TextComment_Text(CXComment comment)
        {
            var textComment_Text = comment.TextComment_Text;
            string textComment_TextStr = textComment_Text.ToString();
            textComment_Text.Dispose();
            return textComment_TextStr;
        }

        public static string GetComment_InlineCommandComment_CommandName(CXComment comment)
        {
            var inlineCommandComment_CommandName = comment.InlineCommandComment_CommandName;
            string inlineCommandComment_CommandNameStr = inlineCommandComment_CommandName.ToString();
            inlineCommandComment_CommandName.Dispose();
            return inlineCommandComment_CommandNameStr;
        }

        public static string GetComment_InlineCommandComment_ArgText(CXComment comment, uint index)
        {
            var inlineCommandComment_ArgText = comment.InlineCommandComment_GetArgText(index);
            string inlineCommandComment_ArgTextStr = inlineCommandComment_ArgText.ToString();
            inlineCommandComment_ArgText.Dispose();
            return inlineCommandComment_ArgTextStr;
        }

        public static string GetComment_HtmlTagComment_TagName(CXComment comment)
        {
            var htmlTagComment_TagName = comment.HtmlTagComment_TagName;
            string htmlTagComment_TagNameStr = htmlTagComment_TagName.ToString();
            htmlTagComment_TagName.Dispose();
            return htmlTagComment_TagNameStr;
        }

        public static string GetComment_HtmlStartTag_AttrName(CXComment comment, uint index)
        {
            var htmlStartTag_AttrName = comment.HtmlStartTag_GetAttrName(index);
            string htmlStartTag_AttrNameStr = htmlStartTag_AttrName.ToString();
            htmlStartTag_AttrName.Dispose();
            return htmlStartTag_AttrNameStr;
        }

        public static string GetComment_HtmlStartTag_AttrValue(CXComment comment, uint index)
        {
            var htmlStartTag_AttrValue = comment.HtmlStartTag_GetAttrValue(index);
            string htmlStartTag_AttrValueStr = htmlStartTag_AttrValue.ToString();
            htmlStartTag_AttrValue.Dispose();
            return htmlStartTag_AttrValueStr;
        }

        public static string GetComment_BlockCommandComment_CommandName(CXComment comment)
        {
            var blockCommandComment_CommandName = comment.BlockCommandComment_CommandName;
            string blockCommandComment_CommandNameStr = blockCommandComment_CommandName.ToString();
            blockCommandComment_CommandName.Dispose();
            return blockCommandComment_CommandNameStr;
        }

        public static string GetComment_BlockCommandComment_ArgText(CXComment comment, uint index)
        {
            var blockCommandComment_ArgText = comment.BlockCommandComment_GetArgText(index);
            string blockCommandComment_ArgTextStr = blockCommandComment_ArgText.ToString();
            blockCommandComment_ArgText.Dispose();
            return blockCommandComment_ArgTextStr;
        }

        public static string GetComment_ParamCommandComment_ParamName(CXComment comment)
        {
            var paramCommandComment_ParamName = comment.ParamCommandComment_ParamName;
            string paramCommandComment_ParamNameStr = paramCommandComment_ParamName.ToString();
            paramCommandComment_ParamName.Dispose();
            return paramCommandComment_ParamNameStr;
        }

        public static string GetComment_TParamCommandComment_ParamName(CXComment comment)
        {
            var tParamCommandComment_ParamName = comment.TParamCommandComment_ParamName;
            string tParamCommandComment_ParamNameStr = tParamCommandComment_ParamName.ToString();
            tParamCommandComment_ParamName.Dispose();
            return tParamCommandComment_ParamNameStr;
        }

        public static string GetComment_VerbatimBlockLineComment_Text(CXComment comment)
        {
            var verbatimBlockLineComment_Text = comment.VerbatimBlockLineComment_Text;
            string verbatimBlockLineComment_TextStr = verbatimBlockLineComment_Text.ToString();
            verbatimBlockLineComment_Text.Dispose();
            return verbatimBlockLineComment_TextStr;
        }

        public static string GetComment_VerbatimLineComment_Text(CXComment comment)
        {
            var verbatimLineComment_Text = comment.VerbatimLineComment_Text;
            string verbatimLineComment_TextStr = verbatimLineComment_Text.ToString();
            verbatimLineComment_Text.Dispose();
            return verbatimLineComment_TextStr;
        }
        #endregion Comment

        #region Token
        public static string GetTokenSpelling(CXToken token, CXTranslationUnit tu)
        {
            var tokenSpelling = token.GetSpelling(tu);
            string tokenSpellingStr = tokenSpelling.ToString();
            tokenSpelling.Dispose();
            return tokenSpellingStr;
        }
        #endregion Token

        #region File
        public static string GetFileName(CXFile file)
        {
            var fileName = file.Name;
            string fileNameStr = fileName.ToString();
            fileName.Dispose();
            return fileNameStr;
        }
        #endregion File

        #region Type
        public static string GetTypeKindSpelling(CXType type)
        {
            var kindSpelling= type.KindSpelling;
            string kindSpellingStr = kindSpelling.ToString();
            kindSpelling.Dispose();
            return kindSpellingStr;
        }

        public static string GetTypeSpelling(CXType type)
        {
            var spelling = type.Spelling;
            string spellingStr = spelling.ToString();
            spelling.Dispose();
            return spellingStr;
        }
        #endregion Type
    }
}
