namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IContainerCreateFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class ContainerCreateFunctionResolutionBuilder : FunctionResolutionBuilder, IContainerCreateFunctionResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;
    private readonly INamedTypeSymbol _returnType;

    public ContainerCreateFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        INamedTypeSymbol returnType,
        
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
        : base(rangeResolutionBaseBuilder, returnType, Array.Empty<(ITypeSymbol, ParameterResolution)>(), wellKnownTypes, referenceGeneratorFactory, localFunctionResolutionBuilderFactory)
    {
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
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
            _rangeResolutionBaseBuilder.DisposalHandling,
            LocalFunctions
                .Select(lf => lf.Build())
                .Select(f => new LocalFunctionResolution(
                    f.Reference,
                    f.TypeFullName,
                    f.Resolvable,
                    f.Parameter,
                    f.DisposalHandling,
                    f.LocalFunctions,
                    f.SynchronicityDecision))
                .ToList(),
            SynchronicityDecision.Value);
    }
}