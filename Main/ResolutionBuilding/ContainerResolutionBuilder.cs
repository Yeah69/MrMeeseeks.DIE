namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes);

    RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(
        RangeResolutionBaseBuilder.ForConstructorParameter parameter,
        string containerReference);

    ContainerResolution Build();
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder
{
    private readonly IContainerInfo _containerInfo;
    
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly IScopeResolutionBuilder _scopeResolutionBuilder;

    internal ContainerResolutionBuilder(
        // parameters
        IContainerInfo containerInfo,
        
        // dependencies
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        ICheckDecorators checkDecorators,
        WellKnownTypes wellKnownTypes,
        Func<IContainerResolutionBuilder, IScopeResolutionBuilder> scopeResolutionBuilderFactory, 
        IUserProvidedScopeElements userProvidedScopeElements) 
        : base(
            (containerInfo.Name, false), 
            wellKnownTypes, 
            typeToImplementationsMapper,
            referenceGeneratorFactory, 
            checkTypeProperties,
            checkDecorators,
            userProvidedScopeElements)
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
                SwitchType(new SwitchTypeParameter(
                    typeSymbol,
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>())),
                Array.Empty<ParameterResolution>(),
                WellKnownTypes.Container.Construct(typeSymbol).FullName(),
                _containerInfo.Name,
                DisposalHandling));
    }

    public RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Container",
            containerReference);

    protected override RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        CreateContainerInstanceReferenceResolution(parameter, "this");

    protected override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeResolutionBuilder.AddCreateResolveFunction(
            parameter,
            rootType, 
            "this",
            disposableCollectionResolution,
            currentParameters);

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