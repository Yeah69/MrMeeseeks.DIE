namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IScopeRootCreateFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class ScopeRootCreateFunctionResolutionBuilder : FunctionResolutionBuilder, IScopeRootCreateFunctionResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;
    private readonly IScopeRootParameter _scopeRootParameter;

    public ScopeRootCreateFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        IScopeRootParameter scopeRootParameter,
        
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
        : base(rangeResolutionBaseBuilder, scopeRootParameter.ReturnType, scopeRootParameter.CurrentParameters, wellKnownTypes, referenceGeneratorFactory, localFunctionResolutionBuilderFactory)
    {
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
        _scopeRootParameter = scopeRootParameter;

        Name = RootReferenceGenerator.Generate("Create");
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => _scopeRootParameter switch
    {
        CreateInterfaceParameter createInterfaceParameter => CreateInterface(createInterfaceParameter).Item1,
        SwitchImplementationParameter switchImplementationParameter => SwitchImplementation(switchImplementationParameter).Item1,
        SwitchInterfaceAfterScopeRootParameter switchInterfaceAfterScopeRootParameter => SwitchInterfaceAfterScopeRoot(switchInterfaceAfterScopeRootParameter).Item1,
        _ => throw new ArgumentOutOfRangeException(nameof(_scopeRootParameter))
    };

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            Parameters,
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