// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace CppAst
{
    public class CppDiagnosticBag
    {
        private readonly List<CppDiagnosticMessage> _messages;

        public CppDiagnosticBag()
        {
            _messages = new List<CppDiagnosticMessage>();
        }

        public void Clear()
        {
            _messages.Clear();
            HasErrors = false;
        }

        public IReadOnlyList<CppDiagnosticMessage> Messages => _messages;

        public bool HasErrors { get; private set; }

        public void Info(string message, CppSourceLocation? location = null)
        {
            LogMessage(CppLogMessageType.Info, message, location);
        }

        public void Warning(string message, CppSourceLocation? location = null)
        {
            LogMessage(CppLogMessageType.Warning, message, location);
        }

        public void Error(string message, CppSourceLocation? location = null)
        {
            LogMessage(CppLogMessageType.Error, message, location);
        }

        public void Log(CppDiagnosticMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (message.Type == CppLogMessageType.Error)
            {
                HasErrors = true;
            }

            _messages.Add(message);
        }

        public void CopyTo(CppDiagnosticBag dest)
        {
            if (dest == null) throw new ArgumentNullException(nameof(dest));
            foreach (var cppDiagnosticMessage in Messages)
            {
                dest.Log(cppDiagnosticMessage);
            }
        }

        protected void LogMessage(CppLogMessageType type, string message, CppSourceLocation? location = null)
        {
            // Try to recover a proper location
            var locationResolved = location ?? new CppSourceLocation(); // In case we have an unexpected BuilderException, use this location instead
            Log(new CppDiagnosticMessage(type, message, locationResolved));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var diagnostics = new StringBuilder();

            foreach (var message in Messages)
            {
                diagnostics.AppendLine(message.ToString());
            }

            return diagnostics.ToString();
        }
    }
}