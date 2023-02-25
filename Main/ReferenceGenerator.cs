using System.Threading;
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

internal class ReferenceGenerator : IReferenceGenerator, IScopeInstance
{
    private int _i = -1;
    private readonly int _j;
    private readonly IDiagLogger _diagLogger;

    internal ReferenceGenerator(
        // parameters
        int j,
        
        // dependencies
        IDiagLogger diagLogger)
    {
        _j = j;
        _diagLogger = diagLogger;
    }

    public string Generate(ITypeSymbol type)
    {
        string baseName;
        switch (type)
        {
            case INamedTypeSymbol namedTypeSymbol:
                baseName = $"{char.ToLower(namedTypeSymbol.Name[0]).ToString()}{namedTypeSymbol.Name.Substring(1)}";
                break;
            case IArrayTypeSymbol { ElementType: { } elementType }:
                baseName = $"{char.ToLower(elementType.Name[0]).ToString()}{elementType.Name.Substring(1)}Array";
                break;
            default:
                _diagLogger.Log(Diagnostics.EmptyReferenceNameWarning(
                    $"A reference name couldn't be generated for \"{type.FullName()}\"", 
                    ExecutionPhase.Resolution));
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
        $"{prefix}{inner}{suffix}_{_j.ToString()}_{Interlocked.Increment(ref _i).ToString()}";
}
    
internal interface IReferenceGeneratorFactory
{
    IReferenceGenerator Create();
}

internal class ReferenceGeneratorFactory : IReferenceGeneratorFactory, IContainerInstance
{
    private readonly Func<int, IReferenceGenerator> _referenceGeneratorFactory;
    private int _j = -1;
        
    public ReferenceGeneratorFactory(Func<int, IReferenceGenerator> referenceGeneratorFactory) => _referenceGeneratorFactory = referenceGeneratorFactory;

    public IReferenceGenerator Create() => _referenceGeneratorFactory(Interlocked.Increment(ref _j));
}