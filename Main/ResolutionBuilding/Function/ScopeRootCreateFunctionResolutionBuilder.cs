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
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
        : base(rangeResolutionBaseBuilder, parameter.ReturnType, parameter.CurrentParameters, synchronicityDecisionMaker, wellKnownTypes, referenceGeneratorFactory, localFunctionResolutionBuilderFactory)
    {
        _parameter = parameter;

        Name = RootReferenceGenerator.Generate("Create");
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => SwitchImplementation(
        _parameter with { ImplementationStack = ImmutableStack<INamedTypeSymbol>.Empty }).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            Parameters,
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