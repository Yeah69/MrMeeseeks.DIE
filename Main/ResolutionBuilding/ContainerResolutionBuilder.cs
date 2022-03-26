using System.Threading.Tasks;
using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IContainerResolutionBuilder : IRangeResolutionBaseBuilder
{
    void AddCreateResolveFunctions(IReadOnlyList<(INamedTypeSymbol, string)> createFunctionData);

    FunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);

    ContainerResolution Build();
}

internal class ContainerResolutionBuilder : RangeResolutionBaseBuilder, IContainerResolutionBuilder, ITransientScopeImplementationResolutionBuilder
{
    private readonly IContainerInfo _containerInfo;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IContainerCreateFunctionResolutionBuilder> _createFunctionResolutionBuilderFactory;

    private readonly List<(IContainerCreateFunctionResolutionBuilder CreateFunction, string MethodNamePrefix)> _rootResolutions = new ();
    private readonly string _transientScopeAdapterReference;
    private readonly IScopeManager _scopeManager;

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
        IUserProvidedScopeElements userProvidedScopeElement) 
        : base(
            containerInfo.Name, 
            checkTypeProperties,
            userProvidedScopeElement,
            wellKnownTypes, 
            referenceGeneratorFactory,
            rangedFunctionGroupResolutionBuilderFactory)
    {
        _containerInfo = containerInfo;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _wellKnownTypes = wellKnownTypes;
        _createFunctionResolutionBuilderFactory = createFunctionResolutionBuilderFactory;
        _scopeManager = scopeManagerFactory(this, transientScopeInterfaceResolutionBuilder);
        
        transientScopeInterfaceResolutionBuilder.AddImplementation(this);
        _transientScopeAdapterReference = RootReferenceGenerator.Generate("TransientScopeAdapter");
    }

    public void AddCreateResolveFunctions(IReadOnlyList<(INamedTypeSymbol, string)> createFunctionData)
    {
        foreach (var (typeSymbol, methodNamePrefix) in createFunctionData)
            _rootResolutions.Add((_createFunctionResolutionBuilderFactory(this, typeSymbol), methodNamePrefix));
    }

    public FunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Container",
            null,
            containerReference);

    public override FunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        CreateContainerInstanceReferenceResolution(parameter, "this");

    public override FunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, "this");

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(IScopeRootParameter parameter, INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetTransientScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                "this",
                disposableCollectionResolution,
                currentParameters);

    public override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                "this",
                _transientScopeAdapterReference,
                disposableCollectionResolution,
                currentParameters);

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
                DisposalHandling,
                privateFunctionResolution.LocalFunctions,
                privateFunctionResolution.IsAsync);
            
            rootFunctions.Add(privateRootResolutionFunction);

            // Create function stays sync
            if (createFunction.OriginalReturnType.Equals(
                    createFunction.ActualReturnType,
                    SymbolEqualityComparer.Default))
            {
                var publicSyncResolutionFunction = new RootResolutionFunction(
                    methodNamePrefix,
                    privateRootResolutionFunction.TypeFullName,
                    "public",
                    createFunction.BuildFunctionCall(
                        Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                        "this"),
                    privateRootResolutionFunction.Parameter,
                    DisposalHandling,
                    Array.Empty<LocalFunctionResolution>(),
                    false);
            
                rootFunctions.Add(publicSyncResolutionFunction);
                
                var boundTaskTypeFullName = _wellKnownTypes
                    .Task1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var publicTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}Async",
                    boundTaskTypeFullName,
                    "public",
                    new TaskFromSyncResolution(
                        createFunction.BuildFunctionCall(
                            Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                            "this"), 
                        wrappedTaskReference, 
                        boundTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    DisposalHandling,
                    Array.Empty<LocalFunctionResolution>(),
                    false);
            
                rootFunctions.Add(publicTaskResolutionFunction);
                
                var boundValueTaskTypeFullName = _wellKnownTypes
                    .ValueTask1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var publicValueTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}ValueAsync",
                    boundValueTaskTypeFullName,
                    "public",
                    new ValueTaskFromSyncResolution(
                        createFunction.BuildFunctionCall(
                            Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                            "this"), 
                        wrappedValueTaskReference, 
                        boundValueTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    DisposalHandling,
                    Array.Empty<LocalFunctionResolution>(),
                    false);
            
                rootFunctions.Add(publicValueTaskResolutionFunction);
            }
            else if (createFunction.ActualReturnType is { } actual
                     && actual.Equals(_wellKnownTypes.Task1.Construct(createFunction.OriginalReturnType),
                         SymbolEqualityComparer.Default))
            {
                var wrappedTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var publicTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}Async",
                    actual.FullName(),
                    "public",
                    createFunction.BuildFunctionCall(
                        Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                        "this"),
                    privateRootResolutionFunction.Parameter,
                    DisposalHandling,
                    Array.Empty<LocalFunctionResolution>(),
                    false);
            
                rootFunctions.Add(publicTaskResolutionFunction);
                
                var boundValueTaskTypeFullName = _wellKnownTypes
                    .ValueTask1
                    .Construct(createFunction.OriginalReturnType)
                    .FullName();
                var wrappedValueTaskReference = RootReferenceGenerator.Generate(_wellKnownTypes.Task);
                var publicValueTaskResolutionFunction = new RootResolutionFunction(
                    $"{methodNamePrefix}ValueAsync",
                    boundValueTaskTypeFullName,
                    "public",
                    new ValueTaskFromWrappedTaskResolution(
                        createFunction.BuildFunctionCall(
                            Array.Empty<(ITypeSymbol Type, ParameterResolution Resolution)>(),
                            "this"), 
                        wrappedValueTaskReference, 
                        boundValueTaskTypeFullName),
                    privateRootResolutionFunction.Parameter,
                    DisposalHandling,
                    Array.Empty<LocalFunctionResolution>(),
                    false);
            
                rootFunctions.Add(publicValueTaskResolutionFunction);
            }
        }

        while (RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo)
               || _scopeManager.HasWorkToDo)
        {
            DoRangedInstancesWork();
            _scopeManager.DoWork();
        }
        
        var (transientScopeResolutions, scopeResolutions) = _scopeManager.Build();

        return new(
            rootFunctions,
            DisposalHandling,
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _transientScopeInterfaceResolutionBuilder.Build(),
            _transientScopeAdapterReference,
            transientScopeResolutions,
            scopeResolutions);
    }

    public void EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference) => CreateRangedInstanceReferenceResolution(
        parameter,
        label,
        reference,
        "Doesn'tMatter");
}