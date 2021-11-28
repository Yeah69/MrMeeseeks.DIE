namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes);

    RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        string containerReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        RangeResolutionBaseBuilder.Decoration? decoration);

    ContainerResolution Build();
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder
{
    private readonly IContainerInfo _containerInfo;
    
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly IScopeResolutionBuilder _scopeResolutionBuilder;

    public ContainerResolutionBuilder(
        // parameters
        IContainerInfo containerInfo,
        
        // dependencies
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        Func<IContainerResolutionBuilder, IScopeResolutionBuilder> scopeResolutionBuilderFactory) 
        : base((containerInfo.Name, false), wellKnownTypes, typeToImplementationsMapper, referenceGeneratorFactory, checkTypeProperties)
    {
        _containerInfo = containerInfo;
        _scopeResolutionBuilder = scopeResolutionBuilderFactory(this);
    }

    public void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes)
    {
        foreach (var typeSymbol in rootTypes)
            _rootResolutions.Add(new RootResolutionFunction(
                nameof(IContainer<object>.Resolve),
                typeSymbol.FullName(),
                "",
                Create(
                    typeSymbol,
                    ReferenceGeneratorFactory.Create(),
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>()),
                Array.Empty<ParameterResolution>(),
                WellKnownTypes.Container.Construct(typeSymbol).FullName(),
                _containerInfo.Name,
                DisposalHandling));
    }

    public RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        string containerReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Decoration? decoration) =>
        CreateRangedInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            currentParameters,
            "Single",
            containerReference,
            decoration);

    protected override RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Decoration? decoration) =>
        CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator, "this", currentParameters, decoration);

    protected override ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType, 
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        DecorationScopeRoot? decoration) =>
        _scopeResolutionBuilder.AddCreateResolveFunction(
            rootType, 
            referenceGenerator, 
            "this",
            disposableCollectionResolution,
            currentParameters,
            decoration);

    public ContainerResolution Build()
    {
        while (RangedInstanceResolutionsQueue.Any() || _scopeResolutionBuilder.HasWorkToDo)
        {
            DoRangedInstancesWork();
            _scopeResolutionBuilder.DoWork();
        }
        
        return new(
            _rootResolutions,
            DisposalHandling,
            RangedInstances
                .GroupBy(t => t.Item1)
                .Select(g => new RangedInstance(
                    g.Key,
                    g.Select(t => t.Item2).ToList(),
                    DisposalHandling))
                .ToList(),
            _scopeResolutionBuilder.Build());
    }
}