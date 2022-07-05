namespace MrMeeseeks.DIE;

internal interface IDiagLogger
{
    void Error(DieException exception);

    void Log(Diagnostic diagnostic);
}

internal class DiagLogger : IDiagLogger
{
    private readonly bool _ignoreErrors;
    private readonly GeneratorExecutionContext _context;

    internal DiagLogger(
        bool ignoreErrors,
        GeneratorExecutionContext context)
    {
        _ignoreErrors = ignoreErrors;
        _context = context;
    }
    
    public void Error(DieException exception)
    {
        switch (exception)
        {
            case ImplementationCycleDieException implementationCycle:
                _context.ReportDiagnostic(Diagnostics.CircularReferenceInsideFactory(implementationCycle));
                break;
            case FunctionCycleDieException:
                _context.ReportDiagnostic(Diagnostics.CircularReferenceAmongFactories);
                break;
            case ValidationDieException:
                break;
            case CompilationDieException slippedResolution:
                _context.ReportDiagnostic(slippedResolution.Diagnostic);
                break;
            default:
                _context.ReportDiagnostic(Diagnostics.UnexpectedException(exception));
                break;
        }
    }

    public void Log(Diagnostic diagnostic)
    {
        if (!_ignoreErrors || diagnostic.Severity != DiagnosticSeverity.Error)
            _context.ReportDiagnostic(diagnostic);
    }
}