using MrMeeseeks.DIE.ResolutionBuilding.Function;

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

    private readonly IDictionary<string, string> _rangedInstanceReferences =
        new Dictionary<string, string>();

    private readonly IDictionary<string, InterfaceFunctionDeclarationResolution> _rangedInstanceReferenceResolutions =
        new Dictionary<string, InterfaceFunctionDeclarationResolution>();
    private readonly IDictionary<string, IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakers =
        new Dictionary<string, IFunctionResolutionSynchronicityDecisionMaker>();
    

    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> _rangedFunctionGroupResolutionBuilderFactory;
    private readonly Func<IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakerFactory;

    private readonly string _name;
    private readonly string _containerAdapterName;


    public TransientScopeInterfaceResolutionBuilder(
        IReferenceGeneratorFactory referenceGeneratorFactory,
        WellKnownTypes wellKnownTypes,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory)
    {
        _wellKnownTypes = wellKnownTypes;
        _rangedFunctionGroupResolutionBuilderFactory = rangedFunctionGroupResolutionBuilderFactory;
        _synchronicityDecisionMakerFactory = synchronicityDecisionMakerFactory;
        _rootReferenceGenerator = referenceGeneratorFactory.Create();

        _name = _rootReferenceGenerator.Generate("ITransientScope");

        _containerAdapterName = _rootReferenceGenerator.Generate("ContainerTransientScopeAdapter");
    }


    public void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation)
    {
        foreach (var (parameter, label, reference, key) in _pastQueuedItems)
            implementation.EnqueueRangedInstanceResolution(
                parameter,
                label,
                reference,
                new (() => _synchronicityDecisionMakers[key]));

        _implementations.Add(implementation);
    }

    public MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "TransientScope",
            containerReference);

    public TransientScopeInterfaceResolution Build() => new(
        _rangedInstanceReferenceResolutions.Values.ToList(),
        _name,
        _containerAdapterName);

    private MultiSynchronicityFunctionCallResolution CreateRangedInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string label,
        string owningObjectReference)
    {
        var (implementationType, currentParameters, _) = parameter;
        InterfaceExtension? interfaceExtension = parameter switch
        {
            ForConstructorParameterWithComposition withComposition => withComposition.Composition,
            ForConstructorParameterWithDecoration withDecoration => withDecoration.Decoration,
            _ => null
        };
        var referenceKey = $"{implementationType.FullName()}{interfaceExtension?.KeySuffix() ?? ""}";
        if (!_rangedInstanceReferences.TryGetValue(referenceKey, out var reference))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
            reference =
                _rootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix);
            _rangedInstanceReferences[referenceKey] = reference;
        }
        
        var key = $"{referenceKey}:::{string.Join(":::", currentParameters.Select(p => p.Type.FullName()))}";
        if (!_rangedInstanceReferenceResolutions.TryGetValue(key, out var interfaceDeclaration))
        {
            var queueItem = new RangedInstanceResolutionsQueueItem(
                parameter,
                label,
                reference,
                key);

            _pastQueuedItems.Add(queueItem);

            var synchronicityDecisionMaker = _synchronicityDecisionMakerFactory();
            _synchronicityDecisionMakers[key] = synchronicityDecisionMaker;

            foreach (var implementation in _implementations)
                implementation.EnqueueRangedInstanceResolution(
                    queueItem.Parameter,
                    queueItem.Label,
                    queueItem.Reference,
                    new (() => synchronicityDecisionMaker));

            interfaceDeclaration = new InterfaceFunctionDeclarationResolution(
                reference,
                implementationType.FullName(),
                _wellKnownTypes.Task1.Construct(implementationType).FullName(),
                _wellKnownTypes.ValueTask1.Construct(implementationType).FullName(),
                currentParameters.Select(t =>
                    new ParameterResolution(_rootReferenceGenerator.Generate(t.Type), t.Type.FullName())).ToList(),
                synchronicityDecisionMaker.Decision);
            _rangedInstanceReferenceResolutions[key] = interfaceDeclaration;
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
                _wellKnownTypes.ValueTask1.Construct(implementationType).FullName(),
                interfaceDeclaration.TypeFullName,
                interfaceDeclaration.Reference,
                owningObjectReference,
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference))
                    .ToList()),
            _synchronicityDecisionMakers[key].Decision);
    }
}