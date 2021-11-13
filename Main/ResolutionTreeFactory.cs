namespace MrMeeseeks.DIE;

internal interface IContainerResolutionBuilder
{
    void AddCreateFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes);

    ContainerResolution Build();
}

internal interface IScopeResolutionBuilder
{
    void AddCreateFunction(INamedTypeSymbol rootType);

    ScopeResolution Build();
}

internal abstract class RangeResolutionBaseBuilder
{
    protected readonly WellKnownTypes WellKnownTypes;
    protected readonly ITypeToImplementationsMapper TypeToImplementationsMapper;
    protected readonly IReferenceGeneratorFactory ReferenceGeneratorFactory;
    protected readonly ICheckTypeProperties CheckTypeProperties;

    private readonly IDictionary<INamedTypeSymbol, ScopedInstanceFunction>
        _containerScopedInstanceReferenceResolutions =
            new Dictionary<INamedTypeSymbol, ScopedInstanceFunction>(SymbolEqualityComparer.Default);
    private readonly Queue<ScopedInstanceFunction> _scopedInstanceResolutionsQueue = new();
    
    protected readonly List<ScopedInstance> ScopedInstances = new ();


    protected RangeResolutionBaseBuilder(
        WellKnownTypes wellKnownTypes,
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties)
    {
        WellKnownTypes = wellKnownTypes;
        TypeToImplementationsMapper = typeToImplementationsMapper;
        ReferenceGeneratorFactory = referenceGeneratorFactory;
        CheckTypeProperties = checkTypeProperties;
    }

    protected abstract SingleInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator);

    protected Resolvable Create(
        ITypeSymbol type, 
        IReferenceGenerator referenceGenerator, 
        IReadOnlyList<(ITypeSymbol Type, FuncParameterResolution Resolution)> currentFuncParameters,
        DisposableCollectionResolution disposableCollectionResolution)
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
                Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                disposableCollectionResolution);
            return new ConstructorResolution(
                referenceGenerator.Generate(namedTypeSymbol),
                namedTypeSymbol.FullName(),
                ImplementsIDisposable(namedTypeSymbol, WellKnownTypes, disposableCollectionResolution, CheckTypeProperties),
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
                .Select(i => Create(i, referenceGenerator, currentFuncParameters, disposableCollectionResolution))
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
                Create(implementationType, referenceGenerator, currentFuncParameters, disposableCollectionResolution));
        }

        if (type.TypeKind == TypeKind.Class)
            return CreateConstructorResolution(type, referenceGenerator, currentFuncParameters, disposableCollectionResolution);

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
                parameterTypes,
                disposableCollectionResolution);
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
        DisposableCollectionResolution disposableCollectionResolution,
        bool skipSingleInstanceCheck = false)
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

        if (!skipSingleInstanceCheck && CheckTypeProperties.ShouldBeSingleInstance(implementationType))
            return CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator);

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
            ImplementsIDisposable(implementationType, WellKnownTypes, disposableCollectionResolution,
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
                            readOnlyList,
                            disposableCollectionResolution));
                })
                .ToList()));
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
    private readonly IDictionary<INamedTypeSymbol, SingleInstanceFunction> _singleInstanceReferenceResolutions =
        new Dictionary<INamedTypeSymbol, SingleInstanceFunction>(SymbolEqualityComparer.Default);
    private readonly Queue<SingleInstanceFunction> _singleInstanceResolutionsQueue = new();

    private readonly IReferenceGenerator _singleInstanceReferenceGenerator;
    private readonly List<(Resolvable, INamedTypeSymbol)> _rootResolutions = new ();
    private readonly List<SingleInstance> _singleInstances = new ();
    private readonly DisposableCollectionResolution _disposableCollectionResolution;
    private readonly IScopeResolutionBuilder _scopeResolutionBuilder;

    public ContainerResolutionBuilder(
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        Func<IScopeResolutionBuilder> scopeResolutionBuilderFactory) : base(wellKnownTypes, typeToImplementationsMapper, referenceGeneratorFactory,
        checkTypeProperties)
    {
        _singleInstanceReferenceGenerator = referenceGeneratorFactory.Create();
        _disposableCollectionResolution = new DisposableCollectionResolution(
            _singleInstanceReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfDisposable),
            WellKnownTypes.ConcurrentBagOfDisposable.FullName());
        _scopeResolutionBuilder = scopeResolutionBuilderFactory();
    }

    public void AddCreateFunctions(IReadOnlyList<INamedTypeSymbol> rootTypes)
    {
        foreach (var typeSymbol in rootTypes)
            _rootResolutions.Add((Create(
                typeSymbol,
                ReferenceGeneratorFactory.Create(),
                Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                _disposableCollectionResolution),
                    typeSymbol));

        while (_singleInstanceResolutionsQueue.Any())
        {
            var singleInstanceFunction = _singleInstanceResolutionsQueue.Dequeue();
            var resolvable = CreateConstructorResolution(
                singleInstanceFunction.Type,
                ReferenceGeneratorFactory.Create(),
                Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                _disposableCollectionResolution,
                true);
            _singleInstances.Add(new SingleInstance(singleInstanceFunction, resolvable));
        }
    }

    protected override SingleInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator)
    {
        if (!_singleInstanceReferenceResolutions.TryGetValue(
                implementationType,
                out SingleInstanceFunction function))
        {
            function = new SingleInstanceFunction(
                _singleInstanceReferenceGenerator.Generate("GetSingleInstance", implementationType),
                implementationType.FullName(),
                implementationType,
                _singleInstanceReferenceGenerator.Generate("_singleInstanceField", implementationType),
                _singleInstanceReferenceGenerator.Generate("_singleInstanceLock"));
            _singleInstanceReferenceResolutions[implementationType] = function;
            _singleInstanceResolutionsQueue.Enqueue(function);
        }

        return new SingleInstanceReferenceResolution(
            referenceGenerator.Generate(implementationType),
            function);
    }

    public ContainerResolution Build() =>
        new(_rootResolutions,
            new DisposalHandling(
                _disposableCollectionResolution,
                _singleInstanceReferenceGenerator.Generate("_disposed"),
                _singleInstanceReferenceGenerator.Generate("disposed"),
                _singleInstanceReferenceGenerator.Generate("Disposed"),
                _singleInstanceReferenceGenerator.Generate("disposable")),
            ScopedInstances,
            _singleInstances,
            _scopeResolutionBuilder.Build());
}

internal class ScopeResolutionBuilder : RangeResolutionBaseBuilder, IScopeResolutionBuilder
{
    private readonly IReferenceGenerator _scopeInstanceReferenceGenerator;
    private readonly List<(Resolvable, INamedTypeSymbol)> _rootResolutions = new ();
    private readonly DisposableCollectionResolution _disposableCollectionResolution;
    
    public ScopeResolutionBuilder(
        WellKnownTypes wellKnownTypes, 
        ITypeToImplementationsMapper typeToImplementationsMapper, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        ICheckTypeProperties checkTypeProperties) : base(wellKnownTypes, typeToImplementationsMapper, referenceGeneratorFactory, checkTypeProperties)
    {
        _scopeInstanceReferenceGenerator = referenceGeneratorFactory.Create();
        _disposableCollectionResolution = new DisposableCollectionResolution(
            _scopeInstanceReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfDisposable),
            WellKnownTypes.ConcurrentBagOfDisposable.FullName());
    }

    protected override SingleInstanceReferenceResolution CreateSingleInstanceReferenceResolution(INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator)
    {
        throw new NotImplementedException();
    }

    public void AddCreateFunction(INamedTypeSymbol rootType)
    {
        throw new NotImplementedException();
    }

    public ScopeResolution Build() =>
        new(_rootResolutions,
            new DisposalHandling(
                _disposableCollectionResolution,
                _scopeInstanceReferenceGenerator.Generate("_disposed"),
                _scopeInstanceReferenceGenerator.Generate("disposed"),
                _scopeInstanceReferenceGenerator.Generate("Disposed"),
                _scopeInstanceReferenceGenerator.Generate("disposable")),
            ScopedInstances);
}