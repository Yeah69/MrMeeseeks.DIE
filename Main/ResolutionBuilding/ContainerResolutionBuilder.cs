using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder : IRangeResolutionBaseBuilder
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
    private readonly Func<IRangeResolutionBaseBuilder, IFunctionResolutionBuilder> _functionResolutionBuilderFactory;

    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _transientScopeAdapterReference;
    private readonly IScopeManager _scopeManager;

    internal ContainerResolutionBuilder(
        // parameters
        IContainerInfo containerInfo,
        
        // dependencies
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        Func<IContainerResolutionBuilder, ITransientScopeInterfaceResolutionBuilder, IScopeManager> scopeManagerFactory,
        Func<IRangeResolutionBaseBuilder, IFunctionResolutionBuilder> functionResolutionBuilderFactory, 
        IUserProvidedScopeElements userProvidedScopeElement) 
        : base(
            containerInfo.Name, 
            checkTypeProperties,
            userProvidedScopeElement,
            wellKnownTypes, 
            referenceGeneratorFactory,
            functionResolutionBuilderFactory)
    {
        _containerInfo = containerInfo;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _functionResolutionBuilderFactory = functionResolutionBuilderFactory;
        _scopeManager = scopeManagerFactory(this, transientScopeInterfaceResolutionBuilder);
        
        transientScopeInterfaceResolutionBuilder.AddImplementation(this);
        _transientScopeAdapterReference = RootReferenceGenerator.Generate("TransientScopeAdapter");
    }

    public void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes)
    {
        var i = 0;
        foreach (var typeSymbol in rootTypes)
            _rootResolutions.Add(new RootResolutionFunction(
                $"Create{i++}",
                typeSymbol.FullName(),
                "public",
                _functionResolutionBuilderFactory(this).ResolveFunction(new SwitchTypeParameter(
                    typeSymbol,
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>())),
                Array.Empty<ParameterResolution>(),
                _containerInfo.Name,
                DisposalHandling));
    }

    public RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Container",
            containerReference);

    public override RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        CreateContainerInstanceReferenceResolution(parameter, "this");

    public override RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, "this");

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(IScopeRootParameter parameter, INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetTransientScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                "this",
                disposableCollectionResolution,
                currentParameters);

    public override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                "this",
                _transientScopeAdapterReference,
                disposableCollectionResolution,
                currentParameters);

    public ContainerResolution Build()
    {
        while (RangedInstanceResolutionsQueue.Any() 
               || _scopeManager.HasWorkToDo)
        {
            DoRangedInstancesWork();
            _scopeManager.DoWork();
        }
        
        var (transientScopeResolutions, scopeResolutions) = _scopeManager.Build();
        
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
            transientScopeResolutions,
            scopeResolutions);
    }

    public void EnqueueRangedInstanceResolution(RangedInstanceResolutionsQueueItem item) => RangedInstanceResolutionsQueue.Enqueue(item);
}