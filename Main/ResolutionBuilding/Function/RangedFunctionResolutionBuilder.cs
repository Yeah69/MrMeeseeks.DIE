namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IRangedFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class RangedFunctionResolutionBuilder : FunctionResolutionBuilder, IRangedFunctionResolutionBuilder
{
    private readonly ForConstructorParameter _forConstructorParameter;

    public RangedFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        string reference,
        ForConstructorParameter forConstructorParameter,
        IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
        object handleIdentity,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        WellKnownTypesCollections wellKnownTypesCollections, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        IFunctionCycleTracker functionCycleTracker,
        IDiagLogger diagLogger)
        : base(
            rangeResolutionBaseBuilder,
            forConstructorParameter.ImplementationType,
            forConstructorParameter.CurrentParameters, 
            synchronicityDecisionMaker, 
            handleIdentity,
            
            wellKnownTypes, 
            wellKnownTypesCollections,
            referenceGeneratorFactory, 
            functionCycleTracker,
            diagLogger)
    {
        _forConstructorParameter = forConstructorParameter;

        Name = reference;
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => CreateConstructorResolution(
        _forConstructorParameter with
        {
            ImplementationStack = ImmutableStack<INamedTypeSymbol>.Empty,
            CurrentParameters = ImmutableSortedDictionary.CreateRange(
                CurrentParameters.Select(t => new KeyValuePair<string, (ITypeSymbol, ParameterResolution)>(
                    t.Item1.FullName(),
                    t)))
        }).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        
        return new(
            Name,
            TypeFullName,
            Constants.PrivateKeyword,
            Resolvable.Value,
            Parameters,
            SynchronicityDecision.Value);
    }
}