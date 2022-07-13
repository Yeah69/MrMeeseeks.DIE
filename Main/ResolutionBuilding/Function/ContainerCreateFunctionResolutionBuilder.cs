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
        IFunctionCycleTracker functionCycleTracker)
        : base(
            rangeResolutionBaseBuilder, 
            returnType, 
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty,
            synchronicityDecisionMaker,
            new object(),
            
            wellKnownTypes, 
            referenceGeneratorFactory, 
            functionCycleTracker)
    {
        _returnType = returnType;
        
        Name = RootReferenceGenerator.Generate("Create");
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchType(new SwitchTypeParameter(
        _returnType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty,
        ImmutableStack<INamedTypeSymbol>.Empty)).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            Array.Empty<ParameterResolution>(),
            SynchronicityDecision.Value);
    }
}