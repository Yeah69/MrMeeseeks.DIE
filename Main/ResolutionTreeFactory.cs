namespace MrMeeseeks.DIE;

internal interface IContainerResolutionBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes);

    RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        string containerReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    ContainerResolution Build();
}

internal interface IScopeResolutionBuilder
{
    bool HasWorkToDo { get; }
    
    ScopeRootResolution AddCreateResolveFunction(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        string singleInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    void DoWork();

    ScopeResolution Build();
}

internal abstract class RangeResolutionBaseBuilder
{
    protected readonly WellKnownTypes WellKnownTypes;
    protected readonly ITypeToImplementationsMapper TypeToImplementationsMapper;
    protected readonly IReferenceGeneratorFactory ReferenceGeneratorFactory;
    protected readonly ICheckTypeProperties CheckTypeProperties;
    
    protected readonly IReferenceGenerator RootReferenceGenerator;
    protected readonly IDictionary<INamedTypeSymbol, RangedInstanceFunction> RangedInstanceReferenceResolutions =
        new Dictionary<INamedTypeSymbol, RangedInstanceFunction>(SymbolEqualityComparer.Default);
    protected readonly HashSet<(RangedInstanceFunction, string)> RangedInstanceQueuedOverloads = new ();
    protected readonly Queue<(RangedInstanceFunction, IReadOnlyList<(ITypeSymbol, ParameterResolution)>, INamedTypeSymbol)> RangedInstanceResolutionsQueue = new();
    
    protected readonly List<(RangedInstanceFunction, RangedInstanceFunctionOverload)> RangedInstances = new ();
    protected readonly DisposableCollectionResolution DisposableCollectionResolution;
    protected readonly DisposalHandling DisposalHandling;
    protected readonly string Name;

    protected RangeResolutionBaseBuilder(
        // parameters
        (string, bool) name,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties)
    {
        WellKnownTypes = wellKnownTypes;
        TypeToImplementationsMapper = typeToImplementationsMapper;
        ReferenceGeneratorFactory = referenceGeneratorFactory;
        CheckTypeProperties = checkTypeProperties;

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        DisposableCollectionResolution = new DisposableCollectionResolution(
            RootReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfDisposable),
            WellKnownTypes.ConcurrentBagOfDisposable.FullName());
        
        Name = name.Item2 ? RootReferenceGenerator.Generate(name.Item1) : name.Item1;
        DisposalHandling = new DisposalHandling(
            DisposableCollectionResolution,
            Name,
            RootReferenceGenerator.Generate("_disposed"),
            RootReferenceGenerator.Generate("disposed"),
            RootReferenceGenerator.Generate("Disposed"),
            RootReferenceGenerator.Generate("disposable"));
    }

    protected abstract RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    protected abstract ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    protected Resolvable Create(
        ITypeSymbol type, 
        IReferenceGenerator referenceGenerator, 
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentFuncParameters)
    {
        if (currentFuncParameters.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.Type.OriginalDefinition, type.OriginalDefinition)) is { Type: not null, Resolution: not null } funcParameter)
        {
            return funcParameter.Resolution;
        }

        if (type.OriginalDefinition.Equals(WellKnownTypes.Lazy1, SymbolEqualityComparer.Default)
            && type is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeArguments.SingleOrDefault() is not INamedTypeSymbol genericType)
            {
                return new ErrorTreeItem(namedTypeSymbol.TypeArguments.Length switch
                {
                    0 => $"[{namedTypeSymbol.FullName()}] Lazy: No type argument",
                    > 1 => $"[{namedTypeSymbol.FullName()}] Lazy: more than one type argument",
                    _ => $"[{namedTypeSymbol.FullName()}] Lazy: {namedTypeSymbol.TypeArguments[0].FullName()} is not a named type symbol"
                });
            }

            var dependency = Create(
                genericType, 
                ReferenceGeneratorFactory.Create(), 
                Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>());
            return new ConstructorResolution(
                referenceGenerator.Generate(namedTypeSymbol),
                namedTypeSymbol.FullName(),
                ImplementsIDisposable(namedTypeSymbol, WellKnownTypes, DisposableCollectionResolution, CheckTypeProperties),
                new ReadOnlyCollection<(string Name, Resolvable Dependency)>(
                    new List<(string Name, Resolvable Dependency)> 
                    { 
                        (
                            "valueFactory", 
                            new FuncResolution(
                                referenceGenerator.Generate("func"),
                                $"global::System.Func<{genericType.FullName()}>",
                                Array.Empty<ParameterResolution>(),
                                dependency)
                        )
                    }));
        }

        if (type.OriginalDefinition.Equals(WellKnownTypes.Enumerable1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(WellKnownTypes.ReadOnlyCollection1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(WellKnownTypes.ReadOnlyList1, SymbolEqualityComparer.Default))
        {
            if (type is not INamedTypeSymbol collectionType)
            {
                return new ErrorTreeItem($"[{type.FullName()}] Collection: Collection is not a named type symbol");
            }
            if (collectionType.TypeArguments.SingleOrDefault() is not INamedTypeSymbol itemType)
            {
                return new ErrorTreeItem(collectionType.TypeArguments.Length switch
                {
                    0 => $"[{type.FullName()}] Collection: No item type argument",
                    > 1 => $"[{type.FullName()}] Collection: More than one item type argument",
                    _ => $"[{type.FullName()}] Collection: {collectionType.TypeArguments[0].FullName()} is not a named type symbol"
                });
            }
            var itemFullName = itemType.FullName();
            var items = TypeToImplementationsMapper
                .Map(itemType)
                .Select(i => Create(i, referenceGenerator, currentFuncParameters))
                .ToList();
            return new CollectionResolution(
                referenceGenerator.Generate(type),
                type.FullName(),
                itemFullName,
                items);
        }

        if (type.TypeKind == TypeKind.Interface)
        {
            var implementations = TypeToImplementationsMapper
                .Map(type);
            if (implementations
                    .SingleOrDefault() is not { } implementationType)
            {
                return new ErrorTreeItem(implementations.Count switch
                {
                    0 => $"[{type.FullName()}] Interface: No implementation found",
                    > 1 => $"[{type.FullName()}] Interface: more than one implementation found",
                    _ => $"[{type.FullName()}] Interface: Found single implementation {implementations[0].FullName()} is not a named type symbol"
                });
            }
            return new InterfaceResolution(
                referenceGenerator.Generate(type),
                type.FullName(),
                Create(implementationType, referenceGenerator, currentFuncParameters));
        }

        if (type.TypeKind == TypeKind.Class)
            return CreateConstructorResolution(type, referenceGenerator, currentFuncParameters);

        if (type.TypeKind == TypeKind.Delegate 
            && type.FullName().StartsWith("global::System.Func<")
            && type is INamedTypeSymbol namedTypeSymbol0)
        {
            var returnType = namedTypeSymbol0.TypeArguments.Last();
            var innerReferenceGenerator = ReferenceGeneratorFactory.Create();
            var parameterTypes = namedTypeSymbol0
                .TypeArguments
                .Take(namedTypeSymbol0.TypeArguments.Length - 1)
                .Select(ts => (Type: ts, Resolution: new ParameterResolution(innerReferenceGenerator.Generate(ts), ts.FullName())))
                .ToArray();

            var dependency = Create(
                returnType, 
                innerReferenceGenerator, 
                parameterTypes);
            return new FuncResolution(
                referenceGenerator.Generate(type),
                type.FullName(),
                parameterTypes.Select(t => t.Resolution).ToArray(),
                dependency);
        }

        return new ErrorTreeItem($"[{type.FullName()}] Couldn't process in resolution tree creation.");
    }

    protected Resolvable CreateConstructorResolution(
        ITypeSymbol typeSymbol,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        bool skipRangedInstanceCheck = false,
        bool skipScopeRootCheck = false)
    {
        var implementations = TypeToImplementationsMapper
            .Map(typeSymbol);
        if (implementations
                .SingleOrDefault() is not { } implementationType)
        {
            return new ErrorTreeItem(implementations.Count switch
            {
                0 => $"[{typeSymbol.FullName()}] Class: No implementation found",
                > 1 => $"[{typeSymbol.FullName()}] Class: more than one implementation found",
                _ =>
                    $"[{typeSymbol.FullName()}] Class: Found single implementation{implementations[0].FullName()} is not a named type symbol"
            });
        }

        if (!skipRangedInstanceCheck && CheckTypeProperties.ShouldBeSingleInstance(implementationType))
            return CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator, currentParameters);

        if (!skipScopeRootCheck && CheckTypeProperties.ShouldBeScopeRoot(implementationType))
            return CreateScopeRootResolution(implementationType, referenceGenerator, DisposableCollectionResolution, currentParameters);
        
        if (!skipRangedInstanceCheck && CheckTypeProperties.ShouldBeScopedInstance(implementationType))
            return CreateScopedInstanceReferenceResolution(implementationType, referenceGenerator, currentParameters);

        if (implementationType.Constructors.SingleOrDefault() is not { } constructor)
        {
            return new ErrorTreeItem(implementations.Count switch
            {
                0 =>
                    $"[{typeSymbol.FullName()}] Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                > 1 =>
                    $"[{typeSymbol.FullName()}] Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                _ =>
                    $"[{typeSymbol.FullName()}] Class.Constructor: {implementationType.Constructors[0].Name} is not a method symbol"
            });
        }

        return new ConstructorResolution(
            referenceGenerator.Generate(implementationType),
            implementationType.FullName(),
            ImplementsIDisposable(
                implementationType, 
                WellKnownTypes, 
                DisposableCollectionResolution,
                CheckTypeProperties),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(constructor
                .Parameters
                .Select(p =>
                {
                    if (p.Type is not INamedTypeSymbol parameterType)
                    {
                        return ("",
                            new ErrorTreeItem(
                                $"[{typeSymbol.FullName()}] Class.Constructor.Parameter: Parameter type {p.Type.FullName()} is not a named type symbol"));
                    }

                    return (
                        p.Name,
                        Create(parameterType,
                            referenceGenerator,
                            currentParameters));
                })
                .ToList()));
    }

    private RangedInstanceReferenceResolution CreateScopedInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        CreateRangedInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            currentParameters,
            "Scoped",
            "this");

    protected RangedInstanceReferenceResolution CreateRangedInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        string label,
        string owningObjectReference)
    {
        if (!RangedInstanceReferenceResolutions.TryGetValue(
                implementationType,
                out RangedInstanceFunction function))
        {
            function = new RangedInstanceFunction(
                RootReferenceGenerator.Generate($"Get{label}Instance", implementationType),
                implementationType.FullName(),
                RootReferenceGenerator.Generate($"_{label.ToLower()}InstanceField", implementationType),
                RootReferenceGenerator.Generate($"_{label.ToLower()}InstanceLock"));
            RangedInstanceReferenceResolutions[implementationType] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!RangedInstanceQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var parameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            RangedInstanceResolutionsQueue.Enqueue((function, parameter, implementationType));
            RangedInstanceQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new RangedInstanceReferenceResolution(
            referenceGenerator.Generate(implementationType),
            function,
            currentParameters.Select(t => t.Resolution).ToList(),
            owningObjectReference);
    }

    protected void DoRangedInstancesWork()
    {
        while (RangedInstanceResolutionsQueue.Any())
        {
            var (scopedInstanceFunction, parameter, type) = RangedInstanceResolutionsQueue.Dequeue();
            var referenceGenerator = ReferenceGeneratorFactory.Create();
            var resolvable = CreateConstructorResolution(
                type,
                referenceGenerator,
                parameter,
                true);
            RangedInstances.Add((
                scopedInstanceFunction, 
                new RangedInstanceFunctionOverload(
                    resolvable, 
                    parameter.Select(t => t.Item2).ToList())));
        }
    }

    private static DisposableCollectionResolution? ImplementsIDisposable(
        INamedTypeSymbol type, 
        WellKnownTypes wellKnownTypes, 
        DisposableCollectionResolution disposableCollectionResolution,
        ICheckTypeProperties checkDisposalManagement) =>
        type.AllInterfaces.Contains(wellKnownTypes.Disposable) && checkDisposalManagement.ShouldBeManaged(type) 
            ? disposableCollectionResolution 
            : null;
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder
{
    private readonly IContainerInfo _containerInfo;
    
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly IScopeResolutionBuilder _scopeResolutionBuilder;

    public ContainerResolutionBuilder(
        // parameters
        IContainerInfo containerInfo,
        
        // dependencies
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        Func<IContainerResolutionBuilder, IScopeResolutionBuilder> scopeResolutionBuilderFactory) 
        : base((containerInfo.Name, false), wellKnownTypes, typeToImplementationsMapper, referenceGeneratorFactory, checkTypeProperties)
    {
        _containerInfo = containerInfo;
        _scopeResolutionBuilder = scopeResolutionBuilderFactory(this);
    }

    public void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes)
    {
        foreach (var typeSymbol in rootTypes)
            _rootResolutions.Add(
                new RootResolutionFunction(
                    nameof(IContainer<object>.Resolve),
                    typeSymbol.FullName(),
                    "",
                    Create(
                        typeSymbol,
                        ReferenceGeneratorFactory.Create(),
                        Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>()),
                    Array.Empty<ParameterResolution>(),
                    WellKnownTypes.Container.Construct(typeSymbol).FullName(),
                    _containerInfo.Name,
                    DisposalHandling));
    }

    public RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        string containerReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        CreateRangedInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            currentParameters,
            "Single",
            containerReference);

    protected override RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator, "this", currentParameters);

    protected override ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType, 
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeResolutionBuilder.AddCreateResolveFunction(
            rootType, 
            referenceGenerator, 
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

internal class ScopeResolutionBuilder : RangeResolutionBaseBuilder, IScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly IDictionary<INamedTypeSymbol, ScopeRootFunction> _scopeRootFunctionResolutions =
        new Dictionary<INamedTypeSymbol, ScopeRootFunction>(SymbolEqualityComparer.Default);
    private readonly HashSet<(ScopeRootFunction, string)> _scopeRootFunctionQueuedOverloads = new ();
    private readonly Queue<(ScopeRootFunction, IReadOnlyList<(ITypeSymbol, ParameterResolution)>, INamedTypeSymbol)> _scopeRootFunctionResolutionsQueue = new();
    
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
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _containerResolutionBuilder.CreateSingleInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            _containerReference,
            currentParameters);

    protected override ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType, 
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        AddCreateResolveFunction(
            rootType, 
            referenceGenerator, 
            _containerReference, 
            disposableCollectionResolution,
            currentParameters);

    public bool HasWorkToDo => _scopeRootFunctionResolutionsQueue.Any() || RangedInstanceResolutionsQueue.Any();

    public ScopeRootResolution AddCreateResolveFunction(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        string singleInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        if (!_scopeRootFunctionResolutions.TryGetValue(
                rootType,
                out ScopeRootFunction function))
        {
            function = new ScopeRootFunction(
                RootReferenceGenerator.Generate("Create", rootType),
                rootType.FullName());
            _scopeRootFunctionResolutions[rootType] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_scopeRootFunctionQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var parameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            _scopeRootFunctionResolutionsQueue.Enqueue((function, parameter, rootType));
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
                var (scopeRootFunction, parameter, type) = _scopeRootFunctionResolutionsQueue.Dequeue();
                var resolvable = CreateConstructorResolution(
                    type,
                    ReferenceGeneratorFactory.Create(),
                    parameter,
                    skipScopeRootCheck: true);
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