namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes);

    RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);

    ContainerResolution Build();
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder, ITransientScopeImplementationResolutionBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;

    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly IScopeResolutionBuilder _scopeResolutionBuilder;
    private readonly string _transientScopeAdapterReference;
    private readonly ITransientScopeResolutionBuilder _transientScopeResolutionBuilder;

    internal ContainerResolutionBuilder(
        // parameters
        IContainerInfo containerInfo,
        
        // dependencies
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        ICheckDecorators checkDecorators,
        WellKnownTypes wellKnownTypes,
        Func<IContainerResolutionBuilder, ITransientScopeInterfaceResolutionBuilder, ITransientScopeResolutionBuilder> transientScopeResolutionBuilderFactory, 
        Func<IContainerResolutionBuilder, ITransientScopeResolutionBuilder, ITransientScopeInterfaceResolutionBuilder, IScopeResolutionBuilder> scopeResolutionBuilderFactory, 
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
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _transientScopeResolutionBuilder = transientScopeResolutionBuilderFactory(this, _transientScopeInterfaceResolutionBuilder);
        _scopeResolutionBuilder = scopeResolutionBuilderFactory(this, _transientScopeResolutionBuilder, _transientScopeInterfaceResolutionBuilder);
        if (_transientScopeResolutionBuilder is TransientScopeResolutionBuilder transientScopeResolutionBuilder)
            transientScopeResolutionBuilder.ScopeResolutionBuilder = _scopeResolutionBuilder;
        
        transientScopeInterfaceResolutionBuilder.AddImplementation(this);
        transientScopeInterfaceResolutionBuilder.AddImplementation(_transientScopeResolutionBuilder);
        _transientScopeAdapterReference = RootReferenceGenerator.Generate("TransientScopeAdapter");
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

    protected override RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, "this");

    protected override TransientScopeRootResolution CreateTransientScopeRootResolution(IScopeRootParameter parameter, INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _transientScopeResolutionBuilder.AddCreateResolveFunction(
            parameter,
            rootType, 
            "this",
            disposableCollectionResolution,
            currentParameters);

    protected override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeResolutionBuilder.AddCreateResolveFunction(
            parameter,
            rootType, 
            "this",
            _transientScopeAdapterReference,
            disposableCollectionResolution,
            currentParameters);

    public ContainerResolution Build()
    {
        while (RangedInstanceResolutionsQueue.Any() 
               || _transientScopeResolutionBuilder.HasWorkToDo 
               || _scopeResolutionBuilder.HasWorkToDo)
        {
            DoRangedInstancesWork();
            _transientScopeResolutionBuilder.DoWork();
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
            _transientScopeInterfaceResolutionBuilder.Build(),
            _transientScopeAdapterReference,
            _transientScopeResolutionBuilder.Build(),
            _scopeResolutionBuilder.Build());
    }

    public void EnqueueRangedInstanceResolution(RangedInstanceResolutionsQueueItem item) => RangedInstanceResolutionsQueue.Enqueue(item);
}