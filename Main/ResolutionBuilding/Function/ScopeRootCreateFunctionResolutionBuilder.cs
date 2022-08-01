namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IScopeRootCreateFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class ScopeRootCreateFunctionResolutionBuilder : FunctionResolutionBuilder, IScopeRootCreateFunctionResolutionBuilder
{
    private readonly SwitchImplementationParameter _parameter;

    public ScopeRootCreateFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        SwitchImplementationParameter parameter,
        IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        IFunctionCycleTracker functionCycleTracker)
        : base(
            rangeResolutionBaseBuilder, 
            parameter.ReturnType, 
            parameter.CurrentParameters,
            synchronicityDecisionMaker,
            new object(),
            
            wellKnownTypes, 
            referenceGeneratorFactory, 
            functionCycleTracker)
    {
        _parameter = parameter;

        Name = RootReferenceGenerator.Generate("Create");
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchImplementation(
        _parameter with
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
            Constants.InternalKeyword,
            Resolvable.Value,
            Parameters,
            SynchronicityDecision.Value);
    }
}