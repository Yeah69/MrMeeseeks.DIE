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
        IFunctionCycleTracker functionCycleTracker,
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
        : base(
            rangeResolutionBaseBuilder,
            forConstructorParameter.ImplementationType,
            forConstructorParameter.CurrentFuncParameters, 
            synchronicityDecisionMaker, 
            handleIdentity,
            
            wellKnownTypes, 
            referenceGeneratorFactory, 
            functionCycleTracker,
            localFunctionResolutionBuilderFactory)
    {
        _forConstructorParameter = forConstructorParameter;
        
        Name = reference;
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => CreateConstructorResolution(
        _forConstructorParameter with { ImplementationStack = ImmutableStack<INamedTypeSymbol>.Empty }).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            _forConstructorParameter.CurrentFuncParameters.Select(t => t.Resolution).ToList(),
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