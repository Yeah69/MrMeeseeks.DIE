using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeResolutionBuilder : ITransientScopeImplementationResolutionBuilder
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
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly Dictionary<string, ScopeRootFunction> _transientScopeRootFunctionResolutions = new ();
    private readonly HashSet<(ScopeRootFunction, string)> _transientScopeRootFunctionQueuedOverloads = new ();
    private readonly Queue<(ScopeRootFunction, IReadOnlyList<ParameterResolution>, INamedTypeSymbol, IScopeRootParameter)> _transientScopeRootFunctionResolutionsQueue = new();
    
    public IScopeResolutionBuilder? ScopeResolutionBuilder { get; set; }
    
    internal TransientScopeResolutionBuilder(
        // parameter
        IContainerResolutionBuilder containerResolutionBuilder,
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        ICheckTypeProperties checkTypeProperties,
        IUserProvidedScopeElements userProvidedScopeElements) 
        : base(
            Constants.DefaultTransientScopeName, 
            wellKnownTypes, 
            referenceGeneratorFactory, 
            checkTypeProperties, 
            userProvidedScopeElements)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
    }

    protected override RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    protected override RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, "this");

    protected override TransientScopeRootResolution CreateTransientScopeRootResolution(IScopeRootParameter parameter, INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        AddCreateResolveFunction(
            parameter,
            rootType,
            _containerReference, 
            disposableCollectionResolution,
            currentParameters);

    protected override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        ScopeResolutionBuilder!.AddCreateResolveFunction(
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
                    CreateInterfaceParameter createInterfaceParameter => CreateInterface(createInterfaceParameter),
                    SwitchImplementationParameter switchImplementationParameter => SwitchImplementation(switchImplementationParameter),
                    SwitchInterfaceAfterScopeRootParameter switchInterfaceAfterScopeRootParameter => SwitchInterfaceAfterScopeRoot(switchInterfaceAfterScopeRootParameter),
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