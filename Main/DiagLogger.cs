using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE;

internal interface IDiagLogger
{
    bool ErrorsIssued { get; }
    void Reset();
    void Error(DieException exception, ExecutionPhase phase);

    void Log(Diagnostic diagnostic);
}

internal class DiagLogger : IDiagLogger, IContainerInstance
{
    private readonly bool _ignoreErrors;
    private readonly GeneratorExecutionContext _context;

    internal DiagLogger(
        IGeneratorConfiguration generatorConfiguration,
        GeneratorExecutionContext context)
    {
        _ignoreErrors = generatorConfiguration.ErrorDescriptionInsteadOfBuildFailure;
        _context = context;
    }

    public bool ErrorsIssued { get; private set; }

    public void Reset() => ErrorsIssued = false;

    public void Error(DieException exception, ExecutionPhase phase)
    {
        switch (exception)
        {
            case ImplementationCycleDieException implementationCycle:
                Log(Diagnostics.CircularReferenceInsideFactory(implementationCycle, phase));
                break;
            case FunctionCycleDieException functionCycleDieException:
                Log(Diagnostics.CircularReferenceAmongFactories(functionCycleDieException, phase));
                break;
            case InitializedInstanceCycleDieException initializedInstanceCycleDieException:
                Log(Diagnostics.CircularReferenceAmongInitializedInstances(initializedInstanceCycleDieException, phase));
                break;
            case ValidationDieException validationDieException:
                foreach (var error in validationDieException.Diagnostics)
                    Log(error);
                break;
            case ResolutionDieException resolutionDieException:
                Log(Diagnostics.ResolutionException(resolutionDieException));
                break;
            case CompilationDieException slippedResolution:
                Log(slippedResolution.Diagnostic);
                break;
            case ImpossibleDieException impossibleDieException:
                Log(Diagnostics.ImpossibleException(impossibleDieException, phase));
                break;
            default:
                Log(Diagnostics.UnexpectedDieException(exception, phase));
                break;
        }

        ErrorsIssued = true;
    }

    public void Log(Diagnostic diagnostic)
    {
        if (!_ignoreErrors || diagnostic.Severity != DiagnosticSeverity.Error)
            _context.ReportDiagnostic(diagnostic);
    }
}