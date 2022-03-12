using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IScopeResolutionBuilder : IRangeResolutionBaseBuilder
{
    bool HasWorkToDo { get; }
    
    ScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        string transientInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    void DoWork();

    ScopeResolution Build();
}

internal class ScopeResolutionBuilder : RangeResolutionBaseBuilder, IScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly IScopeManager _scopeManager;
    private readonly Func<IRangeResolutionBaseBuilder, IScopeRootParameter, IScopeRootCreateFunctionResolutionBuilder> _scopeRootCreateFunctionResolutionBuilderFactory;
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    private readonly string _transientScopeReference;
    private readonly string _transientScopeParameterReference;
    
    private readonly Dictionary<string, IScopeRootCreateFunctionResolutionBuilder> _scopeRootFunctionResolutions = new ();
    private readonly HashSet<(IScopeRootCreateFunctionResolutionBuilder, string)> _scopeRootFunctionQueuedOverloads = new ();
    private readonly Queue<IScopeRootCreateFunctionResolutionBuilder> _scopeRootFunctionResolutionsQueue = new();
    
    internal ScopeResolutionBuilder(
        // parameter
        string name,
        IContainerResolutionBuilder containerResolutionBuilder,
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        IScopeManager scopeManager,
        IUserProvidedScopeElements userProvidedScopeElements, 
        ICheckTypeProperties checkTypeProperties,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, IScopeRootParameter, IScopeRootCreateFunctionResolutionBuilder> scopeRootCreateFunctionResolutionBuilderFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory) 
        : base(
            name,
            checkTypeProperties,
            userProvidedScopeElements,
            wellKnownTypes, 
            referenceGeneratorFactory,
            rangedFunctionGroupResolutionBuilderFactory)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _scopeManager = scopeManager;
        _scopeRootCreateFunctionResolutionBuilderFactory = scopeRootCreateFunctionResolutionBuilderFactory;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
        _transientScopeReference = RootReferenceGenerator.Generate("_transientScope");
        _transientScopeParameterReference = RootReferenceGenerator.Generate("transientScope");
    }

    public override FunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override FunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, _transientScopeReference);

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetTransientScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType,
                _containerReference,
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
                _containerReference, 
                _transientScopeReference,
                disposableCollectionResolution,
                currentParameters);

    public bool HasWorkToDo => _scopeRootFunctionResolutionsQueue.Any() || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo);

    public ScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        string transientInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{parameter.KeySuffix()}";
        if (!_scopeRootFunctionResolutions.TryGetValue(
                key,
                out var function))
        {
            function = _scopeRootCreateFunctionResolutionBuilderFactory(this, parameter);
            _scopeRootFunctionResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_scopeRootFunctionQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            _scopeRootFunctionResolutionsQueue.Enqueue(function);
            _scopeRootFunctionQueuedOverloads.Add((function, listedParameterTypes));
        }

        var scopeRootReference = RootReferenceGenerator.Generate("scopeRoot");

        return new ScopeRootResolution(
            scopeRootReference,
            Name,
            containerInstanceScopeReference,
            transientInstanceScopeReference,
            disposableCollectionResolution,
            function.BuildFunctionCall(currentParameters, scopeRootReference));
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            while (_scopeRootFunctionResolutionsQueue.Any())
            {
                var functionResolution = _scopeRootFunctionResolutionsQueue
                    .Dequeue()
                    .Build();
                
                _rootResolutions.Add(new RootResolutionFunction(
                    functionResolution.Reference,
                    functionResolution.TypeFullName,
                    "internal",
                    functionResolution.Resolvable,
                    functionResolution.Parameter,
                    functionResolution.DisposalHandling,
                    functionResolution.LocalFunctions));
            }
            
            DoRangedInstancesWork();
        }
    }

    public ScopeResolution Build() =>
        new(_rootResolutions,
            DisposalHandling,
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _containerReference,
            _containerParameterReference,
            _transientScopeReference,
            _transientScopeParameterReference,
            Name);
}