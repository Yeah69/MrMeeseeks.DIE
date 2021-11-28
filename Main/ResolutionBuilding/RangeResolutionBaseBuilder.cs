namespace MrMeeseeks.DIE.ResolutionBuilding;

internal abstract class RangeResolutionBaseBuilder
{
    public record DecorationScopeRoot(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType);
    public record Decoration(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        INamedTypeSymbol DecoratorType,
        InterfaceResolution CurrentInterfaceResolution);

    [Flags]
    public enum Skip
    {
        None = 1<<0,
        RangedInstanceCheck = 1<<1,
        ScopeRootCheck = 1<<2
    }
    
    protected readonly WellKnownTypes WellKnownTypes;
    protected readonly ITypeToImplementationsMapper TypeToImplementationsMapper;
    protected readonly IReferenceGeneratorFactory ReferenceGeneratorFactory;
    protected readonly ICheckTypeProperties CheckTypeProperties;
    
    protected readonly IReferenceGenerator RootReferenceGenerator;
    protected readonly IDictionary<string, RangedInstanceFunction> RangedInstanceReferenceResolutions =
        new Dictionary<string, RangedInstanceFunction>();
    protected readonly HashSet<(RangedInstanceFunction, string)> RangedInstanceQueuedOverloads = new ();
    protected readonly Queue<(RangedInstanceFunction, IReadOnlyList<(ITypeSymbol, ParameterResolution)>, INamedTypeSymbol, Decoration?)> RangedInstanceResolutionsQueue = new();
    
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
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Decoration? decoration);
    
    protected abstract ScopeRootResolution CreateScopeRootResolution(
        INamedTypeSymbol rootType,
        IReferenceGenerator referenceGenerator,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        DecorationScopeRoot? decoration);

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
            return CreateInterfaceResolution(type, referenceGenerator, currentFuncParameters);

        if (type.TypeKind == TypeKind.Class)
            return CreateConstructorResolution(type, referenceGenerator, currentFuncParameters, Skip.None);

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

    private Resolvable CreateInterfaceResolution(
        ITypeSymbol typeSymbol,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        var interfaceType = (INamedTypeSymbol) typeSymbol;
        var implementations = TypeToImplementationsMapper
            .Map(typeSymbol);
        if (implementations
                .SingleOrDefault() is not { } implementationType)
        {
            return new ErrorTreeItem(implementations.Count switch
            {
                0 => $"[{typeSymbol.FullName()}] Interface: No implementation found",
                > 1 => $"[{typeSymbol.FullName()}] Interface: more than one implementation found",
                _ => $"[{typeSymbol.FullName()}] Interface: Found single implementation {implementations[0].FullName()} is not a named type symbol"
            });
        }

        var shouldBeDecorated = CheckTypeProperties.ShouldBeDecorated(interfaceType);

        if (shouldBeDecorated && CheckTypeProperties.ShouldBeScopeRoot(implementationType))
        {
            var rootResolution = CreateScopeRootResolution(
                interfaceType,
                referenceGenerator,
                DisposableCollectionResolution,
                currentParameters,
                new DecorationScopeRoot(interfaceType, implementationType));
            return new InterfaceResolution(
                referenceGenerator.Generate(typeSymbol),
                typeSymbol.FullName(),
                rootResolution);
        }

        var currentInterfaceResolution = new InterfaceResolution(
            referenceGenerator.Generate(typeSymbol),
            typeSymbol.FullName(),
            Create(implementationType, referenceGenerator, currentParameters));

        if (shouldBeDecorated)
        {
            var decorators = new Stack<INamedTypeSymbol>(CheckTypeProperties.GetDecorators(interfaceType));
            while (decorators.Any())
            {
                var decorator = decorators.Pop();
                var decoratorResolution = CreateDecoratorConstructorResolution(
                    new Decoration(interfaceType, implementationType, decorator, currentInterfaceResolution),
                    referenceGenerator,
                    currentParameters,
                    Skip.None);
                currentInterfaceResolution = new InterfaceResolution(
                    referenceGenerator.Generate(typeSymbol),
                    typeSymbol.FullName(),
                    decoratorResolution);
            }
        }
        
        return currentInterfaceResolution;
    }

    protected Resolvable CreateConstructorResolution(
        ITypeSymbol typeSymbol,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Skip skip)
    {
        var implementations = TypeToImplementationsMapper
            .Map(typeSymbol);
        var implementationType = implementations.SingleOrDefault();
        if (implementationType is not { })
        {
            return new ErrorTreeItem(implementations.Count switch
            {
                0 => $"[{typeSymbol.FullName()}] Class: No implementation found",
                > 1 => $"[{typeSymbol.FullName()}] Class: more than one implementation found",
                _ =>
                    $"[{typeSymbol.FullName()}] Class: Found single implementation{implementations[0].FullName()} is not a named type symbol"
            });
        }

        if (!skip.HasFlag(Skip.RangedInstanceCheck) && CheckTypeProperties.ShouldBeSingleInstance(implementationType))
            return CreateSingleInstanceReferenceResolution(implementationType, referenceGenerator, currentParameters, null);

        if (!skip.HasFlag(Skip.ScopeRootCheck) && CheckTypeProperties.ShouldBeScopeRoot(implementationType))
            return CreateScopeRootResolution(implementationType, referenceGenerator, DisposableCollectionResolution, currentParameters, null);
        
        if (!skip.HasFlag(Skip.RangedInstanceCheck) && CheckTypeProperties.ShouldBeScopedInstance(implementationType))
            return CreateScopedInstanceReferenceResolution(implementationType, referenceGenerator, currentParameters, null);

        if (implementationType.Constructors.SingleOrDefault() is not { } constructor)
        {
            return new ErrorTreeItem(implementationType.Constructors.Length switch
            {
                0 => $"[{typeSymbol.FullName()}] Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                > 1 => $"[{typeSymbol.FullName()}] Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                _ => $"[{typeSymbol.FullName()}] Class.Constructor: {implementationType.Constructors[0].Name} is not a method symbol"
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

    protected Resolvable CreateDecoratorConstructorResolution(
        Decoration decoration,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Skip skip)
    {
        var decoratorType = decoration.DecoratorType;
        if (!skip.HasFlag(Skip.RangedInstanceCheck) && CheckTypeProperties.ShouldBeSingleInstance(decoration.ImplementationType))
            return CreateSingleInstanceReferenceResolution(decoratorType, referenceGenerator, currentParameters, decoration);

        //if (!skipScopeRootCheck && CheckTypeProperties.ShouldBeScopeRoot(implementationType))
        //    return CreateScopeRootResolution(implementationType, referenceGenerator, DisposableCollectionResolution, currentParameters);
        
        if (!skip.HasFlag(Skip.RangedInstanceCheck) && CheckTypeProperties.ShouldBeScopedInstance(decoration.ImplementationType))
            return CreateScopedInstanceReferenceResolution(decoratorType, referenceGenerator, currentParameters, decoration);

        if (decoratorType.Constructors.SingleOrDefault() is not { } constructor)
        {
            return new ErrorTreeItem(decoratorType.Constructors.Length switch
            {
                0 =>
                    $"[{decoratorType.FullName()}] Class.Constructor: No constructor found for implementation {decoratorType.FullName()}",
                > 1 =>
                    $"[{decoratorType.FullName()}] Class.Constructor: More than one constructor found for implementation {decoratorType.FullName()}",
                _ =>
                    $"[{decoratorType.FullName()}] Class.Constructor: {decoratorType.Constructors[0].Name} is not a method symbol"
            });
        }

        return new ConstructorResolution(
            referenceGenerator.Generate(decoratorType),
            decoratorType.FullName(),
            ImplementsIDisposable(
                decoratorType, 
                WellKnownTypes, 
                DisposableCollectionResolution,
                CheckTypeProperties),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(constructor
                .Parameters
                .Select(p =>
                {
                    if (p.Type.Equals(decoration.InterfaceType, SymbolEqualityComparer.Default))
                    {
                        return (p.Name, decoration.CurrentInterfaceResolution);
                    }
                    if (p.Type is not INamedTypeSymbol parameterType)
                    {
                        return ("",
                            new ErrorTreeItem(
                                $"[{decoratorType.FullName()}] Class.Constructor.Parameter: Parameter type {p.Type.FullName()} is not a named type symbol"));
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
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        Decoration? decoration) =>
        CreateRangedInstanceReferenceResolution(
            implementationType,
            referenceGenerator,
            currentParameters,
            "Scoped",
            "this",
            decoration);

    protected RangedInstanceReferenceResolution CreateRangedInstanceReferenceResolution(
        INamedTypeSymbol implementationType,
        IReferenceGenerator referenceGenerator,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        string label,
        string owningObjectReference,
        Decoration? decoration)
    {
        var decorationKeySuffix = decoration is { }
            ? $":::{decoration.ImplementationType}"
            : "";
        var key = $"{implementationType.FullName()}{decorationKeySuffix}";
        if (!RangedInstanceReferenceResolutions.TryGetValue(
                key,
                out RangedInstanceFunction function))
        {
            var decorationSuffix = decoration is { }
                ? $"_{decoration.ImplementationType.Name}"
                : "";
            function = new RangedInstanceFunction(
                RootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix),
                implementationType.FullName(),
                RootReferenceGenerator.Generate($"_{label.ToLower()}InstanceField", implementationType, decorationSuffix),
                RootReferenceGenerator.Generate($"_{label.ToLower()}InstanceLock{decorationSuffix}"));
            RangedInstanceReferenceResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!RangedInstanceQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var parameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            RangedInstanceResolutionsQueue.Enqueue((function, parameter, implementationType, decoration));
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
            var (scopedInstanceFunction, parameter, type, decoration) = RangedInstanceResolutionsQueue.Dequeue();
            var referenceGenerator = ReferenceGeneratorFactory.Create();
            var resolvable = decoration is {}
                ? CreateDecoratorConstructorResolution(decoration, referenceGenerator, parameter, Skip.RangedInstanceCheck)
                : CreateConstructorResolution(
                    type,
                    referenceGenerator,
                    parameter,
                    Skip.RangedInstanceCheck);
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