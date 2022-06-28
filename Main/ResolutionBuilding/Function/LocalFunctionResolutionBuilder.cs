namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface ILocalFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class LocalFunctionResolutionBuilder : FunctionResolutionBuilder, ILocalFunctionResolutionBuilder
{
    private readonly INamedTypeSymbol _returnType;

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

        Name = RootReferenceGenerator.Generate("Create", _returnType);
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchType(new SwitchTypeParameter(_returnType, CurrentParameters, ImmutableStack<INamedTypeSymbol>.Empty)).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            Parameters,
            SynchronicityDecision.Value);
    }
}