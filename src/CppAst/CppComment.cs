// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// Top level comment container.
    /// </summary>
    public class CppCommentFull : CppComment
    {
        public CppCommentFull() : base(CppCommentKind.Full)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            ChildrenToString(builder);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// Base class for all comments.
    /// </summary>
    public abstract class CppComment
    {
        protected CppComment(CppCommentKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// The kind of comments.
        /// </summary>
        public CppCommentKind Kind { get; }

        /// <summary>
        /// Gets a list of children. Might be null.
        /// </summary>
        public List<CppComment> Children { get; set; }

        protected internal abstract void ToString(StringBuilder builder);

        protected void ChildrenToString(StringBuilder builder)
        {
            if (Children != null)
            {
                foreach (var children in Children)
                {
                    children.ToString(builder);
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }

        public string ChildrenToString()
        {
            var builder = new StringBuilder();
            ChildrenToString(builder);
            return builder.ToString();
        }
    }

    /// <summary>
    /// A comment that is a command (e.g `@param arg1`)
    /// </summary>
    public abstract class CppCommentCommand : CppComment
    {
        protected CppCommentCommand(CppCommentKind kind) : base(kind)
        {
            Arguments = new List<string>();
        }

        public string CommandName { get; set; }

        public List<string> Arguments { get; }

        protected internal override void ToString(StringBuilder builder)
        {
            builder.Append($"@{CommandName}");
            for (var index = 0; index < Arguments.Count; index++)
            {
                var argument = Arguments[index];
                builder.Append(" ");
                builder.Append(argument);
            }
            builder.Append(" ");
        }
    }

    /// <summary>
    /// A comment paragraph.
    /// </summary>
    public class CppCommentParagraph : CppComment
    {
        public CppCommentParagraph() : base(CppCommentKind.Paragraph)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            if (Children != null)
            {
                for (var i = 0; i < Children.Count; i++)
                {
                    var children = Children[i];
                    children.ToString(builder);
                    // If a text is followed by a text, we assume that it was a new line
                    // between the two
                    if (children.Kind == CppCommentKind.Text && i + 1 < Children.Count && Children[i + 1].Kind == CppCommentKind.Text)
                    {
                        var text = ((CppCommentText)children).Text;
                        var nextText = ((CppCommentText)children).Text;
                        if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(nextText))
                        {
                            builder.AppendLine();
                        }
                    }
                }
            }
            builder.AppendLine();
        }
    }

    /// <summary>
    /// A comment block command (`@code ... @endcode`)
    /// </summary>
    public class CppCommentBlockCommand : CppCommentCommand
    {
        public CppCommentBlockCommand() : base(CppCommentKind.BlockCommand)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            ChildrenToString(builder);
        }
    }

    /// <summary>
    /// An inline comment command.
    /// </summary>
    public class CppCommentInlineCommand : CppCommentCommand
    {
        public CppCommentInlineCommand() : base(CppCommentKind.InlineCommand)
        {
        }

        public CppCommentInlineCommandRenderKind RenderKind { get; set; }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            ChildrenToString(builder);
        }
    }

    /// <summary>
    /// Type of rendering for an <see cref="CppCommentInlineCommand"/>
    /// </summary>
    public enum CppCommentInlineCommandRenderKind
    {
        Normal,
        Bold,
        Monospaced,
        Emphasized,
    }

    /// <summary>
    /// A comment for a function/method parameter.
    /// </summary>
    public class CppCommentParamCommand : CppCommentCommand
    {
        public CppCommentParamCommand() : base(CppCommentKind.ParamCommand)
        {
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if the <see cref="ParamIndex"/> is valid.
        /// </summary>
        public bool IsParamIndexValid { get; set; }

        /// <summary>
        /// Gets or sets the index of this parameter in the function parameters.
        /// </summary>
        public int ParamIndex { get; set; }

        /// <summary>
        /// Gets or sets the direction of this parameter (in, out, inout).
        /// </summary>
        public CppCommentParamDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if <see cref="Direction"/> was explicitly specified.
        /// </summary>
        public bool IsDirectionExplicit { get; set; }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            builder.Append(ParamName);
            builder.Append(" ");
            ChildrenToString(builder);
        }
    }


    /// <summary>
    /// A comment for a template parameter command.
    /// </summary>
    public class CppCommentTemplateParamCommand : CppCommentCommand
    {
        public CppCommentTemplateParamCommand() : base(CppCommentKind.TemplateParamCommand)
        {
        }

        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// Depth or this parameter.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if this <see cref="Index"/> is valid
        /// </summary>
        public bool IsPositionValid { get; set; }

        /// <summary>
        /// Gets or sets the index of this template parameter.
        /// </summary>
        public int Index { get; set; }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            builder.Append(ParamName);
            builder.Append(" ");
            ChildrenToString(builder);
        }
    }

    /// <summary>
    /// Direction used by <see cref="CppCommentParamCommand"/>
    /// </summary>
    public enum CppCommentParamDirection
    {
        In,
        Out,
        InOut,
    }

    /// <summary>
    /// An enumeration for <see cref="CppComment"/>
    /// </summary>
    public enum CppCommentKind
    {
        Null = 0,
        Text = 1,
        InlineCommand = 2,
        HtmlStartTag = 3,
        HtmlEndTag = 4,
        Paragraph = 5,
        BlockCommand = 6,
        ParamCommand = 7,
        TemplateParamCommand = 8,
        VerbatimBlockCommand = 9,
        VerbatimBlockLine = 10,
        VerbatimLine = 11,
        Full = 12,
    }

    /// <summary>
    /// A comment for a verbatim block command.
    /// </summary>
    public class CppCommentVerbatimBlockCommand : CppCommentCommand
    {
        public CppCommentVerbatimBlockCommand() : base(CppCommentKind.VerbatimBlockCommand)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            ChildrenToString(builder);
            builder.AppendLine($"@end{CommandName}");
        }
    }

    /// <summary>
    /// A comment for a verbatim line inside a verbatim block.
    /// </summary>
    public class CppCommentVerbatimBlockLine : CppCommentTextBase
    {
        public CppCommentVerbatimBlockLine() : base(CppCommentKind.VerbatimBlockLine)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            builder.AppendLine();
        }

    }

    /// <summary>
    /// Base class for all text based comments.
    /// </summary>
    public abstract class CppCommentTextBase : CppComment
    {
        protected CppCommentTextBase(CppCommentKind kind) : base(kind)
        {
        }

        public string Text { get; set; }

        protected internal override void ToString(StringBuilder builder)
        {
            builder.Append(Text);
        }
    }

    /// <summary>
    /// A simple text comment entry.
    /// </summary>
    public class CppCommentText : CppCommentTextBase
    {
        public CppCommentText() : base(CppCommentKind.Text)
        {
        }
    }

    /// <summary>
    /// A verbatim line comment.
    /// </summary>
    public class CppCommentVerbatimLine : CppCommentTextBase
    {
        public CppCommentVerbatimLine() : base(CppCommentKind.VerbatimLine)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            base.ToString(builder);
            builder.AppendLine();
        }
    }

    /// <summary>
    /// Base class for an HTML comment start or en tag.
    /// </summary>
    public abstract class CppCommentHtmlTag : CppComment
    {
        protected CppCommentHtmlTag(CppCommentKind kind) : base(kind)
        {
        }

        public string TagName { get; set; }

        protected internal abstract override void ToString(StringBuilder builder);
    }

    /// <summary>
    /// An HTML start comment tag.
    /// </summary>
    public class CppCommentHtmlStartTag : CppCommentHtmlTag
    {
        public CppCommentHtmlStartTag() : base(CppCommentKind.HtmlStartTag)
        {
            Attributes = new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Gets or sets a boolean indicating if this start tag is self closing.
        /// </summary>
        public bool IsSelfClosing { get; set; }

        /// <summary>
        /// Gets the list of HTML attributes attached to this start tag.
        /// </summary>
        public List<KeyValuePair<string, string>> Attributes { get; }

        protected internal override void ToString(StringBuilder builder)
        {
            builder.Append("<");
            builder.Append(TagName);

            foreach (var keyValuePair in Attributes)
            {
                builder.Append(" ");
                builder.Append(keyValuePair.Key);
                builder.Append("=\"");
                builder.Append(keyValuePair.Value);
                builder.Append("\"");
            }

            if (IsSelfClosing)
            {
                builder.Append(" /");
            }
            builder.Append(">");
        }
    }

    /// <summary>
    /// An HTML end comment tag.
    /// </summary>
    public class CppCommentHtmlEndTag : CppCommentHtmlTag
    {
        public CppCommentHtmlEndTag() : base(CppCommentKind.HtmlEndTag)
        {
        }

        protected internal override void ToString(StringBuilder builder)
        {
            builder.Append("</");
            builder.Append(TagName);
            builder.Append(">");
        }
    }
}