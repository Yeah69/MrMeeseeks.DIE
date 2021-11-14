namespace MrMeeseeks.DIE;

internal interface IContainerResolutionBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes);

    RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol type,
        IReferenceGenerator referenceGenerator,
        string containerReference);

    ContainerResolution Build();
}

internal interface IScopeResolutionBuilder
{
    bool HasWorkToDo { get; }
    
    ScopeRootResolution AddCreateResolveFunction(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        string singleInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution);

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
    protected readonly IDictionary<INamedTypeSymbol, RangedInstanceFunction> ScopedInstanceReferenceResolutions =
            new Dictionary<INamedTypeSymbol, RangedInstanceFunction>(SymbolEqualityComparer.Default);
    protected readonly Queue<RangedInstanceFunction> ScopedInstanceResolutionsQueue = new();
    
    protected readonly List<RangedInstance> ScopedInstances = new ();
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
        IReferenceGenerator referenceGenerator);
    
    protected abstract ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution);

    protected Resolvable Create(
        ITypeSymbol type, 
        IReferenceGenerator referenceGenerator, 
        IReadOnlyList<(ITypeSymbol Type, FuncParameterResolution Resolution)> currentFuncParameters)
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
                Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>());
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
                                Array.Empty<FuncParameterResolution>(),
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
                .Select(ts => (Type: ts, Resolution: new FuncParameterResolution(innerReferenceGenerator.Generate(ts), ts.FullName())))
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
        IReadOnlyList<(ITypeSymbol Type, FuncParameterResolution Resolution)> readOnlyList,
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
            return CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator);

        if (!skipScopeRootCheck && CheckTypeProperties.ShouldBeScopeRoot(implementationType))
            return CreateScopeRootResolution(implementationType, referenceGenerator, DisposableCollectionResolution);
        
        if (!skipRangedInstanceCheck && CheckTypeProperties.ShouldBeScopedInstance(implementationType))
            return CreateScopedInstanceReferenceResolution(implementationType, referenceGenerator);

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
                            readOnlyList));
                })
                .ToList()));
    }

    protected RangedInstanceReferenceResolution CreateScopedInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator)
    {
        if (!ScopedInstanceReferenceResolutions.TryGetValue(
                implementationType,
                out RangedInstanceFunction function))
        {
            function = new RangedInstanceFunction(
                RootReferenceGenerator.Generate("GetScopedInstance", implementationType),
                implementationType.FullName(),
                implementationType,
                RootReferenceGenerator.Generate("_scopedInstanceField", implementationType),
                RootReferenceGenerator.Generate("_scopedInstanceLock"));
            ScopedInstanceReferenceResolutions[implementationType] = function;
            ScopedInstanceResolutionsQueue.Enqueue(function);
        }

        return new RangedInstanceReferenceResolution(
            referenceGenerator.Generate(implementationType),
            function,
            "this");
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

    private readonly IDictionary<INamedTypeSymbol, RangedInstanceFunction> _singleInstanceReferenceResolutions =
        new Dictionary<INamedTypeSymbol, RangedInstanceFunction>(SymbolEqualityComparer.Default);
    private readonly Queue<RangedInstanceFunction> _singleInstanceResolutionsQueue = new();
    
    private readonly List<RootResolutionFunction> _rootResolutions = new ();
    private readonly List<RangedInstance> _singleInstances = new ();
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
                        Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>()),
                    WellKnownTypes.Container.Construct(typeSymbol).FullName(),
                    _containerInfo.Name,
                    DisposalHandling));
    }

    public RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol type,
        IReferenceGenerator referenceGenerator,
        string containerReference)
    {
        if (!_singleInstanceReferenceResolutions.TryGetValue(
                type,
                out RangedInstanceFunction function))
        {
            function = new RangedInstanceFunction(
                RootReferenceGenerator.Generate("GetSingleInstance", type),
                type.FullName(),
                type,
                RootReferenceGenerator.Generate("_singleInstanceField", type),
                RootReferenceGenerator.Generate("_singleInstanceLock"));
            _singleInstanceReferenceResolutions[type] = function;
            _singleInstanceResolutionsQueue.Enqueue(function);
        }

        return new RangedInstanceReferenceResolution(
            referenceGenerator.Generate(type),
            function,
            containerReference);
    }

    protected override RangedInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator) =>
        CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator, "this");

    protected override ScopeRootResolution CreateScopeRootResolution(INamedTypeSymbol rootType, IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution) =>
        _scopeResolutionBuilder.AddCreateResolveFunction(rootType, referenceGenerator, "this", disposableCollectionResolution);

    public ContainerResolution Build()
    {
        while (_singleInstanceResolutionsQueue.Any() || ScopedInstanceResolutionsQueue.Any() || _scopeResolutionBuilder.HasWorkToDo)
        {
            while (_singleInstanceResolutionsQueue.Any())
            {
                var singleInstanceFunction = _singleInstanceResolutionsQueue.Dequeue();
                var resolvable = CreateConstructorResolution(
                    singleInstanceFunction.Type,
                    ReferenceGeneratorFactory.Create(),
                    Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                    true);
                _singleInstances.Add(new RangedInstance(singleInstanceFunction, resolvable, DisposalHandling));
            }
            
            while (ScopedInstanceResolutionsQueue.Any())
            {
                var scopedInstanceFunction = ScopedInstanceResolutionsQueue.Dequeue();
                var resolvable = CreateConstructorResolution(
                    scopedInstanceFunction.Type,
                    ReferenceGeneratorFactory.Create(),
                    Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                    true);
                ScopedInstances.Add(new RangedInstance(scopedInstanceFunction, resolvable, DisposalHandling));
            }
            
            _scopeResolutionBuilder.DoWork();
        }
        
        return new(
            _rootResolutions,
            DisposalHandling,
            ScopedInstances,
            _singleInstances,
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
    private readonly Queue<ScopeRootFunction> _scopeRootFunctionResolutionsQueue = new();
    
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
        IReferenceGenerator referenceGenerator) =>
        _containerResolutionBuilder.CreateSingleInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            _containerReference);

    protected override ScopeRootResolution CreateScopeRootResolution(INamedTypeSymbol rootType, IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution) =>
        AddCreateResolveFunction(rootType, referenceGenerator, _containerReference, disposableCollectionResolution);

    public bool HasWorkToDo => _scopeRootFunctionResolutionsQueue.Any() || ScopedInstanceResolutionsQueue.Any();

    public ScopeRootResolution AddCreateResolveFunction(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        string singleInstanceScopeReference,
        DisposableCollectionResolution disposableCollectionResolution)
    {
        if (!_scopeRootFunctionResolutions.TryGetValue(
                rootType,
                out ScopeRootFunction function))
        {
            function = new ScopeRootFunction(
                RootReferenceGenerator.Generate("Create", rootType),
                rootType.FullName(),
                rootType);
            _scopeRootFunctionResolutions[rootType] = function;
            _scopeRootFunctionResolutionsQueue.Enqueue(function);
        }

        return new ScopeRootResolution(
            referenceGenerator.Generate(rootType),
            rootType.FullName(),
            referenceGenerator.Generate("scopeRoot"),
            Name,
            singleInstanceScopeReference,
            disposableCollectionResolution,
            function);
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            while (_scopeRootFunctionResolutionsQueue.Any())
            {
                var scopeRootFunction = _scopeRootFunctionResolutionsQueue.Dequeue();
                var resolvable = CreateConstructorResolution(
                    scopeRootFunction.Type,
                    ReferenceGeneratorFactory.Create(),
                    Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                    skipScopeRootCheck: true);
                _rootResolutions.Add(new RootResolutionFunction(
                    scopeRootFunction.Reference,
                    scopeRootFunction.Type.FullName(),
                    "internal",
                    resolvable,
                    "",
                    Name,
                    DisposalHandling));
            }
            
            while (ScopedInstanceResolutionsQueue.Any())
            {
                var scopedInstanceFunction = ScopedInstanceResolutionsQueue.Dequeue();
                var resolvable = CreateConstructorResolution(
                    scopedInstanceFunction.Type,
                    ReferenceGeneratorFactory.Create(),
                    Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                    true);
                ScopedInstances.Add(new RangedInstance(scopedInstanceFunction, resolvable, DisposalHandling));
            }
        }
    }

    public ScopeResolution Build() =>
        new(_rootResolutions,
            DisposalHandling,
            ScopedInstances,
            _containerReference,
            _containerParameterReference,
            Name);
}