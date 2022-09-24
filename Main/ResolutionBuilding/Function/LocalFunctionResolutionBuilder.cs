namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface ICreateFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class CreateFunctionResolutionBuilder : FunctionResolutionBuilder, ICreateFunctionResolutionBuilder
{
    private readonly ITypeSymbol _returnType;
    private readonly string _accessModifier;

    public CreateFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        ITypeSymbol returnType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> parameters,
        IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
        string accessModifier,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        IFunctionCycleTracker functionCycleTracker,
        IDiagLogger diagLogger)
        : base(
            rangeResolutionBaseBuilder, 
            returnType, 
            parameters,
            synchronicityDecisionMaker, 
            new object(),
            
            wellKnownTypes,
            referenceGeneratorFactory,
            functionCycleTracker,
            diagLogger)
    {
        _returnType = returnType;
        _accessModifier = accessModifier;

        Name = RootReferenceGenerator.Generate("Create", _returnType);
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchType(new SwitchTypeParameter(
        _returnType, 
        ImmutableSortedDictionary.CreateRange(
            CurrentParameters.Select(t => new KeyValuePair<string, (ITypeSymbol, ParameterResolution)>(
                t.Item1.FullName(),
                t))), 
        ImmutableStack<INamedTypeSymbol>.Empty)).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        return new(
            Name,
            TypeFullName,
            _accessModifier,
            Resolvable.Value,
            Parameters,
            SynchronicityDecision.Value);
    }
}