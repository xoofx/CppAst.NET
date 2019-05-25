using System;
using CppAst;

namespace CppAst
{
    public enum CppLogMessageType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    /// <summary>
    /// Provides a diagnostic message for a specific location in the source code.
    /// </summary>
    public class CppDiagnosticMessage
    {
        public CppDiagnosticMessage(CppLogMessageType type, string text, CppSourceLocation location)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Type = type;
            Text = text;
            Location = location;
        }

        public readonly CppLogMessageType Type;

        public readonly string Text;

        public readonly CppSourceLocation Location;

        public override string ToString()
        {
            return $"{Location}: {Type.ToString().ToLowerInvariant()}: {Text}";
        }
    }
}