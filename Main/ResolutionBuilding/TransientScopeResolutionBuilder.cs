using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeResolutionBuilder : ITransientScopeImplementationResolutionBuilder, IRangeResolutionBaseBuilder
{
    bool HasWorkToDo { get; }
    
    TransientScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    void DoWork();

    TransientScopeResolution Build();
}

internal class TransientScopeResolutionBuilder : RangeResolutionBaseBuilder, ITransientScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly IScopeManager _scopeManager;
    private readonly Func<IRangeResolutionBaseBuilder, IScopeRootParameter, IScopeRootCreateFunctionResolutionBuilder> _scopeRootCreateFunctionResolutionBuilderFactory;
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly Dictionary<string, IScopeRootCreateFunctionResolutionBuilder> _transientScopeRootFunctionResolutions = new ();
    private readonly HashSet<(IScopeRootCreateFunctionResolutionBuilder, string)> _transientScopeRootFunctionQueuedOverloads = new ();
    private readonly Queue<IScopeRootCreateFunctionResolutionBuilder> _transientScopeRootFunctionResolutionsQueue = new();

    internal TransientScopeResolutionBuilder(
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
    }

    public override FunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override FunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, "this");

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(IScopeRootParameter parameter, INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
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
                "this",
                disposableCollectionResolution,
                currentParameters);

    public bool HasWorkToDo => _transientScopeRootFunctionResolutionsQueue.Any() || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo);

    public TransientScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{parameter.KeySuffix()}";
        if (!_transientScopeRootFunctionResolutions.TryGetValue(
                key,
                out var function))
        {
            function = _scopeRootCreateFunctionResolutionBuilderFactory(this, parameter);
            _transientScopeRootFunctionResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_transientScopeRootFunctionQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            _transientScopeRootFunctionResolutionsQueue.Enqueue(function);
            _transientScopeRootFunctionQueuedOverloads.Add((function, listedParameterTypes));
        }

        var transientScopeRootReference = RootReferenceGenerator.Generate("transientScopeRoot");

        return new TransientScopeRootResolution(
            transientScopeRootReference,
            Name,
            containerInstanceScopeReference,
            disposableCollectionResolution,
            function.BuildFunctionCall(currentParameters, transientScopeRootReference));
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            while (_transientScopeRootFunctionResolutionsQueue.Any())
            {
                var functionResolution = _transientScopeRootFunctionResolutionsQueue
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

    public TransientScopeResolution Build() =>
        new(_rootResolutions,
            DisposalHandling,
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _containerReference,
            _containerParameterReference,
            Name);

    public void EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference) => CreateRangedInstanceReferenceResolution(
        parameter,
        label,
        reference,
        "Doesn'tMatter");
}