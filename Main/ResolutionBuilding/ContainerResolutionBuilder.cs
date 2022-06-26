using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder : IRangeResolutionBaseBuilder, IResolutionBuilder<ContainerResolution>
{
    void AddCreateResolveFunctions(IReadOnlyList<(INamedTypeSymbol, string)> createFunctionData);

    MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);
    
    IFunctionCycleTracker FunctionCycleTracker { get; }
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder, ITransientScopeImplementationResolutionBuilder
{
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IContainerCreateFunctionResolutionBuilder> _createFunctionResolutionBuilderFactory;
    private readonly Func<IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakerFactory;

    private readonly List<(IContainerCreateFunctionResolutionBuilder CreateFunction, string MethodNamePrefix)> _rootResolutions = new ();
    private readonly string _transientScopeAdapterReference;
    private readonly IScopeManager _scopeManager;

    private DisposalType _disposalType = DisposalType.None;

    internal ContainerResolutionBuilder(
        // parameters
        IContainerInfo containerInfo,
        
        // dependencies
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        ICheckTypeProperties checkTypeProperties,
        WellKnownTypes wellKnownTypes,
        Func<IContainerResolutionBuilder, ITransientScopeInterfaceResolutionBuilder, IScopeManager> scopeManagerFactory,
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IContainerCreateFunctionResolutionBuilder> createFunctionResolutionBuilderFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory, 
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory,
        IUserDefinedElements userDefinedElement, 
        IFunctionCycleTracker functionCycleTracker) 
        : base(
            containerInfo.Name, 
            checkTypeProperties,
            userDefinedElement,
            wellKnownTypes, 
            referenceGeneratorFactory,
            rangedFunctionGroupResolutionBuilderFactory,
            synchronicityDecisionMakerFactory,
            localFunctionResolutionBuilderFactory)
    {
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _wellKnownTypes = wellKnownTypes;
        _createFunctionResolutionBuilderFactory = createFunctionResolutionBuilderFactory;
        _synchronicityDecisionMakerFactory = synchronicityDecisionMakerFactory;
        FunctionCycleTracker = functionCycleTracker;
        _scopeManager = scopeManagerFactory(this, transientScopeInterfaceResolutionBuilder);
        
        transientScopeInterfaceResolutionBuilder.AddImplementation(this);
        _transientScopeAdapterReference = RootReferenceGenerator.Generate("TransientScopeAdapter");

        ErrorContext = new ErrorContext(
            containerInfo.Name,
            containerInfo.ContainerType.Locations.FirstOrDefault() ?? Location.None);
    }

    public void AddCreateResolveFunctions(IReadOnlyList<(INamedTypeSymbol, string)> createFunctionData)
    {
        foreach (var (typeSymbol, methodNamePrefix) in createFunctionData)
            _rootResolutions.Add((_createFunctionResolutionBuilderFactory(this, typeSymbol), methodNamePrefix));
    }

    public MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Container",
            null,
            containerReference,
            new (_synchronicityDecisionMakerFactory));

    public IFunctionCycleTracker FunctionCycleTracker { get; }

    public override IErrorContext ErrorContext { get; }

    public override MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        CreateContainerInstanceReferenceResolution(parameter, Constants.ThisKeyword);

    public override MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, _transientScopeAdapterReference);

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetTransientScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                Constants.ThisKeyword,
                currentParameters);

    public override ScopeRootResolution CreateScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType, 
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                Constants.ThisKeyword,
                _transientScopeAdapterReference,
                currentParameters);

    public override void RegisterDisposalType(DisposalType disposalType)
    {
        if (disposalType > _disposalType) _disposalType = disposalType;
    }

    public bool HasWorkToDo =>
        _rootResolutions.Any(r => r.CreateFunction.HasWorkToDo)    
        || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo)
        || LocalFunctions.Values.Any(r => r.HasWorkToDo)
        || _scopeManager.HasWorkToDo;

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            foreach ((IContainerCreateFunctionResolutionBuilder createFunction, _) in _rootResolutions.Where(r => r.CreateFunction.HasWorkToDo).ToList())
            {
                createFunction.DoWork();
            }
            
            DoRangedInstancesWork();
            DoLocalFunctionsWork();
            _scopeManager.DoWork();
        }
    }

    public ContainerResolution Build()
    {
        var rootFunctions = new List<RootResolutionFunction>();
        foreach (var (createFunction, methodNamePrefix) in _rootResolutions)
        {
            var privateFunctionResolution = createFunction.Build();
            var privateRootResolutionFunction = new RootResolutionFunction(
                privateFunctionResolution.Reference,
                privateFunctionResolution.TypeFullName,
                "private",
                privateFunctionResolution.Resolvable,
                privateFunctionResolution.Parameter,
                privateFunctionResolution.SynchronicityDecision);
            
            rootFunctions.Add(privateRootResolutionFunction);

            // Create function stays sync
            if (createFunction.OriginalReturnType.Equals(
                    createFunction.ActualReturnType,
                    SymbolEqualityComparer.Default))
            {
                var call = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                call.Sync.Await = false;
                call.AsyncTask.Await = false;
                call.AsyncValueTask.Await = false;
                var publicSyncResolutionFunction = new RootResolutionFunction(
                    methodNamePrefix,
                    privateRootResolutionFunction.TypeFullName,
                    "public",
                    call,
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicSyncResolutionFunction);
                
                var boundTaskTypeFullName = _wellKnownTypes
                    .Task1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var taskCall = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                taskCall.Sync.Await = false;
                taskCall.AsyncTask.Await = false;
                taskCall.AsyncValueTask.Await = false;
                var publicTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}Async",
                    boundTaskTypeFullName,
                    "public",
                    new TaskFromSyncResolution(
                        taskCall, 
                        wrappedTaskReference, 
                        boundTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicTaskResolutionFunction);
                
                var boundValueTaskTypeFullName = _wellKnownTypes
                    .ValueTask1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var valueTaskCall = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                valueTaskCall.Sync.Await = false;
                valueTaskCall.AsyncTask.Await = false;
                valueTaskCall.AsyncValueTask.Await = false;
                var publicValueTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}ValueAsync",
                    boundValueTaskTypeFullName,
                    "public",
                    new ValueTaskFromSyncResolution(
                        valueTaskCall, 
                        wrappedValueTaskReference, 
                        boundValueTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicValueTaskResolutionFunction);
            }
            else if (createFunction.ActualReturnType is { } actual
                     && actual.Equals(_wellKnownTypes.Task1.Construct(createFunction.OriginalReturnType),
                         SymbolEqualityComparer.Default))
            {
                var call = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                call.Sync.Await = false;
                call.AsyncTask.Await = false;
                call.AsyncValueTask.Await = false;
                var publicTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}Async",
                    actual.FullName(),
                    "public",
                    call,
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicTaskResolutionFunction);
                
                var boundValueTaskTypeFullName = _wellKnownTypes
                    .ValueTask1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var valueCall = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                valueCall.Sync.Await = false;
                valueCall.AsyncTask.Await = false;
                valueCall.AsyncValueTask.Await = false;
                var publicValueTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}ValueAsync",
                    boundValueTaskTypeFullName,
                    "public",
                    new ValueTaskFromWrappedTaskResolution(
                        valueCall, 
                        wrappedValueTaskReference, 
                        boundValueTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicValueTaskResolutionFunction);
            }
            else if (createFunction.ActualReturnType is { } actual1
                     && actual1.Equals(_wellKnownTypes.ValueTask1.Construct(createFunction.OriginalReturnType),
                         SymbolEqualityComparer.Default))
            {
                var boundTaskTypeFullName = _wellKnownTypes
                    .Task1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var call = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                call.Sync.Await = false;
                call.AsyncTask.Await = false;
                call.AsyncValueTask.Await = false;
                var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.ValueTask);
                var publicTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}Async",
                    boundTaskTypeFullName,
                    "public",
                    new TaskFromWrappedValueTaskResolution(
                        call, 
                        wrappedTaskReference, 
                        boundTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicTaskResolutionFunction);
                
                var valueCall = createFunction.BuildFunctionCall(
                    Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                    Constants.ThisKeyword);
                valueCall.Sync.Await = false;
                valueCall.AsyncTask.Await = false;
                valueCall.AsyncValueTask.Await = false;
                var publicValueTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}ValueAsync",
                    actual1.FullName(),
                    "public",
                    valueCall,
                    privateRootResolutionFunction.Parameter,
                    SynchronicityDecision.Sync);
            
                rootFunctions.Add(publicValueTaskResolutionFunction);
            }
        }

        var localFunctions = LocalFunctions
            .Values
            .Select(lf => lf.Build())
            .Select(f => new LocalFunctionResolution(
                f.Reference,
                f.TypeFullName,
                f.Resolvable,
                f.Parameter,
                f.SynchronicityDecision))
            .ToList();
        
        var (transientScopeResolutions, scopeResolutions) = _scopeManager.Build();

        return new(
            rootFunctions,
            localFunctions,
            BuildDisposalHandling(),
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _transientScopeInterfaceResolutionBuilder.Build(),
            _transientScopeAdapterReference,
            transientScopeResolutions,
            scopeResolutions,
            _disposalType,
            RootReferenceGenerator.Generate("transientScopeDisposal"),
            RootReferenceGenerator.Generate("transientScopeToDispose"),
            new NopDisposable(
                RootReferenceGenerator.Generate("NopDisposable"),
                RootReferenceGenerator.Generate("Instance")),
            new NopAsyncDisposable(
                RootReferenceGenerator.Generate("NopDisposable"),
                RootReferenceGenerator.Generate("Instance")),
            new SyncToAsyncDisposable(
                RootReferenceGenerator.Generate("NopDisposable"),
                RootReferenceGenerator.Generate("disposable"),
                RootReferenceGenerator.Generate("_disposable")),
            AddForDisposal,
            AddForDisposalAsync);
    }

    public MultiSynchronicityFunctionCallResolution EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker) => CreateRangedInstanceReferenceResolution(
        parameter,
        label,
        reference,
        "Doesn't Matter, because for interface",
        synchronicityDecisionMaker);
}