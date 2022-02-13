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
    private readonly Func<IRangeResolutionBaseBuilder, IFunctionResolutionBuilder> _functionResolutionBuilderFactory;
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly Dictionary<string, ScopeRootFunction> _transientScopeRootFunctionResolutions = new ();
    private readonly HashSet<(ScopeRootFunction, string)> _transientScopeRootFunctionQueuedOverloads = new ();
    private readonly Queue<(ScopeRootFunction, IReadOnlyList<ParameterResolution>, INamedTypeSymbol, IScopeRootParameter)> _transientScopeRootFunctionResolutionsQueue = new();

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
        Func<IRangeResolutionBaseBuilder, IFunctionResolutionBuilder> functionResolutionBuilderFactory) 
        : base(
            name, 
            checkTypeProperties,
            userProvidedScopeElements,
            wellKnownTypes, 
            referenceGeneratorFactory,
            functionResolutionBuilderFactory)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _scopeManager = scopeManager;
        _functionResolutionBuilderFactory = functionResolutionBuilderFactory;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
    }

    public override RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
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

    public bool HasWorkToDo => _transientScopeRootFunctionResolutionsQueue.Any() || RangedInstanceResolutionsQueue.Any();

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
                out ScopeRootFunction function))
        {
            function = new ScopeRootFunction(
                RootReferenceGenerator.Generate("Create", rootType, parameter.RootFunctionSuffix()),
                rootType.FullName());
            _transientScopeRootFunctionResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_transientScopeRootFunctionQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var currParameter = currentParameters
                .Select(t => new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName()))
                .ToList();
            _transientScopeRootFunctionResolutionsQueue.Enqueue((function, currParameter, rootType, parameter));
            _transientScopeRootFunctionQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new TransientScopeRootResolution(
            RootReferenceGenerator.Generate(rootType),
            rootType.FullName(),
            RootReferenceGenerator.Generate("transientScopeRoot"),
            Name,
            containerInstanceScopeReference,
            currentParameters.Select(t => t.Resolution).ToList(),
            disposableCollectionResolution,
            function);
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            while (_transientScopeRootFunctionResolutionsQueue.Any())
            {
                var (scopeRootFunction, parameter, type, functionParameter) = _transientScopeRootFunctionResolutionsQueue.Dequeue();

                var resolvable = functionParameter switch
                {
                    CreateInterfaceParameter createInterfaceParameter => _functionResolutionBuilderFactory(this).ScopeRootFunction(createInterfaceParameter),
                    SwitchImplementationParameter switchImplementationParameter => _functionResolutionBuilderFactory(this).ScopeRootFunction(switchImplementationParameter),
                    SwitchInterfaceAfterScopeRootParameter switchInterfaceAfterScopeRootParameter => _functionResolutionBuilderFactory(this).ScopeRootFunction(switchInterfaceAfterScopeRootParameter),
                    _ => throw new ArgumentOutOfRangeException(nameof(functionParameter))
                };
                
                _rootResolutions.Add(new RootResolutionFunction(
                    scopeRootFunction.Reference,
                    type.FullName(),
                    "internal",
                    resolvable,
                    parameter,
                    "",
                    Name,
                    DisposalHandling));
            }
            
            DoRangedInstancesWork();
        }
    }

    public TransientScopeResolution Build() =>
        new(_rootResolutions,
            DisposalHandling,
            RangedInstances
                .GroupBy(t => t.Item1)
                .Select(g => new RangedInstance(
                    g.Key,
                    g.Select(t => t.Item2).ToList(),
                    DisposalHandling))
                .ToList(),
            _containerReference,
            _containerParameterReference,
            Name);

    public void EnqueueRangedInstanceResolution(RangedInstanceResolutionsQueueItem item) => RangedInstanceResolutionsQueue.Enqueue(item);
}