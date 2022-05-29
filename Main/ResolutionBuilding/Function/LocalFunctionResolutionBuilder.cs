namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface ILocalFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class LocalFunctionResolutionBuilder : FunctionResolutionBuilder, ILocalFunctionResolutionBuilder
{
    private readonly INamedTypeSymbol _returnType;
    private readonly IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> _parameters;

    public LocalFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        INamedTypeSymbol returnType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> parameters,
        IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        IFunctionCycleTracker functionCycleTracker)
        : base(
            rangeResolutionBaseBuilder, 
            returnType, 
            parameters,
            synchronicityDecisionMaker, 
            new object(),
            
            wellKnownTypes,
            referenceGeneratorFactory,
            functionCycleTracker)
    {
        _returnType = returnType;
        _parameters = parameters;

        Name = RootReferenceGenerator.Generate("Create", _returnType);
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchType(new SwitchTypeParameter(_returnType, _parameters, ImmutableStack<INamedTypeSymbol>.Empty)).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            _parameters.Select(t => t.Resolution).ToList(),
            SynchronicityDecision.Value);
    }
}