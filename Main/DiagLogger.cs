using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MrMeeseeks.DIE
{
    internal interface IDiagLogger
    {
        void Log(int id, string message);

        void Log(int id, string title, string message, string category, DiagnosticSeverity diagnosticSeverity);
    }

    internal class DiagLogger : IDiagLogger
    {
        private readonly GeneratorExecutionContext context;

        public DiagLogger(
            GeneratorExecutionContext context)
        {
            this.context = context;
        }

        public void Log(int id, string title, string message, string category, DiagnosticSeverity diagnosticSeverity)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                   new DiagnosticDescriptor($"DIE{id.ToString().PadLeft(3, '0')}", title, message, category, diagnosticSeverity, true),
                   Location.None));
        }

        public void Log(int id, string message) => Log(id, "INFO", message, "INFO", DiagnosticSeverity.Warning);
    }
}
