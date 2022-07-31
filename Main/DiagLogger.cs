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
                Log(Diagnostics.CircularReferenceInsideFactory(implementationCycle));
                break;
            case FunctionCycleDieException:
                Log(Diagnostics.CircularReferenceAmongFactories);
                break;
            case ValidationDieException validationDieException:
                foreach (var error in validationDieException.Diagnostics)
                    Log(error);
                break;
            case CompilationDieException slippedResolution:
                Log(slippedResolution.Diagnostic);
                break;
            default:
                Log(Diagnostics.UnexpectedDieException(exception));
                break;
        }
    }

    public void Log(Diagnostic diagnostic)
    {
        if (!_ignoreErrors || diagnostic.Severity != DiagnosticSeverity.Error)
            _context.ReportDiagnostic(diagnostic);
    }
}