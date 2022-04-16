namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IContainerCreateFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class ContainerCreateFunctionResolutionBuilder : FunctionResolutionBuilder, IContainerCreateFunctionResolutionBuilder
{
    private readonly INamedTypeSymbol _returnType;

    public ContainerCreateFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        INamedTypeSymbol returnType,
        IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
        
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
        : base(rangeResolutionBaseBuilder, returnType, Array.Empty<(ITypeSymbol, ParameterResolution)>(), synchronicityDecisionMaker, wellKnownTypes, referenceGeneratorFactory, localFunctionResolutionBuilderFactory)
    {
        _returnType = returnType;
        
        Name = RootReferenceGenerator.Generate("Create");
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchType(new SwitchTypeParameter(
        _returnType,
        Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>())).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            Array.Empty<ParameterResolution>(),
            LocalFunctions
                .Select(lf => lf.Build())
                .Select(f => new LocalFunctionResolution(
                    f.Reference,
                    f.TypeFullName,
                    f.Resolvable,
                    f.Parameter,
                    f.LocalFunctions,
                    f.SynchronicityDecision))
                .ToList(),
            SynchronicityDecision.Value);
    }
}