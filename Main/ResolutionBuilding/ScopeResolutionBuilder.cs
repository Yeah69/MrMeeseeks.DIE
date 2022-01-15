namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IScopeResolutionBuilder
{
    bool HasWorkToDo { get; }
    
    ScopeRootResolution AddCreateResolveFunction(
        RangeResolutionBaseBuilder.IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

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
    private readonly Queue<(ScopeRootFunction, IReadOnlyList<ParameterResolution>, INamedTypeSymbol, IScopeRootParameter)> _scopeRootFunctionResolutionsQueue = new();
    
    internal ScopeResolutionBuilder(
        // parameter
        IContainerResolutionBuilder containerResolutionBuilder,        
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        ITypeToImplementationsMapper typeToImplementationsMapper, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        ICheckTypeProperties checkTypeProperties,
        ICheckDecorators checkDecorators, 
        IUserProvidedScopeElements userProvidedScopeElements) 
        : base(
            ("DefaultScope", true), 
            wellKnownTypes, 
            typeToImplementationsMapper, 
            referenceGeneratorFactory, 
            checkTypeProperties, 
            checkDecorators,
            userProvidedScopeElements)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
    }

    protected override RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(
            parameter,
            _containerReference);

    protected override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        AddCreateResolveFunction(
            parameter,
            rootType, 
            _containerReference, 
            disposableCollectionResolution,
            currentParameters);

    public bool HasWorkToDo => _scopeRootFunctionResolutionsQueue.Any() || RangedInstanceResolutionsQueue.Any();

    public ScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{parameter.KeySuffix()}";
        if (!_scopeRootFunctionResolutions.TryGetValue(
                key,
                out ScopeRootFunction function))
        {
            function = new ScopeRootFunction(
                RootReferenceGenerator.Generate("Create", rootType, parameter.RootFunctionSuffix()),
                rootType.FullName());
            _scopeRootFunctionResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_scopeRootFunctionQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var currParameter = currentParameters
                .Select(t => new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName()))
                .ToList();
            _scopeRootFunctionResolutionsQueue.Enqueue((function, currParameter, rootType, parameter));
            _scopeRootFunctionQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new ScopeRootResolution(
            RootReferenceGenerator.Generate(rootType),
            rootType.FullName(),
            RootReferenceGenerator.Generate("scopeRoot"),
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
            while (_scopeRootFunctionResolutionsQueue.Any())
            {
                var (scopeRootFunction, parameter, type, functionParameter) = _scopeRootFunctionResolutionsQueue.Dequeue();

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