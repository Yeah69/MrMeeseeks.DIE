using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder : IRangeResolutionBaseBuilder, IResolutionBuilder<ContainerResolution>
{
    void AddCreateResolveFunctions(IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>)> createFunctionData);

    MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);
    
    IFunctionCycleTracker FunctionCycleTracker { get; }
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder, ITransientScopeImplementationResolutionBuilder
{
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakerFactory;

    private readonly List<(IFunctionResolutionBuilder CreateFunction, string MethodNamePrefix)> _rootResolutions = new ();
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
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, bool, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory, 
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory, 
        Func<
            IRangeResolutionBaseBuilder, 
            ITypeSymbol, 
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>, 
            string,
            ICreateFunctionResolutionBuilder> localFunctionResolutionBuilderFactory,
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
        _synchronicityDecisionMakerFactory = synchronicityDecisionMakerFactory;
        FunctionCycleTracker = functionCycleTracker;
        _scopeManager = scopeManagerFactory(this, transientScopeInterfaceResolutionBuilder);
        
        transientScopeInterfaceResolutionBuilder.AddImplementation(this);
        _transientScopeAdapterReference = RootReferenceGenerator.Generate("TransientScopeAdapter");

        ErrorContext = new ErrorContext(
            containerInfo.Name,
            containerInfo.ContainerType.Locations.FirstOrDefault() ?? Location.None);
    }

    public void AddCreateResolveFunctions(IReadOnlyList<(ITypeSymbol, string, IReadOnlyList<ITypeSymbol>)> createFunctionData)
    {
        foreach (var (typeSymbol, methodNamePrefix, parameterTypes) in createFunctionData)
        {
            var functionResolution = CreateCreateFunctionResolution(
                typeSymbol,
                ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty,
                Constants.PrivateKeyword);
            
            FunctionCycleTracker.RegisterRootHandle(functionResolution.Handle);
            
            _rootResolutions.Add((functionResolution, methodNamePrefix));
        }
    }

    public MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Container",
            null,
            containerReference,
            new (_synchronicityDecisionMakerFactory),
            false);

    public IFunctionCycleTracker FunctionCycleTracker { get; }

    public override IErrorContext ErrorContext { get; }

    public override MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        CreateContainerInstanceReferenceResolution(parameter, Constants.ThisKeyword);

    public override MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, _transientScopeAdapterReference);

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters) =>
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
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters) =>
        _scopeManager
            .GetScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                Constants.ThisKeyword,
                _transientScopeAdapterReference,
                currentParameters);

    public override void RegisterDisposalType(DisposalType disposalType) => _disposalType |= disposalType;

    public bool HasWorkToDo =>
        _rootResolutions.Any(r => r.CreateFunction.HasWorkToDo)    
        || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo)
        || CreateFunctions.Values.Any(r => r.HasWorkToDo)
        || _scopeManager.HasWorkToDo;

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            foreach ((IFunctionResolutionBuilder createFunction, _) in _rootResolutions.Where(r => r.CreateFunction.HasWorkToDo).ToList())
            {
                createFunction.DoWork();
            }
            
            DoRangedInstancesWork();
            DoCreateFunctionsWork();
            _scopeManager.DoWork();
        }
    }

    public ContainerResolution Build()
    {
        var localFunctions = CreateFunctions
            .Values
            .Select(lf => lf.Build())
            .Select(f => new CreateFunctionResolution(
                f.Reference,
                f.TypeFullName,
                f.AccessModifier,
                f.Resolvable,
                f.Parameter,
                f.SynchronicityDecision))
            .ToList();
        
        foreach (var (createFunction, methodNamePrefix) in _rootResolutions)
        {
            // Create function stays sync
            if (SymbolEqualityComparer.Default.Equals(createFunction.OriginalReturnType, createFunction.ActualReturnType))
            {
                var call = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                call.Sync.Await = false;
                call.AsyncTask.Await = false;
                call.AsyncValueTask.Await = false;
                var publicSyncResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffix}",
                    createFunction.OriginalReturnType.FullName(),
                    Constants.PublicKeyword,
                    call,
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicSyncResolutionFunction);
                
                var boundTaskTypeFullName = _wellKnownTypes
                    .Task1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var taskCall = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                taskCall.Sync.Await = false;
                taskCall.AsyncTask.Await = false;
                taskCall.AsyncValueTask.Await = false;
                var publicTaskResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffixAsync}",
                    boundTaskTypeFullName,
                    Constants.PublicKeyword,
                    new TaskFromSyncResolution(
                        taskCall, 
                        wrappedTaskReference, 
                        boundTaskTypeFullName),
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicTaskResolutionFunction);
                
                var boundValueTaskTypeFullName = _wellKnownTypes
                    .ValueTask1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var valueTaskCall = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                valueTaskCall.Sync.Await = false;
                valueTaskCall.AsyncTask.Await = false;
                valueTaskCall.AsyncValueTask.Await = false;
                var publicValueTaskResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffixValueAsync}",
                    boundValueTaskTypeFullName,
                    Constants.PublicKeyword,
                    new ValueTaskFromSyncResolution(
                        valueTaskCall, 
                        wrappedValueTaskReference, 
                        boundValueTaskTypeFullName),
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicValueTaskResolutionFunction);
            }
            else if (createFunction.ActualReturnType is { } actual
                     && SymbolEqualityComparer.Default.Equals(actual, _wellKnownTypes.Task1.Construct(createFunction.OriginalReturnType)))
            {
                var call = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                call.Sync.Await = false;
                call.AsyncTask.Await = false;
                call.AsyncValueTask.Await = false;
                var publicTaskResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffixAsync}",
                    actual.FullName(),
                    Constants.PublicKeyword,
                    call,
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicTaskResolutionFunction);
                
                var boundValueTaskTypeFullName = _wellKnownTypes
                    .ValueTask1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var valueCall = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                valueCall.Sync.Await = false;
                valueCall.AsyncTask.Await = false;
                valueCall.AsyncValueTask.Await = false;
                var publicValueTaskResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffixValueAsync}",
                    boundValueTaskTypeFullName,
                    Constants.PublicKeyword,
                    new ValueTaskFromWrappedTaskResolution(
                        valueCall, 
                        wrappedValueTaskReference, 
                        boundValueTaskTypeFullName),
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicValueTaskResolutionFunction);
            }
            else if (createFunction.ActualReturnType is { } actual1
                     && SymbolEqualityComparer.Default.Equals(actual1, _wellKnownTypes.ValueTask1.Construct(createFunction.OriginalReturnType)))
            {
                var boundTaskTypeFullName = _wellKnownTypes
                    .Task1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var call = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                call.Sync.Await = false;
                call.AsyncTask.Await = false;
                call.AsyncValueTask.Await = false;
                var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.ValueTask);
                var publicTaskResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffixAsync}",
                    boundTaskTypeFullName,
                    Constants.PublicKeyword,
                    new TaskFromWrappedValueTaskResolution(
                        call, 
                        wrappedTaskReference, 
                        boundTaskTypeFullName),
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicTaskResolutionFunction);
                
                var valueCall = createFunction.BuildFunctionCall(
                    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>.Empty, 
                    Constants.ThisKeyword);
                valueCall.Sync.Await = false;
                valueCall.AsyncTask.Await = false;
                valueCall.AsyncValueTask.Await = false;
                var publicValueTaskResolutionFunction = new CreateFunctionResolution(
                    $"{methodNamePrefix}{Constants.CreateFunctionSuffixValueAsync}",
                    actual1.FullName(),
                    Constants.PublicKeyword,
                    valueCall,
                    createFunction.Parameters,
                    SynchronicityDecision.Sync);
            
                localFunctions.Add(publicValueTaskResolutionFunction);
            }
        }
        
        var (transientScopeResolutions, scopeResolutions) = _scopeManager.Build();

        return new(
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
        synchronicityDecisionMaker,
        true);
}