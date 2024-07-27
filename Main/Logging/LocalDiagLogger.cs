using System.Globalization;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.Logging;

internal sealed class LocalDiagLogger : IScopeInstance
{
    private readonly ILogEnhancer _logEnhancer;
    private readonly DiagLogger _diagLogger;

    internal LocalDiagLogger(
        ILogEnhancer logEnhancer,
        DiagLogger diagLogger)
    {
        _logEnhancer = logEnhancer;
        _diagLogger = diagLogger;
    }

    private static string CreateId(DiagLogData data) =>
        $"{Constants.DieAbbreviation}_{data.MajorNumber.ToString(CultureInfo.InvariantCulture.NumberFormat).PadLeft(2, '0')}_{data.MinorNumber.ToString(CultureInfo.InvariantCulture.NumberFormat).PadLeft(2, '0')}";

    internal void Warning(DiagLogData data, Location location) =>
        _diagLogger.Log(Diagnostic.Create(new DiagnosticDescriptor(
                CreateId(data),
                data.Title,
                _logEnhancer.Enhance(data.Message),
                "Warning",
                DiagnosticSeverity.Warning,
                true),
            location));

    internal void Error(DiagLogData data, Location location) =>
        _diagLogger.Error(Diagnostic.Create(new DiagnosticDescriptor(
                CreateId(data),
                data.Title,
                _logEnhancer.Enhance(data.Message),
                "Error",
                DiagnosticSeverity.Error,
                true),
            location),
            data.Kind);
}