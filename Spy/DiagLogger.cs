using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IDiagLogger
    {
        void Log(string message);
    }

    internal class DiagLogger : IDiagLogger
    {
        private readonly GeneratorExecutionContext _context;

        public DiagLogger(
            GeneratorExecutionContext context)
        {
            _context = context;
        }

        private void Log(int id, string title, string message, string category, DiagnosticSeverity diagnosticSeverity) =>
            _context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor($"DIESPY{id.ToString().PadLeft(3, '0')}", title, message, category, diagnosticSeverity, true),
                Location.None));

        public void Log(string message) => Log(0, "INFO", message, "INFO", DiagnosticSeverity.Warning);
    }
}
