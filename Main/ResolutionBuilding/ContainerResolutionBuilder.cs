using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder : IRangeResolutionBaseBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<(INamedTypeSymbol, string)> createFunctionData);

    FunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);

    ContainerResolution Build();
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder, ITransientScopeImplementationResolutionBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IContainerCreateFunctionResolutionBuilder> _createFunctionResolutionBuilderFactory;

    private readonly List<IContainerCreateFunctionResolutionBuilder> _rootResolutions = new ();
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
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IContainerCreateFunctionResolutionBuilder> createFunctionResolutionBuilderFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory, 
        IUserProvidedScopeElements userProvidedScopeElement) 
        : base(
            containerInfo.Name, 
            checkTypeProperties,
            userProvidedScopeElement,
            wellKnownTypes, 
            referenceGeneratorFactory,
            rangedFunctionGroupResolutionBuilderFactory)
    {
        _containerInfo = containerInfo;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _createFunctionResolutionBuilderFactory = createFunctionResolutionBuilderFactory;
        _scopeManager = scopeManagerFactory(this, transientScopeInterfaceResolutionBuilder);
        
        transientScopeInterfaceResolutionBuilder.AddImplementation(this);
        _transientScopeAdapterReference = RootReferenceGenerator.Generate("TransientScopeAdapter");
    }

    public void AddCreateResolveFunctions(IReadOnlyList<(INamedTypeSymbol, string)> createFunctionData)
    {
        foreach (var typeSymbol in rootTypes)
            _rootResolutions.Add(_createFunctionResolutionBuilderFactory(this, typeSymbol));
    }

    public FunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Container",
            null,
            containerReference);

    public override FunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        CreateContainerInstanceReferenceResolution(parameter, "this");

    public override FunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
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
        var privateRootFunctions = _rootResolutions
            .Select(b => b.Build())
            .Select(f => new RootResolutionFunction(
                f.Reference,
                f.TypeFullName,
                "private",
                f.Resolvable,
                f.Parameter,
                DisposalHandling,
                f.LocalFunctions,
                f.IsAsync))
            .ToList();

        var i = 0;
        var publicRootFunctions = privateRootFunctions
            .Select(f => new RootResolutionFunction(
                $"Create{i++}",
                f.TypeFullName,
                "public",
                new FunctionCallResolution(
                    RootReferenceGenerator.Generate("result"),
                    f.TypeFullName,
                    f.Reference,
                    "this",
                    Array.Empty<(string, string)>()),
                f.Parameter,
                DisposalHandling,
                Array.Empty<LocalFunctionResolution>(),
                f.IsAsync));

        while (RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo)
               || _scopeManager.HasWorkToDo)
        {
            DoRangedInstancesWork();
            _scopeManager.DoWork();
        }
        
        var (transientScopeResolutions, scopeResolutions) = _scopeManager.Build();

        return new(
            privateRootFunctions.Concat(publicRootFunctions).ToList(),
            DisposalHandling,
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _transientScopeInterfaceResolutionBuilder.Build(),
            _transientScopeAdapterReference,
            transientScopeResolutions,
            scopeResolutions);
    }

    public void EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference) => CreateRangedInstanceReferenceResolution(
        parameter,
        label,
        reference,
        "Doesn'tMatter");
}