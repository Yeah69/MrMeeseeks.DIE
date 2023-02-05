using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal enum SynchronicityDecision
{
    Undecided,
    Sync,
    AsyncTask,
    AsyncValueTask
}

internal interface IFunctionResolutionSynchronicityDecisionMaker
{
    Lazy<SynchronicityDecision> Decision { get; }
    
    void Register(IAwaitableResolution awaitableResolution);
    void ForceAwait();
}

internal class FunctionResolutionSynchronicityDecisionMaker : IFunctionResolutionSynchronicityDecisionMaker
{
    private readonly ISet<IAwaitableResolution> _potentialAwaits = new HashSet<IAwaitableResolution>();
    private bool _forceAwait;
    
    public Lazy<SynchronicityDecision> Decision { get; }

    public FunctionResolutionSynchronicityDecisionMaker() =>
        Decision = new(() => _forceAwait || _potentialAwaits.Any(pa => pa.Await)
            ? SynchronicityDecision.AsyncValueTask
            : SynchronicityDecision.Sync);

    public void Register(IAwaitableResolution awaitableResolution)
    {
        if (Decision.IsValueCreated)
        {
            throw new InvalidOperationException("Registration of awaitable resolution after (!) synchronicity was decided.");
        }

        _potentialAwaits.Add(awaitableResolution);
    }

    public void ForceAwait() => _forceAwait = true;
}

internal interface IFunctionResolutionBuilder : IResolutionBuilder<FunctionResolution>
{
    FunctionResolutionBuilderHandle Handle { get; }
    ITypeSymbol OriginalReturnType { get; }
    ITypeSymbol? ActualReturnType { get; }
    IReadOnlyList<ParameterResolution> Parameters { get; }
    IReadOnlyList<(ITypeSymbol, ParameterResolution)> CurrentParameters { get; }
    
    MultiSynchronicityFunctionCallResolution BuildFunctionCall(
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters,
        string? ownerReference);
}

internal abstract partial class FunctionResolutionBuilder : IFunctionResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;
    private readonly IFunctionResolutionSynchronicityDecisionMaker _synchronicityDecisionMaker;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;
    private readonly IFunctionCycleTracker _functionCycleTracker;
    private readonly IDiagLogger _diagLogger;
    private readonly ICheckTypeProperties _checkTypeProperties;

    private readonly Dictionary<INamedTypeSymbol, Resolvable> _scopedInstancesReferenceCache = new(SymbolEqualityComparer.Default);

    protected readonly IReferenceGenerator RootReferenceGenerator;
    
    private readonly IUserDefinedElements _userDefinedElements;

    public FunctionResolutionBuilderHandle Handle { get; }
    
    protected abstract string Name { get; }
    protected string TypeFullName => ActualReturnType?.FullName() ?? OriginalReturnType.FullName();

    public Lazy<SynchronicityDecision> SynchronicityDecision => _synchronicityDecisionMaker.Decision;
    protected Lazy<Resolvable> Resolvable { get; } 
    
    public IReadOnlyList<(ITypeSymbol, ParameterResolution)> CurrentParameters { get; }
    
    public IReadOnlyList<ParameterResolution> Parameters { get; }
    
    public ITypeSymbol OriginalReturnType { get; }
    public ITypeSymbol? ActualReturnType { get; protected set; }

    internal FunctionResolutionBuilder(
        // parameters
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        ITypeSymbol returnType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters,
        IFunctionResolutionSynchronicityDecisionMaker synchronicityDecisionMaker,
        object handleIdentity,

        // dependencies
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        IFunctionCycleTracker functionCycleTracker,
        IDiagLogger diagLogger)
    {
        OriginalReturnType = returnType;
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
        _synchronicityDecisionMaker = synchronicityDecisionMaker;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesCollections = wellKnownTypesCollections;
        _functionCycleTracker = functionCycleTracker;
        _diagLogger = diagLogger;
        _checkTypeProperties = rangeResolutionBaseBuilder.CheckTypeProperties;
        _userDefinedElements = rangeResolutionBaseBuilder.UserDefinedElements;
        Handle = new FunctionResolutionBuilderHandle(
            handleIdentity,
            $"(implementation) {OriginalReturnType.FullName()}({string.Join(", ", currentParameters.Select(p => p.Value.Item2.TypeFullName))})");

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        CurrentParameters = currentParameters
            .Select(p => (p.Value.Item1, new ParameterResolution(RootReferenceGenerator.Generate(p.Value.Item1), p.Value.Item2.TypeFullName)))
            .ToList();
        Parameters = CurrentParameters
            .Select(p => p.Item2)
            .ToList();

        Resolvable = new(CreateResolvable);
    }

    private string ErrorMessage(IImmutableStack<INamedTypeSymbol> stack, ITypeSymbol currentType, string message) => 
        $"[R:{_rangeResolutionBaseBuilder.ErrorContext.Prefix}][TS:{(stack.IsEmpty ? "empty" : stack.Peek().FullName())}][CT:{currentType.FullName()}] {message} [S:{(stack.IsEmpty ? "empty" : string.Join("<==", stack.Select(t => t.FullName())))}]";

    protected (Resolvable, ITaskConsumableResolution?) SwitchType(SwitchTypeParameter parameter)
    {
        var (type, currentFuncParameters, implementationStack) = parameter;
        if (currentFuncParameters.FirstOrDefault(t => SymbolEqualityComparer.IncludeNullability.Equals(
                t.Value.Item1.OriginalDefinition, type.OriginalDefinition)) is { Value.Item1: not null, Value.Item2: not null } funcParameter)
            return (funcParameter.Value.Item2, null);

        var valueTaskedType = _wellKnownTypes.ValueTask1.Construct(type);
        var taskedType = _wellKnownTypes.Task1.Construct(type);

        if (_userDefinedElements.GetFactoryFieldFor(type) is { } instance)
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    instance.Name,
                    false),
                null);
        if (_userDefinedElements.GetFactoryFieldFor(valueTaskedType) is { } valueTaskedInstance)
        {
            _synchronicityDecisionMaker.ForceAwait();
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    valueTaskedInstance.Name,
                    true),
                null);
        }
        if (_userDefinedElements.GetFactoryFieldFor(taskedType) is { } taskedInstance)
        {
            _synchronicityDecisionMaker.ForceAwait();
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    taskedInstance.Name,
                    true),
                null);
        }

        if (_userDefinedElements.GetFactoryPropertyFor(type) is { } property)
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    property.Name,
                    false),
                null);
        if (_userDefinedElements.GetFactoryPropertyFor(valueTaskedType) is { } valueTaskedProperty)
        {
            _synchronicityDecisionMaker.ForceAwait();
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    valueTaskedProperty.Name,
                    true),
                null);
        }
        if (_userDefinedElements.GetFactoryPropertyFor(taskedType) is { } taskedProperty)
        {
            _synchronicityDecisionMaker.ForceAwait();
            return (
                new FieldResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    taskedProperty.Name,
                    true),
                null);
        }

        if (_userDefinedElements.GetFactoryMethodFor(type) is { } method)
            return (
                new FactoryResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    method.Name,
                    method
                        .Parameters
                        .Select(p => (p.Name, SwitchType(new SwitchTypeParameter(p.Type, currentFuncParameters, implementationStack)).Item1))
                        .ToList(),
                    false),
                null);
        if (_userDefinedElements.GetFactoryMethodFor(valueTaskedType) is { } valueTaskedMethod)
        {
            _synchronicityDecisionMaker.ForceAwait();
            return (
                new FactoryResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    valueTaskedMethod.Name,
                    valueTaskedMethod
                        .Parameters
                        .Select(p => (p.Name, SwitchType(new SwitchTypeParameter(p.Type, currentFuncParameters, implementationStack)).Item1))
                        .ToList(),
                    true),
                null);
        }
        if (_userDefinedElements.GetFactoryMethodFor(taskedType) is { } taskedMethod)
        {
            _synchronicityDecisionMaker.ForceAwait();
            return (
                new FactoryResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    taskedMethod.Name,
                    taskedMethod
                        .Parameters
                        .Select(p => (p.Name, SwitchType(new SwitchTypeParameter(p.Type, currentFuncParameters, implementationStack)).Item1))
                        .ToList(),
                    true),
                null);
        }

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypes.Task1)
            && type is INamedTypeSymbol task)
            return SwitchTask(new SwitchTaskParameter(
                SwitchType(new SwitchTypeParameter(task.TypeArguments[0], currentFuncParameters, implementationStack))));

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypes.ValueTask1)
            && type is INamedTypeSymbol valueTask)
            return SwitchValueTask(new SwitchValueTaskParameter(
                SwitchType(new SwitchTypeParameter(valueTask.TypeArguments[0], currentFuncParameters, implementationStack))));

        if (type.FullName().StartsWith("global::System.ValueTuple<") && type is INamedTypeSymbol valueTupleType)
        {
            var constructor = valueTupleType
                .InstanceConstructors
                .First(c => c.Parameters.Length > 0);
            var constructorResolution = new ConstructorResolution(
                RootReferenceGenerator.Generate(valueTupleType),
                valueTupleType.FullName(),
                DisposalType.None,
                valueTupleType
                    .TypeArguments
                    .Select((t, i) => (constructor.Parameters[i].Name, SwitchType(new SwitchTypeParameter(t, currentFuncParameters, implementationStack)).Item1))
                    .ToList(),
                Array.Empty<(string Name, Resolvable Dependency)>(),
                null,
                null,
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
                    .Select(t => SwitchType(new SwitchTypeParameter(t, currentFuncParameters, implementationStack)).Item1)
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

        if (type.FullName().StartsWith("global::System.Tuple<") && type is INamedTypeSymbol tupleType)
        {
            var constructorResolution = new ConstructorResolution(
                RootReferenceGenerator.Generate(tupleType),
                tupleType.FullName(),
                DisposalType.None,
                tupleType
                    .TypeArguments
                    .Select((t, i) => (tupleType.InstanceConstructors[0].Parameters[i].Name, SwitchType(new SwitchTypeParameter(t, currentFuncParameters, implementationStack)).Item1))
                    .ToList(),
                Array.Empty<(string Name, Resolvable Dependency)>(),
                null,
                null,
                null);
            return (constructorResolution, constructorResolution);
        }

        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypes.Lazy1)
            && type is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.TypeArguments.SingleOrDefault() is not INamedTypeSymbol genericType)
            {
                return (new ErrorTreeItem(Diagnostics.CompilationError(namedTypeSymbol.TypeArguments.Length switch 
                    {
                        0 => ErrorMessage(implementationStack, namedTypeSymbol, "Lazy: No type argument"),
                        > 1 => ErrorMessage(implementationStack, namedTypeSymbol, "Lazy: more than one type argument"),
                        _ => ErrorMessage(implementationStack, namedTypeSymbol, "Lazy: {namedTypeSymbol.TypeArguments[0].FullName()} is not a named type symbol"),
                    }, _rangeResolutionBaseBuilder.ErrorContext.Location,
                        ExecutionPhase.Resolution)),
                    null);
            }

            var currentParameters = ImmutableSortedDictionary.CreateRange(
                currentFuncParameters.Select(cp => 
                    new KeyValuePair<string, (ITypeSymbol, ParameterResolution)>(
                        cp.Value.Item1.FullName(),
                        cp.Value)));
            
            var constructorInjection = new LazyResolution(
                RootReferenceGenerator.Generate(namedTypeSymbol),
                namedTypeSymbol.FullName(),
                CreateFactoryResolution(genericType, currentParameters));
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
                    new ErrorTreeItem(Diagnostics.CompilationError(
                        ErrorMessage(
                            implementationStack, 
                            type, 
                            "Func: Return type not named"), 
                        _rangeResolutionBaseBuilder.ErrorContext.Location,
                        ExecutionPhase.Resolution)),
                    null);
            }
            
            var lambdaParameters = namedTypeSymbol0
                .TypeArguments
                .Take(namedTypeSymbol0.TypeArguments.Length - 1)
                .Select(ts => (Type: ts, Resolution: new ParameterResolution(RootReferenceGenerator.Generate(ts), ts.FullName())))
                .ToArray();

            var setOfProcessedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

            var nextCurrentParameters = currentFuncParameters;
            
            foreach (var lambdaParameter in lambdaParameters)
            {
                if (setOfProcessedTypes.Contains(lambdaParameter.Type, SymbolEqualityComparer.IncludeNullability)
                    || lambdaParameter.Type is not INamedTypeSymbol && lambdaParameter.Type is not IArrayTypeSymbol)
                    continue;

                setOfProcessedTypes.Add(lambdaParameter.Type);

                nextCurrentParameters = nextCurrentParameters.SetItem(lambdaParameter.Type.FullName(), lambdaParameter);
            }
            
            return (
                new FuncResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    lambdaParameters.Select(t => t.Resolution).ToImmutableArray(),
                    CreateFactoryResolution(returnType, nextCurrentParameters)),
                null);
        }

        if (IsCollectionType(type))
        {
            if (type is not INamedTypeSymbol && type is not IArrayTypeSymbol)
            {
                return (
                    new ErrorTreeItem(Diagnostics.CompilationError(
                        ErrorMessage(
                            implementationStack, 
                            type, 
                            "Collection: Collection is not a named type symbol"), 
                        _rangeResolutionBaseBuilder.ErrorContext.Location,
                        ExecutionPhase.Resolution)),
                    null);
            }

            INamedTypeSymbol wrappedType;
            switch (type)
            {
                case INamedTypeSymbol collectionType
                    when collectionType.TypeArguments.SingleOrDefault() is INamedTypeSymbol innerType:
                    wrappedType = innerType;
                    break;
                case IArrayTypeSymbol { ElementType: INamedTypeSymbol innerType1 }:
                    wrappedType = innerType1;
                    break;
                default:
                    return (new ErrorTreeItem(Diagnostics.CompilationError(
                            "Collection: Item type couldn't be determined", 
                            _rangeResolutionBaseBuilder.ErrorContext.Location,
                            ExecutionPhase.Resolution)),
                        null);
            }

            ITypeSymbol wrappedItemTypeSymbol = wrappedType;
            ITypeSymbol unwrappedItemTypeSymbol = wrappedType;
            TaskType? taskType = null;

            if (SymbolEqualityComparer.Default.Equals(wrappedType.OriginalDefinition, _wellKnownTypes.Task1)
                && type is INamedTypeSymbol enumerableTask)
            {
                wrappedItemTypeSymbol = enumerableTask.TypeArguments[0];
                taskType = TaskType.Task;
            }

            if (SymbolEqualityComparer.Default.Equals(wrappedType.OriginalDefinition, _wellKnownTypes.ValueTask1)
                && type is INamedTypeSymbol enumerableValueTask)
            {
                wrappedItemTypeSymbol = enumerableValueTask.TypeArguments[0];
                taskType = TaskType.ValueTask;
            }
            
            if (wrappedItemTypeSymbol is not INamedTypeSymbol wrappedItemType)
            {
                return (
                    new ErrorTreeItem(Diagnostics.CompilationError(
                        ErrorMessage(
                            implementationStack, 
                            type, 
                            "Collection: Collection's inner type is not a named type symbol"), 
                        _rangeResolutionBaseBuilder.ErrorContext.Location,
                        ExecutionPhase.Resolution)),
                    null);
            }

            if (taskType is { })
                unwrappedItemTypeSymbol = wrappedItemType.TypeArguments[0];
            
            if (unwrappedItemTypeSymbol is not INamedTypeSymbol unwrappedItemType)
            {
                return (
                    new ErrorTreeItem(Diagnostics.CompilationError(
                        ErrorMessage(
                            implementationStack, 
                            type, 
                            "Collection: Collection's inner type is not a named type symbol"), 
                        _rangeResolutionBaseBuilder.ErrorContext.Location,
                        ExecutionPhase.Resolution)),
                    null);
            }
                
            var itemTypeIsInterface = unwrappedItemType.TypeKind == TypeKind.Interface;
            var items = _checkTypeProperties
                .MapToImplementations(unwrappedItemType)
                .Select(i =>
                {
                    var itemResolution = itemTypeIsInterface
                        ? SwitchInterfaceWithoutComposition(new CreateInterfaceParameter(unwrappedItemType, i, currentFuncParameters, implementationStack))
                        : SwitchClass(new SwitchClassParameter(i, currentFuncParameters, implementationStack));
                    return (taskType switch
                    {
                        TaskType.Task => SwitchTask(new SwitchTaskParameter(itemResolution)),
                        TaskType.ValueTask => SwitchValueTask(new SwitchValueTaskParameter(itemResolution)),
                        _ => itemResolution
                    }).Item1;
                })
                .ToList();

            return (
                new ArrayResolution(
                    RootReferenceGenerator.Generate(type),
                    type.FullName(),
                    wrappedItemType.FullName(),
                    items),
                null);
        }

        if (type is { TypeKind: TypeKind.Interface} 
            or { TypeKind: TypeKind.Class, IsAbstract: true }
            && type is INamedTypeSymbol interfaceOrAbstractType)
            return SwitchInterface(new SwitchInterfaceAfterScopeRootParameter(interfaceOrAbstractType, currentFuncParameters, implementationStack));

        if (type is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct} classOrStructType)
            return SwitchClass(new SwitchClassParameter(classOrStructType, currentFuncParameters, implementationStack));

        return (
            new ErrorTreeItem(Diagnostics.CompilationError(
                ErrorMessage(
                    implementationStack, 
                    type, 
                    "Couldn't process in resolution tree creation."),
                _rangeResolutionBaseBuilder.ErrorContext.Location,
                ExecutionPhase.Resolution)),
            null);

        IFactoryResolution CreateFactoryResolution(
            INamedTypeSymbol mostOuterInnerType,
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters)
        {
            var asynchronicityStack = new Stack<SynchronicityDecision>();

            var mostInnerType = UnwrapAsynchronicity(mostOuterInnerType, asynchronicityStack);
            
            var newCreateFunction = _rangeResolutionBaseBuilder.CreateCreateFunctionResolution(
                mostInnerType,
                currentParameters,
                Constants.PrivateKeyword);

            var functionCall = newCreateFunction.BuildFunctionCall(
                currentParameters,
                Constants.ThisKeyword);
            _functionCycleTracker.RegisterRootHandle(functionCall.FunctionResolutionBuilderHandle);

            var currentResolution = (IFactoryResolution)functionCall;
            var currentType = mostInnerType;

            if (asynchronicityStack.Any() && asynchronicityStack.Pop() is var firstAsynchronicity)
            {
                currentType = firstAsynchronicity switch
                {
                    Function.SynchronicityDecision.AsyncValueTask => _wellKnownTypes.ValueTask1.Construct(currentType),
                    Function.SynchronicityDecision.AsyncTask => _wellKnownTypes.Task1.Construct(currentType),
                    _ => throw new ImpossibleDieException(new Guid("3946978C-4A1D-477B-8C4B-D610DE6BC6A7"))
                };
                currentResolution = new AsyncFactoryCallResolution(
                    RootReferenceGenerator.Generate(currentType), 
                    currentType.FullName(), 
                    functionCall,
                    firstAsynchronicity);

                while (asynchronicityStack.Any() && asynchronicityStack.Pop() is var nextAsynchronicity)
                {
                    currentType = nextAsynchronicity switch
                    {
                        Function.SynchronicityDecision.AsyncValueTask => _wellKnownTypes.ValueTask1.Construct(currentType),
                        Function.SynchronicityDecision.AsyncTask => _wellKnownTypes.Task1.Construct(currentType),
                        _ => throw new ImpossibleDieException(new Guid("041B43CA-287F-42BC-AEBB-36B931EDB424"))
                    };
                    currentResolution = nextAsynchronicity switch
                    {
                        Function.SynchronicityDecision.AsyncValueTask => new ValueTaskFromSyncResolution(
                            (Resolvable) currentResolution,
                            RootReferenceGenerator.Generate(currentType), 
                            currentType.FullName()),
                        Function.SynchronicityDecision.AsyncTask =>  new TaskFromSyncResolution(
                            (Resolvable) currentResolution,
                            RootReferenceGenerator.Generate(currentType), 
                            currentType.FullName()),
                        _ => throw new ImpossibleDieException(new Guid("61F9F5A1-0E5B-418F-80D5-BAFC41DCB551"))
                    };
                }
            }

            return currentResolution;

            INamedTypeSymbol UnwrapAsynchronicity(INamedTypeSymbol currentWrappedType, Stack<SynchronicityDecision> stack)
            {
                if (SymbolEqualityComparer.Default.Equals(currentWrappedType.OriginalDefinition, _wellKnownTypes.ValueTask1)
                    && currentWrappedType.TypeArguments.FirstOrDefault() is INamedTypeSymbol innerValueTaskType)
                {
                    stack.Push(Function.SynchronicityDecision.AsyncValueTask);
                    return UnwrapAsynchronicity(innerValueTaskType, stack);
                }
            
                if (SymbolEqualityComparer.Default.Equals(currentWrappedType.OriginalDefinition, _wellKnownTypes.Task1)
                    && currentWrappedType.TypeArguments.FirstOrDefault() is INamedTypeSymbol innerTaskType)
                {
                    stack.Push(Function.SynchronicityDecision.AsyncTask);
                    return UnwrapAsynchronicity(innerTaskType, stack);
                }

                return currentWrappedType;
            }
        }
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
            if (constructorResolution.Initialization is TaskBaseInitializationResolution taskBaseResolution)
                taskBaseResolution.Await = false;
            var taskResolution = constructorResolution.Initialization switch
            {
                TaskInitializationResolution taskInitialization => new TaskFromTaskResolution(
                    resolution.Item1,
                    taskInitialization,
                    wrappedTaskReference,
                    boundTaskTypeFullName),
                ValueTaskInitializationResolution taskInitialization => new TaskFromValueTaskResolution(
                    resolution.Item1,
                    taskInitialization,
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
            if (constructorResolution.Initialization is TaskBaseInitializationResolution taskBaseResolution)
                taskBaseResolution.Await = false;
            var taskResolution = constructorResolution.Initialization switch
            {
                TaskInitializationResolution taskInitialization => new ValueTaskFromTaskResolution(
                    resolution.Item1,
                    taskInitialization,
                    wrappedValueTaskReference,
                    boundValueTaskTypeFullName),
                ValueTaskInitializationResolution taskInitialization => new ValueTaskFromValueTaskResolution(
                    resolution.Item1,
                    taskInitialization,
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

    private (Resolvable, ITaskConsumableResolution?) SwitchInterface(
        SwitchInterfaceAfterScopeRootParameter parameter)
    {
        var (interfaceType, currentParameters, implementationStack) = parameter;
        if (_checkTypeProperties.ShouldBeComposite(interfaceType))
        {
            var implementations = _checkTypeProperties.MapToImplementations(interfaceType);
            var compositeImplementationType = _checkTypeProperties.GetCompositeFor(interfaceType)
                ?? throw new ImpossibleDieException(new Guid("2E4A28E8-573B-4087-95FE-BDED04DB3981"));
            var interfaceResolutions = implementations.Select(i => SwitchInterfaceWithoutComposition(new CreateInterfaceParameter(
                interfaceType,
                i,
                currentParameters,
                implementationStack))).ToList();
            var composition = new CompositionInterfaceExtension(
                interfaceType,
                implementations.ToList(),
                compositeImplementationType,
                interfaceResolutions.Select(ir => ir.Item1).ToList());
            return SwitchInterfaceWithoutComposition(new CreateInterfaceParameterAsComposition(
                interfaceType, 
                compositeImplementationType,
                currentParameters,
                implementationStack, 
                composition));
        }
        if (_checkTypeProperties.MapToSingleFittingImplementation(interfaceType) is not { } implementationType)
        {
            if (interfaceType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                _diagLogger.Log(Diagnostics.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\", but injecting null instead.",
                    ExecutionPhase.Resolution));
                return (new NullResolution(RootReferenceGenerator.Generate(interfaceType), interfaceType.FullName()),
                    null);
            }
            return (new ErrorTreeItem(Diagnostics.CompilationError(
                    ErrorMessage(
                        implementationStack,
                        interfaceType,
                        $"Interface: Multiple or no implementations where a single is required for \"{interfaceType.FullName()}\"."),
                    _rangeResolutionBaseBuilder.ErrorContext.Location,
                    ExecutionPhase.Resolution)),
                null);
        }

        return SwitchInterfaceWithoutComposition(new CreateInterfaceParameter(
            interfaceType,
            implementationType,
            currentParameters,
            implementationStack));
    }

    private (InterfaceResolution, ITaskConsumableResolution?) SwitchInterfaceWithoutComposition(CreateInterfaceParameter parameter)
    {
        var (interfaceType, implementationType, currentParameters, implementationStack) = parameter;
        var shouldBeDecorated = _checkTypeProperties.ShouldBeDecorated(interfaceType);

        var (nextResolvable, _) = parameter switch
        {
            CreateInterfaceParameterAsComposition asComposition => SwitchImplementation(new SwitchImplementationParameterWithComposition(
                asComposition.Composition.CompositeType,
                currentParameters,
                implementationStack,
                asComposition.Composition)),
            _ => SwitchClass(new SwitchClassParameter(
                implementationType,
                currentParameters,
                implementationStack))
        };

        var currentInterfaceResolution = new InterfaceResolution(
            RootReferenceGenerator.Generate(interfaceType),
            interfaceType.FullName(),
            nextResolvable);

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
                    implementationStack,
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
        var (implementationType, currentParameters, implementationStack) = parameter;
        
        if (_checkTypeProperties.MapToSingleFittingImplementation(implementationType) is not { } chosenImplementationType)
        {
            if (implementationType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                _diagLogger.Log(Diagnostics.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{implementationType.FullName()}\", but injecting null instead.",
                    ExecutionPhase.Resolution));
                return (
                    new NullResolution(RootReferenceGenerator.Generate(implementationType),
                        implementationType.FullName()), null);
            }
            return (new ErrorTreeItem(Diagnostics.CompilationError(
                    ErrorMessage(
                        implementationStack,
                        implementationType,
                        $"Interface: Multiple or no implementations where a single is required for \"{implementationType.FullName()}\","),
                    _rangeResolutionBaseBuilder.ErrorContext.Location,
                    ExecutionPhase.Resolution)),
                null);
        }

        var nextParameter = new SwitchImplementationParameter(
            chosenImplementationType,
            currentParameters,
            implementationStack);
        
        var ret = _checkTypeProperties.ShouldBeScopeRoot(chosenImplementationType) switch
        {
            ScopeLevel.TransientScope => (_rangeResolutionBaseBuilder.CreateTransientScopeRootResolution(
                nextParameter,
                chosenImplementationType, 
                currentParameters), null),
            ScopeLevel.Scope => (_rangeResolutionBaseBuilder.CreateScopeRootResolution(
                nextParameter, 
                chosenImplementationType, 
                currentParameters), null),
            _ => SwitchImplementation(nextParameter)
        };
        
        if (ret.Item1 is MultiSynchronicityFunctionCallResolution multi)
            _functionCycleTracker.TrackFunctionCall(Handle, multi.FunctionResolutionBuilderHandle);

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: { } awaitableResolution })
        {
            _functionCycleTracker.TrackFunctionCall(Handle, awaitableResolution.FunctionResolutionBuilderHandle);
            _synchronicityDecisionMaker.Register(awaitableResolution);
        }

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: { } awaitableResolution0 })
        {
            _functionCycleTracker.TrackFunctionCall(Handle, awaitableResolution0.FunctionResolutionBuilderHandle);
            _synchronicityDecisionMaker.Register(awaitableResolution0);
        }

        if (ret.Item1 is ScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution })
            ret.Item2 = taskConsumableResolution;

        if (ret.Item1 is TransientScopeRootResolution { ScopeRootFunction: ITaskConsumableResolution taskConsumableResolution0 })
            ret.Item2 = taskConsumableResolution0;

        return ret;
    }

    protected (Resolvable, ITaskConsumableResolution?) SwitchImplementation(SwitchImplementationParameter parameter)
    {
        var (implementationType, currentParameters, implementationStack) = parameter;
        var scopeLevel = parameter switch
        {
            SwitchImplementationParameterWithDecoration or SwitchImplementationParameterWithComposition => ScopeLevel.None,
            _ => _checkTypeProperties.GetScopeLevelFor(parameter.ImplementationType)
        };
        var nextParameter = parameter switch
        {
            SwitchImplementationParameterWithComposition withComposition => new ForConstructorParameterWithComposition(
                withComposition.Composition.CompositeType, 
                currentParameters,
                implementationStack,
                withComposition.Composition),
            SwitchImplementationParameterWithDecoration withDecoration => new ForConstructorParameterWithDecoration(
                withDecoration.Decoration.DecoratorType,
                currentParameters,
                implementationStack,
                withDecoration.Decoration),
            _ => new ForConstructorParameter(implementationType, currentParameters, implementationStack)
        };

        if (scopeLevel != ScopeLevel.None 
            && _scopedInstancesReferenceCache.TryGetValue(implementationType, out var scopedReference))
            return (scopedReference, null);

        var ret = scopeLevel switch
        {
            ScopeLevel.Container => 
                (_rangeResolutionBaseBuilder.CreateContainerInstanceReferenceResolution(nextParameter), null),
            ScopeLevel.TransientScope => 
                (_rangeResolutionBaseBuilder.CreateTransientScopeInstanceReferenceResolution(nextParameter), null),
            ScopeLevel.Scope => 
                (_rangeResolutionBaseBuilder.CreateScopeInstanceReferenceResolution(nextParameter), null),
            _ => 
                CreateConstructorResolution(nextParameter)
        };
        
        if (scopeLevel != ScopeLevel.None)
            _scopedInstancesReferenceCache[implementationType] = new ProxyResolvable(ret.Item1.Reference, ret.Item1.TypeFullName);
        
        if (ret.Item1 is MultiSynchronicityFunctionCallResolution multi)
            _functionCycleTracker.TrackFunctionCall(Handle, multi.FunctionResolutionBuilderHandle);

        if (ret.Item1 is IAwaitableResolution awaitableResolution)
            _synchronicityDecisionMaker.Register(awaitableResolution);

        if (ret.Item2 is null)
            ret.Item2 = ret.Item1 as ITaskConsumableResolution;

        return ret;
    }

    protected (Resolvable, ITaskConsumableResolution?) CreateConstructorResolution(ForConstructorParameter parameter)
    {
        var (implementationType, currentParameters, implementationStack) = parameter;

        if (_checkTypeProperties.GetConstructorChoiceFor(implementationType) is not { } constructor)
        {
            if (implementationType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                _diagLogger.Log(Diagnostics.NullResolutionWarning(
                    $"Interface: Multiple or no implementations where a single is required for \"{implementationType.FullName()}\", but injecting null instead.",
                    ExecutionPhase.Resolution));
                return (new NullResolution(RootReferenceGenerator.Generate(implementationType),
                    implementationType.FullName()), null);
            }
            return (new ErrorTreeItem(Diagnostics.CompilationError(
                    implementationType.InstanceConstructors.Length switch
                    {
                        0 => ErrorMessage(implementationStack, implementationType,
                            $"Class.Constructor: No constructor found for implementation {implementationType.FullName()}"),
                        > 1 => ErrorMessage(implementationStack, implementationType,
                            $"Class.Constructor: More than one constructor found for implementation {implementationType.FullName()}"),
                        _ => ErrorMessage(implementationStack, implementationType,
                            $"Class.Constructor: {implementationType.InstanceConstructors[0].Name} is not a method symbol")
                    }, _rangeResolutionBaseBuilder.ErrorContext.Location,
                    ExecutionPhase.Resolution)),
                null);
        }

        var implementationCycle = implementationStack.Contains(implementationType, SymbolEqualityComparer.Default);

        if (implementationCycle)
        {
            var cycleStack = ImmutableStack.Create(implementationType);
            var stack = implementationStack;
            var i = implementationType;
            do
            {
                stack = stack.Pop(out var popped);
                cycleStack = cycleStack.Push(popped);
                i = popped;
            } while (!SymbolEqualityComparer.Default.Equals(implementationType, i));
            
            throw new ImplementationCycleDieException(cycleStack);
        }

        implementationStack = implementationStack.Push(implementationType);
        
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

        var (userDefinedConstructorParametersInjection, outConstructorParameters) = GetUserDefinedInjectionResolution(
            _userDefinedElements.GetConstructorParametersInjectionFor(implementationType),
            (name, parameters) => new UserDefinedConstructorParametersInjectionResolution(name, parameters));

        var (userDefinedPropertiesInjection, outProperties) = GetUserDefinedInjectionResolution(
            _userDefinedElements.GetPropertiesInjectionFor(implementationType),
            (name, parameters) => new UserDefinedPropertiesInjectionResolution(name, parameters));

        IInitializationResolution? typeInitializationResolution = null;

        if (_checkTypeProperties.GetInitializerFor(implementationType) is { } tuple)
        {
            var (userDefinedInitializerParametersInjection, outInitializerParameters) = GetUserDefinedInjectionResolution(
                _userDefinedElements.GetInitializerParametersInjectionFor(implementationType),
                (name, parameters) => new UserDefinedInitializerParametersInjectionResolution(name, parameters));
            
            var (initializationInterface, initializationMethod) = tuple;
            var initializationTypeFullName = initializationInterface.FullName();
            var initializationMethodName = initializationMethod.Name;

            var parameters = new ReadOnlyCollection<(string Name, Resolvable Dependency)>(tuple
                .Initializer
                .Parameters
                .Select(p => ProcessInitializerParametersChildType(p.Type, p.Name, currentParameters))
                .ToList());
            
            typeInitializationResolution = initializationMethod.ReturnsVoid switch
            {
                true => new SyncInitializationResolution(
                    initializationTypeFullName, 
                    initializationMethodName, 
                    parameters, 
                    userDefinedInitializerParametersInjection),
                false when SymbolEqualityComparer.Default.Equals(initializationMethod.ReturnType, _wellKnownTypes.Task) =>
                    new TaskInitializationResolution(
                        initializationTypeFullName, 
                        initializationMethodName,
                        _wellKnownTypes.Task.FullName(),
                        RootReferenceGenerator.Generate(_wellKnownTypes.Task),
                        parameters,
                        userDefinedInitializerParametersInjection),
                false when SymbolEqualityComparer.Default.Equals(initializationMethod.ReturnType, _wellKnownTypes.ValueTask) => 
                    new ValueTaskInitializationResolution(
                        initializationTypeFullName, 
                        initializationMethodName, 
                        _wellKnownTypes.ValueTask.FullName(),
                        RootReferenceGenerator.Generate(_wellKnownTypes.ValueTask),
                        parameters,
                        userDefinedInitializerParametersInjection),
                _ => typeInitializationResolution
            };

            if (typeInitializationResolution is IAwaitableResolution awaitableResolution)
            {
                _synchronicityDecisionMaker.Register(awaitableResolution);
            }

            (string Name, Resolvable Dependency) ProcessInitializerParametersChildType(
                ITypeSymbol typeSymbol,
                string parameterName,
                ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currParameter)
            {
                if (outInitializerParameters.TryGetValue(parameterName, out var outParameterResolution))
                    return (parameterName,
                        new ParameterResolution(outParameterResolution.Reference, outParameterResolution.TypeFullName));
                return ProcessChildType(typeSymbol, parameterName, currParameter);
            }
        }

        var resolution = new ConstructorResolution(
            RootReferenceGenerator.Generate(implementationType),
            implementationType.FullName(SymbolDisplayMiscellaneousOptions.None),
            GetDisposalTypeFor(implementationType),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(constructor
                .Parameters
                .Select(p => ProcessConstructorParametersChildType(p.Type, p.Name, currentParameters))
                .ToList()),
            new ReadOnlyCollection<(string Name, Resolvable Dependency)>(
                (_checkTypeProperties.GetPropertyChoicesFor(implementationType) 
                 ?? implementationType
                     .GetMembers()
                     .OfType<IPropertySymbol>()
                     .Where(_ => !implementationType.IsRecord)
                     .Where(p => p.SetMethod?.IsInitOnly ?? false))
                .Select(p => ProcessPropertyChildType(p.Type, p.Name, currentParameters))
                .ToList()),
            typeInitializationResolution,
            userDefinedConstructorParametersInjection,
            userDefinedPropertiesInjection);

        return (resolution, resolution);

        (string Name, Resolvable Dependency) ProcessConstructorParametersChildType(
            ITypeSymbol typeSymbol,
            string parameterName,
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currParameter)
        {
            if (outConstructorParameters.TryGetValue(parameterName, out var outParameterResolution))
                return (parameterName,
                    new ParameterResolution(outParameterResolution.Reference, outParameterResolution.TypeFullName));
            return ProcessChildType(typeSymbol, parameterName, currParameter);
        }

        (string Name, Resolvable Dependency) ProcessPropertyChildType(
            ITypeSymbol typeSymbol,
            string parameterName,
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currParameter)
        {
            if (outProperties.TryGetValue(parameterName, out var outPropertyResolution))
                return (parameterName,
                    new ParameterResolution(outPropertyResolution.Reference, outPropertyResolution.TypeFullName));
            return ProcessChildType(typeSymbol, parameterName, currParameter);
        }

        (string Name, Resolvable Dependency) ProcessChildType(
            ITypeSymbol typeSymbol, 
            string parameterName, 
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currParameter)
        {
            if (checkForDecoration && decoration is {})
            {
                if (SymbolEqualityComparer.Default.Equals(typeSymbol, decoration.InterfaceType))
                    return (parameterName, decoration.CurrentInterfaceResolution);
                    
                if (SymbolEqualityComparer.Default.Equals(typeSymbol, _wellKnownTypes.Task1.Construct(decoration.InterfaceType))
                    && decoration.CurrentInterfaceResolution.Dependency is ConstructorResolution constrRes0)
                    return (parameterName, SwitchTask(new SwitchTaskParameter((decoration.CurrentInterfaceResolution, constrRes0))).Item1);
                    
                if (SymbolEqualityComparer.Default.Equals(typeSymbol, _wellKnownTypes.ValueTask1.Construct(decoration.InterfaceType))
                    && decoration.CurrentInterfaceResolution.Dependency is ConstructorResolution constrRes1)
                    return (parameterName, SwitchValueTask(new SwitchValueTaskParameter((decoration.CurrentInterfaceResolution, constrRes1))).Item1);
            }

            if (checkForComposition && composition is {})
            {
                if (IsConstructedCollectionType(typeSymbol, composition.InterfaceType))
                    return (parameterName, new ArrayResolution(
                        RootReferenceGenerator.Generate(typeSymbol),
                        typeSymbol.FullName(),
                        composition.InterfaceType.FullName(),
                        composition.InterfaceResolutionComposition));
                
                if (IsConstructedCollectionType(typeSymbol, _wellKnownTypes.Task1.Construct(composition.InterfaceType)))
                    return (parameterName, new ArrayResolution(
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
                
                if (IsConstructedCollectionType(typeSymbol, _wellKnownTypes.ValueTask1.Construct(composition.InterfaceType)))
                    return (parameterName, new ArrayResolution(
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
                && SymbolEqualityComparer.Default.Equals(typeSymbol, _wellKnownTypes.IDisposable))
                return (parameterName, new TransientScopeAsSyncDisposableResolution(
                    RootReferenceGenerator.Generate(_wellKnownTypes.IDisposable),
                    _wellKnownTypes.IDisposable.FullName()));

            if (isTransientScopeRoot
                && SymbolEqualityComparer.Default.Equals(typeSymbol, _wellKnownTypes.IAsyncDisposable))
                return (parameterName, new TransientScopeAsAsyncDisposableResolution(
                    RootReferenceGenerator.Generate(_wellKnownTypes.IAsyncDisposable),
                    _wellKnownTypes.IAsyncDisposable.FullName()));
            
            if (typeSymbol is not INamedTypeSymbol && typeSymbol is not IArrayTypeSymbol)
                return ("",
                    new ErrorTreeItem(Diagnostics.CompilationError(
                        ErrorMessage(
                            implementationStack, 
                            typeSymbol, 
                            $"Class.Constructor.Parameter: Parameter type {typeSymbol.FullName()} is not a named type symbol"), 
                        _rangeResolutionBaseBuilder.ErrorContext.Location,
                        ExecutionPhase.Resolution)));

            return (
                parameterName,
                SwitchType(new SwitchTypeParameter(
                    typeSymbol,
                    currParameter,
                    implementationStack)).Item1);
        }

        (TInjection?, IImmutableDictionary<string, OutParameterResolution>) GetUserDefinedInjectionResolution<TInjection>(
            IMethodSymbol? userDefinedInjectionMethod, 
            Func<string, IReadOnlyList<(string Name, Resolvable Dependency, bool isOut)>, TInjection> injectionResolutionFactory) 
            where TInjection : UserDefinedInjectionResolution
        {
            var outParameters = ImmutableDictionary<string, OutParameterResolution>.Empty;
            TInjection? userDefinedInjectionResolution = null;

            if (userDefinedInjectionMethod is not null)
            {
                userDefinedInjectionResolution = injectionResolutionFactory(
                    userDefinedInjectionMethod.Name,
                    userDefinedInjectionMethod
                        .Parameters
                        .Select(p =>
                        {
                            var isOut = p.RefKind == RefKind.Out;

                            if (isOut)
                            {
                                var outParameter = new OutParameterResolution(
                                    RootReferenceGenerator.Generate(p.Type),
                                    p.Type.FullName());
                                outParameters = outParameters.Add(p.Name, outParameter);
                                return (
                                    p.Name,
                                    outParameter,
                                    isOut);
                            }

                            return (
                                p.Name,
                                SwitchType(new SwitchTypeParameter(p.Type, currentParameters, implementationStack)).Item1,
                                isOut);
                        })
                        .ToList());
            }

            return (userDefinedInjectionResolution, outParameters);
        }
    }

    private DisposalType GetDisposalTypeFor(INamedTypeSymbol type)
    {
        var disposalType = _checkTypeProperties.ShouldDisposalBeManaged(type);
        _rangeResolutionBaseBuilder.RegisterDisposalType(disposalType);
        return disposalType;
    }

    protected void AdjustForSynchronicity() =>
        ActualReturnType = SynchronicityDecision.Value switch
        {
            Function.SynchronicityDecision.AsyncTask => _wellKnownTypes.Task1.Construct(OriginalReturnType),
            Function.SynchronicityDecision.AsyncValueTask => _wellKnownTypes.ValueTask1.Construct(OriginalReturnType),
            _ => OriginalReturnType
        };

    public MultiSynchronicityFunctionCallResolution BuildFunctionCall(
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters, string? ownerReference)
    {
        var returnReference = RootReferenceGenerator.Generate("ret");
        return new(new(returnReference,
                OriginalReturnType.FullName(),
                OriginalReturnType.FullName(),
                Name,
                ownerReference,
                CreateParameter()) 
                { Await = false },
            new(returnReference,
                _wellKnownTypes.Task1.Construct(OriginalReturnType).FullName(),
                OriginalReturnType.FullName(),
                Name,
                ownerReference,
                CreateParameter()),
            new(returnReference,
                _wellKnownTypes.ValueTask1.Construct(OriginalReturnType).FullName(),
                OriginalReturnType.FullName(),
                Name,
                ownerReference,
                CreateParameter()),
            SynchronicityDecision,
            Handle);

        IReadOnlyList<(string Name, string Reference)> CreateParameter() =>
            Parameters.Join(
                currentParameters,
                p => p.TypeFullName,
                p => p.Value.Item2.TypeFullName,
                (p, cp) => (p.Reference, cp.Value.Item2.Reference)).ToList();
    }

    private bool IsCollectionType(ITypeSymbol type) =>
        SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IEnumerable1)
        || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyCollection1)
        || SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, _wellKnownTypesCollections.IReadOnlyList1)
        || type is IArrayTypeSymbol;

    private bool IsConstructedCollectionType(ITypeSymbol type, INamedTypeSymbol interfaceType) =>
        SymbolEqualityComparer.Default.Equals(type, _wellKnownTypesCollections.IEnumerable1.Construct(interfaceType))
        || SymbolEqualityComparer.Default.Equals(type, _wellKnownTypesCollections.IReadOnlyCollection1.Construct(interfaceType))
        || SymbolEqualityComparer.Default.Equals(type, _wellKnownTypesCollections.IReadOnlyList1.Construct(interfaceType))
        || type is IArrayTypeSymbol { ElementType: {} elementType } && SymbolEqualityComparer.Default.Equals(elementType, interfaceType);

    public bool HasWorkToDo => !Resolvable.IsValueCreated;

    protected abstract Resolvable CreateResolvable();
    
    public void DoWork()
    {
        var _ = Resolvable.Value;
    }

    public abstract FunctionResolution Build();
}