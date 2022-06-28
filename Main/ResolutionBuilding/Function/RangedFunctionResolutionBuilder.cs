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
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        IFunctionCycleTracker functionCycleTracker)
        : base(
            rangeResolutionBaseBuilder,
            forConstructorParameter.ImplementationType,
            forConstructorParameter.CurrentFuncParameters, 
            synchronicityDecisionMaker, 
            handleIdentity,
            
            wellKnownTypes, 
            referenceGeneratorFactory, 
            functionCycleTracker)
    {
        _forConstructorParameter = forConstructorParameter;
        
        Name = reference;
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => CreateConstructorResolution(
        _forConstructorParameter with
        {
            ImplementationStack = ImmutableStack<INamedTypeSymbol>.Empty,
            CurrentFuncParameters = CurrentParameters
        }).Item1;

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