namespace MrMeeseeks.DIE;

internal interface IResolutionTreeFactory
{
    ContainerResolution Create(ITypeSymbol root);
}

internal class ResolutionTreeFactory : IResolutionTreeFactory
{
    private readonly ITypeToImplementationsMapper _typeToImplementationsMapper;
    private readonly IReferenceGeneratorFactory _referenceGeneratorFactory;
    private readonly ICheckTypeProperties _checkTypeProperties;
    private readonly WellKnownTypes _wellKnownTypes;

    private readonly IDictionary<INamedTypeSymbol, SingleInstanceFunction> _singleInstanceReferenceResolutions =
        new Dictionary<INamedTypeSymbol, SingleInstanceFunction>(SymbolEqualityComparer.Default);
    private readonly Queue<SingleInstanceFunction> _singleInstanceResolutionsQueue = new();

    private readonly IReferenceGenerator _singleInstanceReferenceGenerator;

    public ResolutionTreeFactory(
        ITypeToImplementationsMapper typeToImplementationsMapper,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes)
    {
        _typeToImplementationsMapper = typeToImplementationsMapper;
        _referenceGeneratorFactory = referenceGeneratorFactory;
        _checkTypeProperties = checkTypeProperties;
        _wellKnownTypes = wellKnownTypes;
        _singleInstanceReferenceGenerator = referenceGeneratorFactory.Create();
    }

    public ContainerResolution Create(ITypeSymbol type)
    {
        var disposableCollectionResolution = new DisposableCollectionResolution(
            _singleInstanceReferenceGenerator.Generate(_wellKnownTypes.ConcurrentBagOfDisposable),
            _wellKnownTypes.ConcurrentBagOfDisposable.FullName());
        var referenceGenerator = _referenceGeneratorFactory.Create();
        var rootResolution = Create(
            type,
            referenceGenerator,
            Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
            disposableCollectionResolution);
        var singleInstances = new List<SingleInstance>();

        while (_singleInstanceResolutionsQueue.Any())
        {
            var singleInstanceFunction = _singleInstanceResolutionsQueue.Dequeue();
            var resolvable = CreateConstructorResolution(
                singleInstanceFunction.Type,
                _referenceGeneratorFactory.Create(),
                Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                disposableCollectionResolution,
                true);
            singleInstances.Add(new SingleInstance(singleInstanceFunction, resolvable));
        }

        return new ContainerResolution(
            rootResolution,
            new ContainerResolutionDisposalHandling(
                disposableCollectionResolution,
                _singleInstanceReferenceGenerator.Generate("_disposed"),
                _singleInstanceReferenceGenerator.Generate("disposed"),
                _singleInstanceReferenceGenerator.Generate("Disposed"),
                _singleInstanceReferenceGenerator.Generate("disposable")),
            singleInstances);
    }

    private Resolvable Create(
        ITypeSymbol type, 
        IReferenceGenerator referenceGenerator, 
        IReadOnlyList<(ITypeSymbol Type, FuncParameterResolution Resolution)> currentFuncParameters,
        DisposableCollectionResolution disposableCollectionResolution)
    {
        if (currentFuncParameters.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.Type.OriginalDefinition, type.OriginalDefinition)) is { Type: not null, Resolution: not null } funcParameter)
        {
            return funcParameter.Resolution;
        }

        if (type.OriginalDefinition.Equals(_wellKnownTypes.Lazy1, SymbolEqualityComparer.Default)
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
                _referenceGeneratorFactory.Create(), 
                Array.Empty<(ITypeSymbol Type, FuncParameterResolution Resolution)>(),
                disposableCollectionResolution);
            return new ConstructorResolution(
                referenceGenerator.Generate(namedTypeSymbol),
                namedTypeSymbol.FullName(),
                ImplementsIDisposable(namedTypeSymbol, _wellKnownTypes, disposableCollectionResolution, _checkTypeProperties),
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

        if (type.OriginalDefinition.Equals(_wellKnownTypes.Enumerable1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(_wellKnownTypes.ReadOnlyCollection1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(_wellKnownTypes.ReadOnlyList1, SymbolEqualityComparer.Default))
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
            var items = _typeToImplementationsMapper
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
            var implementations = _typeToImplementationsMapper
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
            var innerReferenceGenerator = _referenceGeneratorFactory.Create();
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
        
    private Resolvable CreateConstructorResolution(
        ITypeSymbol typeSymbol, 
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, FuncParameterResolution Resolution)> readOnlyList, 
        DisposableCollectionResolution disposableCollectionResolution,
        bool skipSingleInstanceCheck = false)
    {
        var implementations = _typeToImplementationsMapper
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

        if (!skipSingleInstanceCheck && _checkTypeProperties.ShouldBeSingleInstance(implementationType))
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
            ImplementsIDisposable(implementationType, _wellKnownTypes, disposableCollectionResolution,
                _checkTypeProperties),
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

    private SingleInstanceReferenceResolution CreateSingleInstanceReferenceResolution(
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

    private static DisposableCollectionResolution? ImplementsIDisposable(
        INamedTypeSymbol type, 
        WellKnownTypes wellKnownTypes, 
        DisposableCollectionResolution disposableCollectionResolution,
        ICheckTypeProperties checkDisposalManagement) =>
        type.AllInterfaces.Contains(wellKnownTypes.Disposable) && checkDisposalManagement.ShouldBeManaged(type) 
            ? disposableCollectionResolution 
            : null;
}