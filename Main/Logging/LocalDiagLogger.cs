using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Logging;

internal interface ILocalDiagLogger
{
    void Warning(DiagLogData data, Location location);
    void Error(DiagLogData data, Location location);
}

internal class LocalDiagLogger : ILocalDiagLogger, IScopeInstance
{
    private readonly IFunctionLevelLogMessageEnhancer _messageEnhancer;
    private readonly IDiagLogger _diagLogger;

    internal LocalDiagLogger(
        IFunctionLevelLogMessageEnhancer messageEnhancer,
        IDiagLogger diagLogger)
    {
        _messageEnhancer = messageEnhancer;
        _diagLogger = diagLogger;
    }

    private string CreateId(DiagLogData data) =>
        $"{Constants.DieAbbreviation}_{data.MajorNumber.ToString().PadLeft(2, '0')}_{data.MinorNumber.ToString().PadLeft(2, '0')}";

    public void Warning(DiagLogData data, Location location) =>
        _diagLogger.Log(Diagnostic.Create(new DiagnosticDescriptor(
                CreateId(data),
                data.Title,
                _messageEnhancer.Enhance(data.Message),
                "Warning",
                DiagnosticSeverity.Warning,
                true),
            location));

    public void Error(DiagLogData data, Location location) =>
        _diagLogger.Error(Diagnostic.Create(new DiagnosticDescriptor(
                CreateId(data),
                data.Title,
                _messageEnhancer.Enhance(data.Message),
                "Error",
                DiagnosticSeverity.Error,
                true),
            location),
            data.Kind);
}