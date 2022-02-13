using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IRangeResolutionBaseBuilder
{
    ICheckTypeProperties CheckTypeProperties { get; }
    IUserProvidedScopeElements UserProvidedScopeElements { get; }
    DisposableCollectionResolution DisposableCollectionResolution { get; }
    
    RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter);

    RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);

    RangedInstanceReferenceResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);
    
    TransientScopeRootResolution CreateTransientScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
}

internal abstract class RangeResolutionBaseBuilder : IRangeResolutionBaseBuilder
{
    public ICheckTypeProperties CheckTypeProperties { get; }
    public IUserProvidedScopeElements UserProvidedScopeElements { get; }
    public DisposableCollectionResolution DisposableCollectionResolution { get; }

    protected readonly WellKnownTypes WellKnownTypes;
    private readonly Func<IRangeResolutionBaseBuilder, IFunctionResolutionBuilder> _functionResolutionBuilderFactory;

    protected readonly IReferenceGenerator RootReferenceGenerator;
    protected readonly IDictionary<string, RangedInstanceFunction> RangedInstanceReferenceResolutions =
        new Dictionary<string, RangedInstanceFunction>();
    protected readonly HashSet<(RangedInstanceFunction, string)> RangedInstanceQueuedOverloads = new ();
    protected readonly Queue<RangedInstanceResolutionsQueueItem> RangedInstanceResolutionsQueue = new();
    
    protected readonly List<(RangedInstanceFunction, RangedInstanceFunctionOverload)> RangedInstances = new ();
    protected readonly DisposalHandling DisposalHandling;
    protected readonly string Name;

    protected RangeResolutionBaseBuilder(
        // parameters
        string name,
        ICheckTypeProperties checkTypeProperties,
        IUserProvidedScopeElements userProvidedScopeElements,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, IFunctionResolutionBuilder> functionResolutionBuilderFactory)
    {
        CheckTypeProperties = checkTypeProperties;
        UserProvidedScopeElements = userProvidedScopeElements;
        WellKnownTypes = wellKnownTypes;
        _functionResolutionBuilderFactory = functionResolutionBuilderFactory;

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        DisposableCollectionResolution = new DisposableCollectionResolution(
            RootReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfDisposable),
            WellKnownTypes.ConcurrentBagOfDisposable.FullName());
        
        Name = name;
        DisposalHandling = new DisposalHandling(
            DisposableCollectionResolution,
            Name,
            RootReferenceGenerator.Generate("_disposed"),
            RootReferenceGenerator.Generate("disposed"),
            RootReferenceGenerator.Generate("Disposed"),
            RootReferenceGenerator.Generate("disposable"));
    }

    public abstract RangedInstanceReferenceResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter);

    public abstract RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter);

    public RangedInstanceReferenceResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Scope",
            "this");
    
    public abstract TransientScopeRootResolution CreateTransientScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    public abstract ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    protected RangedInstanceReferenceResolution CreateRangedInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string label,
        string owningObjectReference)
    {
        var (implementationType, currentParameters) = parameter;
        InterfaceExtension? interfaceExtension = parameter switch
        {
            ForConstructorParameterWithComposition withComposition => withComposition.Composition,
            ForConstructorParameterWithDecoration withDecoration => withDecoration.Decoration,
            _ => null
        };
        var key = $"{implementationType.FullName()}{interfaceExtension?.KeySuffix() ?? ""}";
        if (!RangedInstanceReferenceResolutions.TryGetValue(
                key,
                out RangedInstanceFunction function))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
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
            var tempParameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(RootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            RangedInstanceResolutionsQueue.Enqueue(new RangedInstanceResolutionsQueueItem(function, tempParameter, implementationType, interfaceExtension));
            RangedInstanceQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new RangedInstanceReferenceResolution(
            RootReferenceGenerator.Generate(implementationType),
            function,
            currentParameters.Select(t => t.Resolution).ToList(),
            owningObjectReference);
    }

    protected void DoRangedInstancesWork()
    {
        while (RangedInstanceResolutionsQueue.Any())
        {
            var (scopeInstanceFunction, parameter, type, interfaceExtension) = RangedInstanceResolutionsQueue.Dequeue();
            var resolvable = interfaceExtension switch
            {
                DecorationInterfaceExtension decoration => _functionResolutionBuilderFactory(this).RangedFunction(new ForConstructorParameterWithDecoration(
                    decoration.DecoratorType, parameter, decoration)),
                CompositionInterfaceExtension composition => _functionResolutionBuilderFactory(this).RangedFunction(new ForConstructorParameterWithComposition(
                    composition.CompositeType, parameter, composition)),
                _ => _functionResolutionBuilderFactory(this).RangedFunction(new ForConstructorParameter(type, parameter))
            };
            RangedInstances.Add((
                scopeInstanceFunction, 
                new RangedInstanceFunctionOverload(
                    resolvable, 
                    parameter.Select(t => t.Item2).ToList())));
        }
    }
}