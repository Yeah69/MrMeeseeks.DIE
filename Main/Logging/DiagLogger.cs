using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Logging;

internal interface IDiagLogger
{
    bool ErrorsIssued { get; }
    IReadOnlyList<DieExceptionKind> ErrorKinds { get; }
    IReadOnlyList<string> DieBuildErrorCodes { get; }
    void Error(Diagnostic diagnostic, DieExceptionKind? kind);
    void Log(Diagnostic diagnostic);
}

internal sealed class DiagLogger : IDiagLogger, IContainerInstance
{
    private readonly bool _ignoreErrors;
    private readonly GeneratorExecutionContext _context;
    private readonly List<DieExceptionKind> _errorKinds = [];
    private readonly List<string> _dieBuildErrorCodes = [];

    internal DiagLogger(
        IGeneratorConfiguration generatorConfiguration,
        GeneratorExecutionContext context)
    {
        _ignoreErrors = generatorConfiguration.ErrorDescriptionInsteadOfBuildFailure;
        _context = context;
    }

    public bool ErrorsIssued { get; private set; }
    public IReadOnlyList<DieExceptionKind> ErrorKinds => _errorKinds;
    public IReadOnlyList<string> DieBuildErrorCodes => _dieBuildErrorCodes;

    public void Error(Diagnostic diagnostic, DieExceptionKind? kind)
    {
        if (kind is {} notNull)
            _errorKinds.Add(notNull);
        Log(diagnostic);
        ErrorsIssued = true;
    }

    public void Log(Diagnostic diagnostic)
    {
        _dieBuildErrorCodes.Add(diagnostic.Id);
        if (!_ignoreErrors || diagnostic.Severity != DiagnosticSeverity.Error)
            _context.ReportDiagnostic(diagnostic);
    }
}