using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal enum SynchronicityDecision
{
    Undecided,
    Sync,
    AsyncTask,
    AsyncValueTask
}

internal interface IFunctionResolutionBuilder : IResolutionBuilder<FunctionResolution>
{
    INamedTypeSymbol OriginalReturnType { get; }
    INamedTypeSymbol? ActualReturnType { get; }
    
    MultiSynchronicityFunctionCallResolution BuildFunctionCall(
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        string? ownerReference);

    MethodGroupResolution BuildMethodGroup();
}

internal abstract class FunctionResolutionBuilder : IFunctionResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> _localFunctionResolutionBuilderFactory;
    private readonly ICheckTypeProperties _checkTypeProperties;

    protected readonly IReferenceGenerator RootReferenceGenerator;

    private readonly DisposableCollectionResolution _disposableCollectionResolution;
    private readonly IUserProvidedScopeElements _userProvidedScopeElements;

    protected readonly IList<IFunctionResolutionBuilder> LocalFunctions = new List<IFunctionResolutionBuilder>();

    protected readonly ISet<IAwaitableResolution> PotentialAwaits = new HashSet<IAwaitableResolution>();
    
    protected abstract string Name { get; }
    protected string TypeFullName => ActualReturnType?.FullName() ?? OriginalReturnType.FullName();

    public Lazy<SynchronicityDecision> SynchronicityDecision { get; }
    protected Lazy<Resolvable> Resolvable { get; } 
    
    protected IReadOnlyList<ParameterResolution> Parameters { get; }
    
    public INamedTypeSymbol OriginalReturnType { get; }
    public INamedTypeSymbol? ActualReturnType { get; protected set; }

    internal FunctionResolutionBuilder(
        // parameters
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        INamedTypeSymbol returnType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters,
        
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
    {
        OriginalReturnType = returnType;
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
        _wellKnownTypes = wellKnownTypes;
        _localFunctionResolutionBuilderFactory = localFunctionResolutionBuilderFactory;
        _checkTypeProperties = rangeResolutionBaseBuilder.CheckTypeProperties;
        _userProvidedScopeElements = rangeResolutionBaseBuilder.UserProvidedScopeElements;

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        _disposableCollectionResolution = _rangeResolutionBaseBuilder.DisposableCollectionResolution;
        Parameters = currentParameters
            .Select(p => new ParameterResolution(RootReferenceGenerator.Generate(p.Type), TypeFullName))
            .ToList();

        SynchronicityDecision = new(() =>
            PotentialAwaits.Any(pa => pa.Await)
                ? Function.SynchronicityDecision.AsyncValueTask
                : Function.SynchronicityDecision.Sync);
        Resolvable = new(CreateResolvable);
    }

    protected (Resolvable, ITaskConsumableResolution?) SwitchType(SwitchTypeParameter parameter)
    {
        var (type, currentFuncParameters) = parameter;
        if (currentFuncParameters.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.Type.OriginalDefinition, type.OriginalDefinition)) is { Type: not null, Resolution: not null } funcParameter)
            return (funcParameter.Resolution, null);

        if (_userProvidedScopeElements.GetInstanceFor(type) is { } instance)
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(instance.Type),
                    instance.Type.FullName(),
                    instance.Name),
                null);

        if (_userProvidedScopeElements.GetPropertyFor(type) is { } property)
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(property.Type),
                    property.Type.FullName(),
                    property.Name),
                null);

        if (_userProvidedScopeElements.GetFactoryFor(type) is { } factory)
            return (
                new FactoryResolution(
                    RootReferenceGenerator.Generate(factory.ReturnType),
                    factory.ReturnType.FullName(),
                    factory.Name,
                    factory
                        .Parameters
                        .Select(p => (p.Name, SwitchType(new SwitchTypeParameter(p.Type, currentFuncParameters)).Item1))
                        .ToList()),
                null);

        if (type.OriginalDefinition.Equals(_wellKnownTypes.Task1, SymbolEqualityComparer.Default)
            && type is INamedTypeSymbol task)
            return SwitchTask(new SwitchTaskParameter(SwitchType(new SwitchTypeParameter(task.TypeArguments[0], currentFuncParameters))));

        if (type.OriginalDefinition.Equals(_wellKnownTypes.ValueTask1, SymbolEqualityComparer.Default)
            && type is INamedTypeSymbol valueTask)
            return SwitchValueTask(new SwitchValueTaskParameter(SwitchType(new SwitchTypeParameter(valueTask.TypeArguments[0], currentFuncParameters))));

        if (type.FullName().StartsWith("global::System.ValueTuple<") && type is INamedTypeSymbol valueTupleType)
        {
            var constructorResolution = new ConstructorResolution(
                RootReferenceGenerator.Generate(valueTupleType),
                valueTupleType.FullName(),
                ImplementsIDisposable(valueTupleType, _wellKnownTypes, _disposableCollectionResolution, _checkTypeProperties),
                valueTupleType
                    .TypeArguments
                    .Select((t, i) => ($"item{(i + 1)}", SwitchType(new SwitchTypeParameter(t, currentFuncParameters)).Item1))
                    .ToList(),
                Array.Empty<(string Name, Resolvable Dependency)>(),
                null);
            return (constructorResolution, constructorResolution);
        }

        if (type.FullName().StartsWith("(") && type.FullName().EndsWith(")") && type is INamedTypeSymbol syntaxValueTupleType)
        {
            var itemTypes = GetTypeArguments(syntaxValueTupleType).ToList();
            
            return (new SyntaxValueTupleResolution(
                RootReferenceGenerator.Generate("syntaxValueTuple"),
                syntaxValueTupleType.FullName(),
                itemTypes
                    .Select(t => SwitchType(new SwitchTypeParameter(t, currentFuncParameters)).Item1)
                    .ToList()),
                    null);

            IEnumerable<ITypeSymbol> GetTypeArguments(INamedTypeSymbol currentSyntaxValueTupleType)
            {
                foreach (var typeArgument in currentSyntaxValueTupleType.TypeArguments)
                {
                    if (typeArgument.FullName().StartsWith("(") && typeArgument.FullName().EndsWith(")") &&
                        typeArgument is INamedTypeSymbol nextSyntaxValueTupleType)
                    {
                        foreach (var typeSymbol in GetTypeArguments(nextSyntaxValueTupleType))
                        {
                            yield return typeSymbol;
                        }
                    }
                    else
                    {
                        yield return typeArgument;
                    }
                }
            }
        }

        if (type.OriginalDefinition.Equals(_wellKnownTypes.Lazy1, SymbolEqualityComparer.Default)
            && type is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeArguments.SingleOrDefault() is not INamedTypeSymbol genericType)
            {
                return (
                    new ErrorTreeItem(namedTypeSymbol.TypeArguments.Length switch
                    {
                        0 => $"[{namedTypeSymbol.FullName()}] Lazy: No type argument",
                        > 1 => $"[{namedTypeSymbol.FullName()}] Lazy: more than one type argument",
                        _ => $"[{namedTypeSymbol.FullName()}] Lazy: {namedTypeSymbol.TypeArguments[0].FullName()} is not a named type symbol"
                    }),
                    null);
            }

            var newLocalFunction = _localFunctionResolutionBuilderFactory(_rangeResolutionBaseBuilder, genericType, Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>());
            LocalFunctions.Add(newLocalFunction);

            var constructorInjection = new LazyResolution(
                RootReferenceGenerator.Generate(namedTypeSymbol),
                namedTypeSymbol.FullName(),
                newLocalFunction.BuildMethodGroup());
            return (constructorInjection, null);
        }

        if (type.TypeKind == TypeKind.Delegate 
            && type.FullName().StartsWith("global::System.Func<")
            && type is INamedTypeSymbol namedTypeSymbol0)
        {
            var returnTypeRaw = namedTypeSymbol0.TypeArguments.Last();
            
            if (returnTypeRaw is not INamedTypeSymbol returnType)
            {
                return (
                    new ErrorTreeItem($"[{type.FullName()}] Func: Return type not named"),
                    null);
            }
            
            var parameterTypes = namedTypeSymbol0
                .TypeArguments
                .Take(namedTypeSymbol0.TypeArguments.Length - 1)
                .Select(ts => (Type: ts, Resolution: new ParameterResolution(RootReferenceGenerator.Generate(ts), ts.FullName())))
                .ToArray();

            var newLocalFunction = _localFunctionResolutionBuilderFactory(_rangeResolutionBaseBuilder, returnType, parameterTypes);
            LocalFunctions.Add(newLocalFunction);
            
            return (
                new FuncResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    newLocalFunction.BuildMethodGroup()),
                null);
        }

        if (type.OriginalDefinition.Equals(_wellKnownTypes.Enumerable1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(_wellKnownTypes.ReadOnlyCollection1, SymbolEqualityComparer.Default)
            || type.OriginalDefinition.Equals(_wellKnownTypes.ReadOnlyList1, SymbolEqualityComparer.Default))
        {
            if (type is not INamedTypeSymbol collectionType)
            {
                return (
                    new ErrorTreeItem($"[{type.FullName()}] Collection: Collection is not a named type symbol"),
                    null);
            }
            if (collectionType.TypeArguments.SingleOrDefault() is not INamedTypeSymbol wrappedType)
            {
                return (
                    new ErrorTreeItem(collectionType.TypeArguments.Length switch
                    {
                        0 => $"[{type.FullName()}] Collection: No item type argument",
                        > 1 => $"[{type.FullName()}] Collection: More than one item type argument",
                        _ => $"[{type.FullName()}] Collection: {collectionType.TypeArguments[0].FullName()} is not a named type symbol"
                    }),
                    null);
            }
            ITypeSymbol wrappedItemTypeSymbol = wrappedType;
            ITypeSymbol unwrappedItemTypeSymbol = wrappedType;
            TaskType? taskType = null;

            if (wrappedType.OriginalDefinition.Equals(_wellKnownTypes.Task1, SymbolEqualityComparer.Default)
                && type is INamedTypeSymbol enumerableTask)
            {
                wrappedItemTypeSymbol = enumerableTask.TypeArguments[0];
                taskType = TaskType.Task;
            }

            if (wrappedType.OriginalDefinition.Equals(_wellKnownTypes.ValueTask1, SymbolEqualityComparer.Default)
                && type is INamedTypeSymbol enumerableValueTask)
            {
                wrappedItemTypeSymbol = enumerableValueTask.TypeArguments[0];
                taskType = TaskType.ValueTask;
            }
            
            if (wrappedItemTypeSymbol is not INamedTypeSymbol wrappedItemType)
            {
                return (
                    new ErrorTreeItem($"[{type.FullName()}] Collection: Collection's inner type is not a named type symbol"),
                    null);
            }

            if (taskType is { })
                unwrappedItemTypeSymbol = wrappedItemType.TypeArguments[0];
            
            if (unwrappedItemTypeSymbol is not INamedTypeSymbol unwrappedItemType)
            {
                return (
                    new ErrorTreeItem($"[{type.FullName()}] Collection: Collection's inner type is not a named type symbol"),
                    null);
            }
                
            var itemTypeIsInterface = unwrappedItemType.TypeKind == TypeKind.Interface;
            var items = _checkTypeProperties
                .MapToImplementations(unwrappedItemType)
                .Select(i =>
                {
                    var itemResolution = itemTypeIsInterface
                        ? SwitchInterfaceForSpecificImplementation(
                            new SwitchInterfaceForSpecificImplementationParameter(unwrappedItemType, i, currentFuncParameters))
                        : SwitchClass(new SwitchClassParameter(i, currentFuncParameters));
                    return (taskType switch
                    {
                        TaskType.Task => SwitchTask(new SwitchTaskParameter(itemResolution)),
                        TaskType.ValueTask => SwitchValueTask(new SwitchValueTaskParameter(itemResolution)),
                        _ => itemResolution
                    }).Item1;
                })
                .ToList();

            return (
                new CollectionResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    wrappedItemType.FullName(),
                    items),
                null);
        }

        if (type.TypeKind == TypeKind.Interface)
            return SwitchInterface(new SwitchInterfaceParameter(type, currentFuncParameters));

        if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
            return SwitchClass(new SwitchClassParameter(type, currentFuncParameters));

        return (
            new ErrorTreeItem($"[{type.FullName()}] Couldn't process in resolution tree creation."),
            null);
    }

    private (Resolvable, ITaskConsumableResolution?) SwitchTask(SwitchTaskParameter parameter)
    {
        var resolution = parameter.InnerResolution;
        var boundTaskTypeFullName = _wellKnownTypes
            .Task1
            .ConstructUnboundGenericType()
            .FullName()
            .Replace("<>", $"<{resolution.Item1.TypeFullName}>");
        var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
        if (resolution.Item2 is ConstructorResolution constructorResolution)
        {
            if (constructorResolution.Initialization is TaskBaseTypeInitializationResolution taskBaseResolution)
                taskBaseResolution.Await = false;
            var taskResolution = constructorResolution.Initialization switch
            {
                TaskTypeInitializationResolution taskTypeInitialization => new TaskFromTaskResolution(
                    resolution.Item1,
                    taskTypeInitialization,
                    wrappedTaskReference,
                    boundTaskTypeFullName),
                ValueTaskTypeInitializationResolution taskTypeInitialization => new TaskFromValueTaskResolution(
                    resolution.Item1,
                    taskTypeInitialization,
                    wrappedTaskReference,
                    boundTaskTypeFullName),
                _ => (Resolvable) new TaskFromSyncResolution(resolution.Item1, wrappedTaskReference, boundTaskTypeFullName)
            };
            return (taskResolution, resolution.Item2);
        }
        else if (resolution.Item2 is MultiSynchronicityFunctionCallResolution multiSynchronicityFunctionCallResolution)
        {
            var taskReference = RootReferenceGenerator.Generate("task");
            var taskFullName = multiSynchronicityFunctionCallResolution.AsyncTask.TypeFullName;
            multiSynchronicityFunctionCallResolution.Sync.Await = false;
            multiSynchronicityFunctionCallResolution.AsyncTask.Await = false;
            multiSynchronicityFunctionCallResolution.AsyncValueTask.Await = false;
            return (new MultiTaskResolution(
                new TaskFromSyncResolution(
                    resolution.Item1,
                    taskReference,
                    taskFullName),
                new NewReferenceResolvable(taskReference, taskFullName, resolution.Item1),
                new TaskFromWrappedValueTaskResolution(
                    resolution.Item1,
                    taskReference,
                    taskFullName),
                multiSynchronicityFunctionCallResolution.LazySynchronicityDecision), null);
        }
        return (new TaskFromSyncResolution(resolution.Item1, wrappedTaskReference, boundTaskTypeFullName), resolution.Item2);
    }

    private (Resolvable, ITaskConsumableResolution?) SwitchValueTask(SwitchValueTaskParameter parameter)
    {
        var resolution = parameter.InnerResolution;
        var boundValueTaskTypeFullName = _wellKnownTypes
            .ValueTask1
            .ConstructUnboundGenericType()
            .FullName()
            .Replace("<>", $"<{resolution.Item1.TypeFullName}>");
        var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.ValueTask);
        if (resolution.Item2 is ConstructorResolution constructorResolution)
        {
            if (constructorResolution.Initialization is TaskBaseTypeInitializationResolution taskBaseResolution)
                taskBaseResolution.Await = false;
            var taskResolution = constructorResolution.Initialization switch
            {
                TaskTypeInitializationResolution taskTypeInitialization => new ValueTaskFromTaskResolution(
                    resolution.Item1,
                    taskTypeInitialization,
                    wrappedValueTaskReference,
                    boundValueTaskTypeFullName),
                ValueTaskTypeInitializationResolution taskTypeInitialization => new ValueTaskFromValueTaskResolution(
                    resolution.Item1,
                    taskTypeInitialization,
                    wrappedValueTaskReference,
                    boundValueTaskTypeFullName),
                _ => (Resolvable) new ValueTaskFromSyncResolution(resolution.Item1, wrappedValueTaskReference, boundValueTaskTypeFullName)
            };
            return (taskResolution, resolution.Item2);
        }
        else if (resolution.Item2 is MultiSynchronicityFunctionCallResolution multiSynchronicityFunctionCallResolution)
        {
            var valueTaskReference = RootReferenceGenerator.Generate("valueTask");
            var valueTaskFullName = multiSynchronicityFunctionCallResolution.AsyncValueTask.TypeFullName;
            multiSynchronicityFunctionCallResolution.Sync.Await = false;
            multiSynchronicityFunctionCallResolution.AsyncTask.Await = false;
            multiSynchronicityFunctionCallResolution.AsyncValueTask.Await = false;
            return (new MultiTaskResolution(
                new ValueTaskFromSyncResolution(
                    resolution.Item1,
                    valueTaskReference,
                    valueTaskFullName),
                new ValueTaskFromWrappedTaskResolution(
                    resolution.Item1,
                    valueTaskReference,
                    valueTaskFullName),
                new NewReferenceResolvable(valueTaskReference, valueTaskFullName, resolution.Item1),
                multiSynchronicityFunctionCallResolution.LazySynchronicityDecision), null);
        }
        return (new TaskFromSyncResolution(resolution.Item1, wrappedValueTaskReference, boundValueTaskTypeFullName), resolution.Item2);
    }

    private (Resolvable, ITaskConsumableResolution?) SwitchInterface(SwitchInterfaceParameter parameter)
    {
        var (typeSymbol, currentParameters) = parameter;
        var interfaceType = (INamedTypeSymbol) typeSymbol;
        var implementations = _checkTypeProperties
            .MapToImplementations(typeSymbol);
        var shouldBeScopeRoot = implementations.Max(i => _checkTypeProperties.ShouldBeScopeRoot(i));

        var nextParameter = new SwitchInterfaceAfterScopeRootParameter(
            interfaceType,
            implementations,
            currentParameters);
        
        var ret = shouldBeScopeRoot switch
        {
            ScopeLevel.TransientScope => (_rangeResolutionBaseBuilder.CreateTransientScopeRootResolution(
                nextParameter,
                interfaceType,
                _disposableCollectionResolution,
                currentParameters), null),
            ScopeLevel.Scope => (_rangeResolutionBaseBuilder.CreateScopeRootResolution(
                nextParameter,
                interfaceType,
                _disposableCollectionResolution,
                currentParameters), null),
            _ => SwitchInterfaceAfterScopeRoot(nextParameter)
        };

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: IAwaitableResolution awaitableResolution })
            PotentialAwaits.Add(awaitableResolution);

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: IAwaitableResolution awaitableResolution0 })
            PotentialAwaits.Add(awaitableResolution0);

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution })
            ret.Item2 = taskConsumableResolution;

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution0 })
            ret.Item2 = taskConsumableResolution0;

        return ret;
    }

    protected (Resolvable, ITaskConsumableResolution?) SwitchInterfaceAfterScopeRoot(
        SwitchInterfaceAfterScopeRootParameter parameter)
    {
        var (interfaceType, implementations, currentParameters) = parameter;
        if (_checkTypeProperties.ShouldBeComposite(interfaceType))
        {
            var compositeImplementationType = _checkTypeProperties.GetCompositeFor(interfaceType);
            var interfaceResolutions = implementations.Select(i => CreateInterface(new CreateInterfaceParameter(
                interfaceType,
                i,
                currentParameters))).ToList();
            var composition = new CompositionInterfaceExtension(
                interfaceType,
                implementations.ToList(),
                compositeImplementationType,
                interfaceResolutions.Select(ir => ir.Item1).ToList());
            return CreateInterface(new CreateInterfaceParameterAsComposition(
                interfaceType, 
                compositeImplementationType,
                currentParameters, 
                composition));
        }
        if (implementations.SingleOrDefault() is not { } implementationType)
        {
            return (
                new ErrorTreeItem(implementations.Count switch
                {
                    0 => $"[{interfaceType.FullName()}] Interface: No implementation found",
                    > 1 => $"[{interfaceType.FullName()}] Interface: more than one implementation found",
                    _ =>
                        $"[{interfaceType.FullName()}] Interface: Found single implementation {implementations[0].FullName()} is not a named type symbol"
                }),
                null);
        }

        return CreateInterface(new CreateInterfaceParameter(
            interfaceType,
            implementationType,
            currentParameters));
    }

    private (Resolvable, ITaskConsumableResolution?) SwitchInterfaceForSpecificImplementation(
        SwitchInterfaceForSpecificImplementationParameter parameter)
    {
        var (interfaceType, implementationType, currentParameters) = parameter;
        
        var nextParameter = new CreateInterfaceParameter(
            interfaceType,
            implementationType,
            currentParameters);

        (Resolvable, ITaskConsumableResolution?) ret = _checkTypeProperties.ShouldBeScopeRoot(implementationType) switch
        {
            ScopeLevel.TransientScope => (_rangeResolutionBaseBuilder.CreateTransientScopeRootResolution(
                nextParameter,
                interfaceType,
                _disposableCollectionResolution,
                currentParameters), null),
            ScopeLevel.Scope => (_rangeResolutionBaseBuilder.CreateScopeRootResolution(
                nextParameter,
                interfaceType,
                _disposableCollectionResolution,
                currentParameters), null),
            _ => CreateInterface(nextParameter)
        };

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: IAwaitableResolution awaitableResolution })
            PotentialAwaits.Add(awaitableResolution);

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: IAwaitableResolution awaitableResolution0 })
            PotentialAwaits.Add(awaitableResolution0);

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution })
            ret.Item2 = taskConsumableResolution;

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution0 })
            ret.Item2 = taskConsumableResolution0;

        return ret;
    }

    protected (InterfaceResolution, ITaskConsumableResolution?) CreateInterface(CreateInterfaceParameter parameter)
    {
        var (interfaceType, implementationType, currentParameters) = parameter;
        var shouldBeDecorated = _checkTypeProperties.ShouldBeDecorated(interfaceType);

        var nextParameter = parameter switch
        {
            CreateInterfaceParameterAsComposition asComposition => new SwitchImplementationParameterWithComposition(
                asComposition.Composition.CompositeType,
                currentParameters,
                asComposition.Composition),
            _ => new SwitchImplementationParameter(
                implementationType,
                currentParameters)
        };

        var currentInterfaceResolution = new InterfaceResolution(
            RootReferenceGenerator.Generate(interfaceType),
            interfaceType.FullName(),
            SwitchImplementation(nextParameter).Item1);

        if (shouldBeDecorated)
        {
            var decorators = new Queue<INamedTypeSymbol>(_checkTypeProperties.GetSequenceFor(interfaceType, implementationType));
            while (decorators.Any())
            {
                var decorator = decorators.Dequeue();
                var decoration = new DecorationInterfaceExtension(
                    interfaceType, 
                    implementationType, 
                    decorator,
                    currentInterfaceResolution);
                var decoratorResolution = SwitchImplementation(new SwitchImplementationParameterWithDecoration(
                    decorator,
                    currentParameters,
                    decoration)).Item1;
                currentInterfaceResolution = new InterfaceResolution(
                    RootReferenceGenerator.Generate(interfaceType),
                    interfaceType.FullName(),
                    decoratorResolution);
            }
        }
        
        return (currentInterfaceResolution, currentInterfaceResolution.Dependency as ConstructorResolution);
    }

    private (Resolvable, ITaskConsumableResolution?) SwitchClass(SwitchClassParameter parameter)
    {
        var (typeSymbol, currentParameters) = parameter;
        var implementations = _checkTypeProperties
            .MapToImplementations(typeSymbol);
        var implementationType = implementations.SingleOrDefault();
        if (implementationType is not { })
        {
            return (
                new ErrorTreeItem(implementations.Count switch
                {
                    0 => $"[{typeSymbol.FullName()}] Class: No implementation found",
                    > 1 => $"[{typeSymbol.FullName()}] Class: more than one implementation found",
                    _ => $"[{typeSymbol.FullName()}] Class: Found single implementation{implementations[0].FullName()} is not a named type symbol"
                }),
                null);
        }

        var nextParameter = new SwitchImplementationParameter(
            implementationType,
            currentParameters);
        
        var ret = _checkTypeProperties.ShouldBeScopeRoot(implementationType) switch
        {
            ScopeLevel.TransientScope => (_rangeResolutionBaseBuilder.CreateTransientScopeRootResolution(
                nextParameter,
                implementationType, 
                _disposableCollectionResolution, 
                currentParameters), null),
            ScopeLevel.Scope => (_rangeResolutionBaseBuilder.CreateScopeRootResolution(
                nextParameter, 
                implementationType, 
                _disposableCollectionResolution, 
                currentParameters), null),
            _ => SwitchImplementation(nextParameter)
        };

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: IAwaitableResolution awaitableResolution })
            PotentialAwaits.Add(awaitableResolution);

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: IAwaitableResolution awaitableResolution0 })
            PotentialAwaits.Add(awaitableResolution0);

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution })
            ret.Item2 = taskConsumableResolution;

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution0 })
            ret.Item2 = taskConsumableResolution0;

        return ret;
    }

    protected (Resolvable, ITaskConsumableResolution?) SwitchImplementation(SwitchImplementationParameter parameter)
    {
        var (implementationType, currentParameters) = parameter;
        var scopeLevel = parameter switch
        {
            SwitchImplementationParameterWithComposition withComposition =>
                withComposition.Composition.ImplementationTypes.Select(i => _checkTypeProperties.GetScopeLevelFor(i))
                    .Min(),
            SwitchImplementationParameterWithDecoration withDecoration => _checkTypeProperties.GetScopeLevelFor(
                withDecoration.Decoration.ImplementationType),
            _ => _checkTypeProperties.GetScopeLevelFor(parameter.ImplementationType)
        };
        var nextParameter = parameter switch
        {
            SwitchImplementationParameterWithComposition withComposition => new ForConstructorParameterWithComposition(
                withComposition.Composition.CompositeType, 
                currentParameters,
                withComposition.Composition),
            SwitchImplementationParameterWithDecoration withDecoration => new ForConstructorParameterWithDecoration(
                withDecoration.Decoration.DecoratorType,
                currentParameters,
                withDecoration.Decoration),
            _ => new ForConstructorParameter(implementationType, currentParameters)
        };

        var ret = scopeLevel switch
        {
            >= ScopeLevel.Scope when parameter is SwitchImplementationParameterWithDecoration or SwitchImplementationParameterWithDecoration => 
                (_rangeResolutionBaseBuilder.CreateScopeInstanceReferenceResolution(nextParameter), null),
            ScopeLevel.Container => 
                (_rangeResolutionBaseBuilder.CreateContainerInstanceReferenceResolution(nextParameter), null),
            ScopeLevel.TransientScope => 
                (_rangeResolutionBaseBuilder.CreateTransientScopeInstanceReferenceResolution(nextParameter), null),
            ScopeLevel.Scope => 
                (_rangeResolutionBaseBuilder.CreateScopeInstanceReferenceResolution(nextParameter), null),
            _ => 
                CreateConstructorResolution(nextParameter)
        };

        if (ret.Item1 is IAwaitableResolution awaitableResolution)
            PotentialAwaits.Add(awaitableResolution);

        if (ret.Item2 is null)
            ret.Item2 = ret.Item1 as ITaskConsumableResolution;

        return ret;
    }

    protected (Resolvable, ITaskConsumableResolution?) CreateConstructorResolution(ForConstructorParameter parameter)
    {
        var (implementationType, currentParameters) = parameter;
        
        if (_checkTypeProperties.GetConstructorChoiceFor(implementationType) is not { } constructor)
        {
            return (
                new ErrorTreeItem(implementationType.Constructors.Length switch
                {
                    0 => $"[{implementationType.FullName()}] Class.Constructor: No constructor found for implementation {implementationType.FullName()}",
                    > 1 => $"[{implementationType.FullName()}] Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}",
                    _ => $"[{implementationType.FullName()}] Class.Constructor: {implementationType.Constructors[0].Name} is not a method symbol"
                }),
                null);
        }

        var checkForDecoration = false;
        DecorationInterfaceExtension? decoration = null;
        
        if (parameter is ForConstructorParameterWithDecoration withDecoration)
        {
            checkForDecoration = true;
            decoration = withDecoration.Decoration;
        }
        
        var checkForComposition = false;
        CompositionInterfaceExtension? composition = null;
        
        if (parameter is ForConstructorParameterWithComposition withComposition)
        {
            checkForComposition = true;
            composition = withComposition.Composition;
        }

        var isTransientScopeRoot =
            _checkTypeProperties.ShouldBeScopeRoot(implementationType) == ScopeLevel.TransientScope;

        ITypeInitializationResolution? typeInitializationResolution = null;

        if (_checkTypeProperties.GetInitializerFor(implementationType) is { } tuple)
        {
            var (initializationInterface, initializationMethod) = tuple;
            var initializationTypeFullName = initializationInterface.FullName();
            var initializationMethodName = initializationMethod.Name;
            typeInitializationResolution = initializationMethod.ReturnsVoid switch
            {
                true => new SyncTypeInitializationResolution(initializationTypeFullName, initializationMethodName),
                false when initializationMethod.ReturnType.Equals(_wellKnownTypes.Task, SymbolEqualityComparer.Default) =>
                    new TaskTypeInitializationResolution(
                        initializationTypeFullName, 
                        initializationMethodName,
                        _wellKnownTypes.Task.FullName(),
                        RootReferenceGenerator.Generate(_wellKnownTypes.Task)),
                false when initializationMethod.ReturnType.Equals(_wellKnownTypes.ValueTask, SymbolEqualityComparer.Default) => 
                    new ValueTaskTypeInitializationResolution(
                        initializationTypeFullName, 
                        initializationMethodName, 
                        _wellKnownTypes.ValueTask.FullName(),
                        RootReferenceGenerator.Generate(_wellKnownTypes.ValueTask)),
                _ => typeInitializationResolution
            };

            if (typeInitializationResolution is IAwaitableResolution awaitableResolution)
            {
                PotentialAwaits.Add(awaitableResolution);
            }
        }


        var resolution = new ConstructorResolution(
            RootReferenceGenerator.Generate(implementationType),
            implementationType.FullName(),
            ImplementsIDisposable(
                implementationType, 
                _wellKnownTypes, 
                _disposableCollectionResolution,
                _checkTypeProperties),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(constructor
                .Parameters
                .Select(p => ProcessChildType(p.Type, p.Name, implementationType, currentParameters))
                .ToList()),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(implementationType
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.SetMethod?.IsInitOnly ?? false)
                .Select(p => ProcessChildType(p.Type, p.Name, implementationType, currentParameters))
                .ToList()),
            typeInitializationResolution);

        return (resolution, resolution);

        (string Name, Resolvable Dependency) ProcessChildType(
            ITypeSymbol typeSymbol, 
            string parameterName, 
            INamedTypeSymbol impType, 
            IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currParameter)
        {
            if (checkForDecoration && decoration is {})
            {
                if (typeSymbol.Equals(decoration.InterfaceType, SymbolEqualityComparer.Default))
                    return (parameterName, decoration.CurrentInterfaceResolution);
                    
                if (typeSymbol.Equals(_wellKnownTypes.Task1.Construct(decoration.InterfaceType), SymbolEqualityComparer.Default)
                    && decoration.CurrentInterfaceResolution.Dependency is ConstructorResolution constrRes0)
                    return (parameterName, SwitchTask(new SwitchTaskParameter((decoration.CurrentInterfaceResolution, constrRes0))).Item1);
                    
                if (typeSymbol.Equals(_wellKnownTypes.ValueTask1.Construct(decoration.InterfaceType), SymbolEqualityComparer.Default)
                    && decoration.CurrentInterfaceResolution.Dependency is ConstructorResolution constrRes1)
                    return (parameterName, SwitchValueTask(new SwitchValueTaskParameter((decoration.CurrentInterfaceResolution, constrRes1))).Item1);
            }

            if (checkForComposition && composition is {})
            {
                if (typeSymbol.Equals(_wellKnownTypes.Enumerable1.Construct(composition.InterfaceType), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(_wellKnownTypes.ReadOnlyCollection1.Construct(composition.InterfaceType), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(_wellKnownTypes.ReadOnlyList1.Construct(composition.InterfaceType), SymbolEqualityComparer.Default))
                    return (parameterName, new CollectionResolution(
                        RootReferenceGenerator.Generate(typeSymbol),
                        typeSymbol.FullName(),
                        composition.InterfaceType.FullName(),
                        composition.InterfaceResolutionComposition));
                
                if (typeSymbol.Equals(_wellKnownTypes.Enumerable1.Construct(_wellKnownTypes.Task1.Construct(composition.InterfaceType)), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(_wellKnownTypes.ReadOnlyCollection1.Construct(_wellKnownTypes.Task1.Construct(composition.InterfaceType)), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(_wellKnownTypes.ReadOnlyList1.Construct(_wellKnownTypes.Task1.Construct(composition.InterfaceType)), SymbolEqualityComparer.Default))
                    return (parameterName, new CollectionResolution(
                        RootReferenceGenerator.Generate(typeSymbol),
                        typeSymbol.FullName(),
                        _wellKnownTypes.Task1.Construct(composition.InterfaceType).FullName(),
                        composition.InterfaceResolutionComposition
                            .Select(ir =>
                            {
                                if (ir.Dependency is ConstructorResolution constRes)
                                    return SwitchTask(new SwitchTaskParameter((ir, constRes))).Item1;
                                return null;
                            })
                            .OfType<Resolvable>()
                            .ToList()));
                
                if (typeSymbol.Equals(_wellKnownTypes.Enumerable1.Construct(_wellKnownTypes.ValueTask1.Construct(composition.InterfaceType)), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(_wellKnownTypes.ReadOnlyCollection1.Construct(_wellKnownTypes.ValueTask1.Construct(composition.InterfaceType)), SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(_wellKnownTypes.ReadOnlyList1.Construct(_wellKnownTypes.ValueTask1.Construct(composition.InterfaceType)), SymbolEqualityComparer.Default))
                    return (parameterName, new CollectionResolution(
                        RootReferenceGenerator.Generate(typeSymbol),
                        typeSymbol.FullName(),
                        _wellKnownTypes.ValueTask1.Construct(composition.InterfaceType).FullName(),
                        composition.InterfaceResolutionComposition
                            .Select(ir =>
                            {
                                if (ir.Dependency is ConstructorResolution constRes)
                                    return SwitchValueTask(new SwitchValueTaskParameter((ir, constRes))).Item1;
                                return null;
                            })
                            .OfType<Resolvable>()
                            .ToList()));
            }

            if (isTransientScopeRoot
                && typeSymbol.Equals(_wellKnownTypes.Disposable, SymbolEqualityComparer.Default))
                return (parameterName, new TransientScopeAsDisposableResolution(
                    RootReferenceGenerator.Generate(_wellKnownTypes.Disposable),
                    _wellKnownTypes.Disposable.FullName()));
            if (typeSymbol is not INamedTypeSymbol parameterType)
                return ("",
                    new ErrorTreeItem(
                        $"[{impType.FullName()}] Class.Constructor.Parameter: Parameter type {typeSymbol.FullName()} is not a named type symbol"));

            return (
                parameterName,
                SwitchType(new SwitchTypeParameter(
                    parameterType,
                    currParameter)).Item1);
        }
    }

    protected void AdjustForSynchronicity() =>
        ActualReturnType = SynchronicityDecision.Value switch
        {
            Function.SynchronicityDecision.AsyncTask => _wellKnownTypes.Task1.Construct(OriginalReturnType),
            Function.SynchronicityDecision.AsyncValueTask => _wellKnownTypes.ValueTask1.Construct(OriginalReturnType),
            _ => OriginalReturnType
        };

    private static DisposableCollectionResolution? ImplementsIDisposable(
        INamedTypeSymbol type, 
        WellKnownTypes wellKnownTypes, 
        DisposableCollectionResolution disposableCollectionResolution,
        ICheckTypeProperties checkDisposalManagement) =>
        type.AllInterfaces.Contains(wellKnownTypes.Disposable) && checkDisposalManagement.ShouldBeManaged(type) 
            ? disposableCollectionResolution 
            : null;

    public MultiSynchronicityFunctionCallResolution BuildFunctionCall(
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters, string? ownerReference)
    {
        var returnReference = RootReferenceGenerator.Generate("ret");
        return new(new(returnReference,
                OriginalReturnType.FullName(),
                OriginalReturnType.FullName(),
                Name,
                ownerReference,
                Parameters.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference)).ToList()) 
                { Await = false },
            new(returnReference,
                _wellKnownTypes.Task1.Construct(OriginalReturnType).FullName(),
                OriginalReturnType.FullName(),
                Name,
                ownerReference,
                Parameters.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference)).ToList()),
            new(returnReference,
                _wellKnownTypes.ValueTask1.Construct(OriginalReturnType).FullName(),
                OriginalReturnType.FullName(),
                Name,
                ownerReference,
                Parameters.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference)).ToList()),
            SynchronicityDecision);
    }

    public MethodGroupResolution BuildMethodGroup() => new (Name, TypeFullName, null);

    public bool HasWorkToDo => !Resolvable.IsValueCreated
        || LocalFunctions.Any(lf => lf.HasWorkToDo);

    protected abstract Resolvable CreateResolvable();
    
    public void DoWork()
    {
        var _ = Resolvable.Value;
        while (LocalFunctions.Any(lf => lf.HasWorkToDo))
        {
            foreach (var localFunction in LocalFunctions)
            {
                localFunction.DoWork();
            }
        }
    }

    public abstract FunctionResolution Build();
}