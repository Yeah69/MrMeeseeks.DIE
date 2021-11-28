namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IScopeResolutionBuilder
{
    bool HasWorkToDo { get; }
    
    ScopeRootResolution AddCreateResolveFunction(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        string singleInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        RangeResolutionBaseBuilder.DecorationScopeRoot? decoration);

    void DoWork();

    ScopeResolution Build();
}

internal class ScopeResolutionBuilder : RangeResolutionBaseBuilder, IScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly Dictionary<string, ScopeRootFunction> _scopeRootFunctionResolutions = new ();
    private readonly HashSet<(ScopeRootFunction, string)> _scopeRootFunctionQueuedOverloads = new ();
    private readonly Queue<(ScopeRootFunction, IReadOnlyList<(ITypeSymbol, ParameterResolution)>, INamedTypeSymbol, DecorationScopeRoot?)> _scopeRootFunctionResolutionsQueue = new();
    
    public ScopeResolutionBuilder(
        // parameter
        IContainerResolutionBuilder containerResolutionBuilder,        
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        ITypeToImplementationsMapper typeToImplementationsMapper, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        ICheckTypeProperties checkTypeProperties) : base(("DefaultScope", true), wellKnownTypes, typeToImplementationsMapper, referenceGeneratorFactory, checkTypeProperties)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
    }

    protected override RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Decoration? decoration) =>
        _containerResolutionBuilder.CreateSingleInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            _containerReference,
            currentParameters,
            decoration);

    protected override ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType, 
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        DecorationScopeRoot? decoration) =>
        AddCreateResolveFunction(
            rootType, 
            referenceGenerator, 
            _containerReference, 
            disposableCollectionResolution,
            currentParameters,
            decoration);

    public bool HasWorkToDo => _scopeRootFunctionResolutionsQueue.Any() || RangedInstanceResolutionsQueue.Any();

    public ScopeRootResolution AddCreateResolveFunction(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        string singleInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        DecorationScopeRoot? decoration)
    {
        var decorationKeySuffix = decoration is { }
            ? $":::{decoration.ImplementationType}"
            : "";
        var key = $"{rootType.FullName()}{decorationKeySuffix}";
        if (!_scopeRootFunctionResolutions.TryGetValue(
                key,
                out ScopeRootFunction function))
        {
            var decorationSuffix = decoration is { }
                ? $"_{decoration.ImplementationType.Name}"
                : "";
            function = new ScopeRootFunction(
                RootReferenceGenerator.Generate("Create", rootType, decorationSuffix),
                rootType.FullName());
            _scopeRootFunctionResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_scopeRootFunctionQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var parameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            _scopeRootFunctionResolutionsQueue.Enqueue((function, parameter, rootType, decoration));
            _scopeRootFunctionQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new ScopeRootResolution(
            referenceGenerator.Generate(rootType),
            rootType.FullName(),
            referenceGenerator.Generate("scopeRoot"),
            Name,
            singleInstanceScopeReference,
            currentParameters.Select(t => t.Resolution).ToList(),
            disposableCollectionResolution,
            function);
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            while (_scopeRootFunctionResolutionsQueue.Any())
            {
                var (scopeRootFunction, parameter, type, decorationScopeRoot) = _scopeRootFunctionResolutionsQueue.Dequeue();
                var referenceGenerator = ReferenceGeneratorFactory.Create();
                var resolvable = CreateConstructorResolution(
                    decorationScopeRoot?.ImplementationType ?? type,
                    referenceGenerator,
                    parameter,
                    Skip.ScopeRootCheck);
                
                if (decorationScopeRoot is {InterfaceType: {} interfaceType, ImplementationType: {} implementationType})
                {
                    var currentInterfaceResolution = new InterfaceResolution(
                        referenceGenerator.Generate(interfaceType),
                        interfaceType.FullName(),
                        resolvable);
                    var decorators = new Stack<INamedTypeSymbol>(CheckTypeProperties.GetDecorators(interfaceType));
                    while (decorators.Any())
                    {
                        var decorator = decorators.Pop();
                        var decoratorResolution = CreateDecoratorConstructorResolution(
                            new Decoration(
                                interfaceType, 
                                implementationType, 
                                decorator, 
                                currentInterfaceResolution),
                            referenceGenerator,
                            parameter,
                            Skip.None);
                        currentInterfaceResolution = new InterfaceResolution(
                            referenceGenerator.Generate(interfaceType),
                            interfaceType.FullName(),
                            decoratorResolution);
                    }

                    resolvable = currentInterfaceResolution;
                }
                
                _rootResolutions.Add(new RootResolutionFunction(
                    scopeRootFunction.Reference,
                    type.FullName(),
                    "internal",
                    resolvable,
                    parameter.Select(t => t.Item2).ToList(),
                    "",
                    Name,
                    DisposalHandling));
            }
            
            DoRangedInstancesWork();
        }
    }

    public ScopeResolution Build() =>
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
}