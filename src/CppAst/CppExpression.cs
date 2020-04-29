// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    /// <summary>
    /// Base class for expressions used in <see cref="CppField.InitExpression"/> and <see cref="CppParameter.InitExpression"/>
    /// </summary>
    public abstract class CppExpression : CppElement
    {
        protected CppExpression(CppExpressionKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Gets the kind of this expression.
        /// </summary>
        public CppExpressionKind Kind { get; }

        /// <summary>
        /// Gets the arguments of this expression. Might be null.
        /// </summary>
        public List<CppExpression> Arguments { get; set; }

        /// <summary>
        /// Adds an argument to this expression.
        /// </summary>
        /// <param name="arg">An argument</param>
        public void AddArgument(CppExpression arg)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg));
            if (Arguments == null) Arguments = new List<CppExpression>();
            Arguments.Add(arg);
        }

        protected void ArgumentsSeparatedByCommaToString(StringBuilder builder)
        {
            if (Arguments != null)
            {
                for (var i = 0; i < Arguments.Count; i++)
                {
                    var expression = Arguments[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(expression);
                }
            }
        }
    }

    /// <summary>
    /// An expression that is not exposed in details but only through a list of <see cref="CppToken"/>
    /// and a textual representation
    /// </summary>
    public class CppRawExpression : CppExpression
    {
        public CppRawExpression(CppExpressionKind kind) : base(kind)
        {
            Tokens = new List<CppToken>();
        }

        /// <summary>
        /// Gets the tokens associated to this raw expression.
        /// </summary>
        public List<CppToken> Tokens { get; }

        /// <summary>
        /// Gets or sets a textual representation from the tokens. 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Update the <see cref="Text"/> representation from the <see cref="Tokens"/>.
        /// </summary>
        public void UpdateTextFromTokens()
        {
            Text = CppToken.TokensToString(Tokens);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// A C++ Init list expression `{ a, b, c }`
    /// </summary>
    public class CppInitListExpression : CppExpression
    {
        public CppInitListExpression() : base(CppExpressionKind.InitList)
        {
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("{");
            ArgumentsSeparatedByCommaToString(builder);
            builder.Append("}");
            return builder.ToString();
        }
    }

    /// <summary>
    /// A binary expression
    /// </summary>
    public class CppBinaryExpression : CppExpression
    {
        public CppBinaryExpression(CppExpressionKind kind) : base(kind)
        {
        }

        /// <summary>
        /// The binary operator as a string.
        /// </summary>
        public string Operator { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Arguments != null && Arguments.Count > 0)
            {
                builder.Append(Arguments[0]);
            }

            builder.Append(" ");
            builder.Append(Operator);
            builder.Append(" ");

            if (Arguments != null && Arguments.Count > 1)
            {
                builder.Append(Arguments[1]);
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// A unary expression.
    /// </summary>
    public class CppUnaryExpression : CppExpression
    {
        public CppUnaryExpression(CppExpressionKind kind) : base(kind)
        {
        }

        /// <summary>
        /// The unary operator as a string.
        /// </summary>
        public string Operator { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Operator);
            if (Arguments != null && Arguments.Count > 0)
            {
                builder.Append(Arguments[0]);
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// An expression surrounding another expression by parenthesis.
    /// </summary>
    public class CppParenExpression : CppExpression
    {
        public CppParenExpression() : base(CppExpressionKind.Paren)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("(");
            ArgumentsSeparatedByCommaToString(builder);
            builder.Append(")");
            return builder.ToString();
        }
    }

    /// <summary>
    /// A literal expression.
    /// </summary>
    public class CppLiteralExpression : CppExpression
    {
        public CppLiteralExpression(CppExpressionKind kind, string value) : base(kind)
        {
            Value = value;
        }

        /// <summary>
        /// A textual representation of the literal value.
        /// </summary>
        public string Value { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }
    }
}