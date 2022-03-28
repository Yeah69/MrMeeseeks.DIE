namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeInterfaceResolutionBuilder
{
    void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation);
    MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);

    TransientScopeInterfaceResolution Build();
}

internal class TransientScopeInterfaceResolutionBuilder : ITransientScopeInterfaceResolutionBuilder
{
    private readonly IReferenceGenerator _rootReferenceGenerator;

    private readonly HashSet<RangedInstanceResolutionsQueueItem> _pastQueuedItems = new();
    private readonly IList<ITransientScopeImplementationResolutionBuilder> _implementations =
        new List<ITransientScopeImplementationResolutionBuilder>();
    protected readonly IDictionary<string, InterfaceFunctionDeclarationResolution> RangedInstanceReferenceResolutions =
        new Dictionary<string, InterfaceFunctionDeclarationResolution>();

    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> _rangedFunctionGroupResolutionBuilderFactory;

    private readonly string _name;
    private readonly string _containerAdapterName;


    public TransientScopeInterfaceResolutionBuilder(
        IReferenceGeneratorFactory referenceGeneratorFactory,
        WellKnownTypes wellKnownTypes,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory)
    {
        _wellKnownTypes = wellKnownTypes;
        _rangedFunctionGroupResolutionBuilderFactory = rangedFunctionGroupResolutionBuilderFactory;
        _rootReferenceGenerator = referenceGeneratorFactory.Create();

        _name = _rootReferenceGenerator.Generate("ITransientScope");

        _containerAdapterName = _rootReferenceGenerator.Generate("ContainerTransientScopeAdapter");
    }


    public void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation)
    {
        foreach (var (parameter, label, reference) in _pastQueuedItems)
            implementation.EnqueueRangedInstanceResolution(
                parameter,
                label,
                reference);

        _implementations.Add(implementation);
    }

    public MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "TransientScope",
            containerReference);

    public TransientScopeInterfaceResolution Build() => new(
        RangedInstanceReferenceResolutions.Values.ToList(),
        _name,
        _containerAdapterName);

    private MultiSynchronicityFunctionCallResolution CreateRangedInstanceReferenceResolution(
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
        if (!RangedInstanceReferenceResolutions.TryGetValue(key, out var interfaceDeclaration))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
            var reference = _rootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix);
            var queueItem = new RangedInstanceResolutionsQueueItem(
                parameter,
                label,
                reference);

            _pastQueuedItems.Add(queueItem);
            
            foreach (var implementation in _implementations)
                implementation.EnqueueRangedInstanceResolution(
                    queueItem.Parameter,
                    queueItem.Label,
                    queueItem.Reference);

            interfaceDeclaration = new InterfaceFunctionDeclarationResolution(
                reference,
                implementationType.FullName(),
                currentParameters.Select(t =>
                    new ParameterResolution(_rootReferenceGenerator.Generate(t.Type), t.Type.FullName())).ToList());
            RangedInstanceReferenceResolutions[key] = interfaceDeclaration;
        }

        var returnReference = _rootReferenceGenerator.Generate("ret");

        return new(
            new(returnReference,
                interfaceDeclaration.TypeFullName,
                interfaceDeclaration.TypeFullName,
                interfaceDeclaration.Reference,
                owningObjectReference,
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference))
                    .ToList())
                { Await = false },
            new(returnReference,
                _wellKnownTypes.Task1.Construct(implementationType).FullName(),
                interfaceDeclaration.TypeFullName,
                interfaceDeclaration.Reference,
                owningObjectReference,
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference))
                    .ToList()),
            new(returnReference,
                interfaceDeclaration.TypeFullName,
                _wellKnownTypes.ValueTask1.Construct(implementationType).FullName(),
                interfaceDeclaration.Reference,
                owningObjectReference,
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference))
                    .ToList()),
            new Lazy<SynchronicityDecision>(() => SynchronicityDecision.Sync)); // todo solve situation with transient scope instance interface
    }
}