using System.Globalization;
using System.Threading;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IReferenceGenerator
{
    string Generate(ITypeSymbol type);
    string Generate(string prefix, ITypeSymbol type);
    string Generate(string prefix, ITypeSymbol type, string suffix);
    string Generate(string hardcodedName);
}

internal sealed class ReferenceGenerator : IReferenceGenerator, IScopeInstance
{
    private int _i = -1;
    private readonly int _j;
    private readonly ILocalDiagLogger _localDiagLogger;

    internal ReferenceGenerator(
        IReferenceGeneratorCounter referenceGeneratorCounter,
        ILocalDiagLogger localDiagLogger)
    {
        _j = referenceGeneratorCounter.GetCount();
        _localDiagLogger = localDiagLogger;
    }

    public string Generate(ITypeSymbol type)
    {
        string baseName;
        switch (type)
        {
            case INamedTypeSymbol namedTypeSymbol:
                baseName = $"{char.ToLower(namedTypeSymbol.Name[0], CultureInfo.InvariantCulture).ToString()}{namedTypeSymbol.Name[1..]}";
                break;
            case IArrayTypeSymbol { ElementType: { } elementType }:
                baseName = $"{char.ToLower(elementType.Name[0], CultureInfo.InvariantCulture).ToString()}{elementType.Name[1..]}Array";
                break;
            case ITypeParameterSymbol typeParameterSymbol:
                baseName = $"{char.ToLower(typeParameterSymbol.Name[0], CultureInfo.InvariantCulture).ToString()}{typeParameterSymbol.Name[1..]}";
                break;
            default:
                _localDiagLogger.Warning(WarningLogData.EmptyReferenceNameWarning(
                    $"A reference name couldn't be generated for \"{type.FullName()}\""),
                    Location.None);
                baseName = "empty";
                break;
        }

        return GenerateInner(string.Empty, baseName, string.Empty);
    }

    public string Generate(string prefix, ITypeSymbol type) =>
        GenerateInner(prefix, type.Name, string.Empty);

    public string Generate(string prefix, ITypeSymbol type, string suffix) =>
        GenerateInner(prefix, type.Name, suffix);

    public string Generate(string hardcodedName) => 
        GenerateInner(string.Empty, hardcodedName, string.Empty);
    
    private string GenerateInner(string prefix, string inner, string suffix) => 
        $"{prefix}{inner}{suffix}_{_j.ToString(CultureInfo.InvariantCulture.NumberFormat)}_{Interlocked.Increment(ref _i).ToString(CultureInfo.InvariantCulture.NumberFormat)}";
}
    
internal interface IReferenceGeneratorCounter
{
    int GetCount();
}

internal sealed class ReferenceGeneratorCounter : IReferenceGeneratorCounter, IContainerInstance
{
    private int _j = -1;

    public int GetCount() => Interlocked.Increment(ref _j);
}