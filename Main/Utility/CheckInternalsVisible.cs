using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Utility;

internal interface ICheckInternalsVisible
{
    bool Check(IAssemblySymbol assembly);
    bool Check(ISymbol symbol);
}

internal class CheckInternalsVisible : ICheckInternalsVisible
{
    private readonly Compilation _compilation;
    private readonly IContainerWideContext _containerWideContext;

    internal CheckInternalsVisible(
        GeneratorExecutionContext generatorExecutionContext,
        IContainerWideContext containerWideContext)
    {
        _compilation = generatorExecutionContext.Compilation;
        _containerWideContext = containerWideContext;
    }
    
    public bool Check(IAssemblySymbol assembly) =>
        CustomSymbolEqualityComparer.Default.Equals(_compilation.Assembly, assembly) 
        ||assembly
            .GetAttributes()
            .Any(ad =>
                CustomSymbolEqualityComparer.Default.Equals(
                    ad.AttributeClass, 
                    _containerWideContext.WellKnownTypes.InternalsVisibleToAttribute)
                && ad.ConstructorArguments.Length == 1
                && ad.ConstructorArguments[0].Value is string assemblyName
                && Equals(assemblyName, _compilation.AssemblyName));

    public bool Check(ISymbol symbol) => 
        Check(symbol.ContainingAssembly);
}