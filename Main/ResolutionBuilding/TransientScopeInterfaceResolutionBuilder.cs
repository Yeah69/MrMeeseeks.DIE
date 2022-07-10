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

    private readonly IDictionary<string, (string, FunctionResolutionBuilderHandle)> _rangedInstanceReferences =
        new Dictionary<string, (string, FunctionResolutionBuilderHandle)>();

    private readonly IDictionary<string, InterfaceFunctionDeclarationResolution> _rangedInstanceReferenceResolutions =
        new Dictionary<string, InterfaceFunctionDeclarationResolution>();
    private readonly IDictionary<string, IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakers =
        new Dictionary<string, IFunctionResolutionSynchronicityDecisionMaker>();
    

    private readonly WellKnownTypes _wellKnownTypes;
    private readonly IFunctionCycleTracker _functionCycleTracker;
    private readonly Func<IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakerFactory;

    private readonly string _name;
    private readonly string _containerAdapterName;


    public TransientScopeInterfaceResolutionBuilder(
        IReferenceGeneratorFactory referenceGeneratorFactory,
        WellKnownTypes wellKnownTypes,
        IFunctionCycleTracker functionCycleTracker,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory)
    {
        _wellKnownTypes = wellKnownTypes;
        _functionCycleTracker = functionCycleTracker;
        _synchronicityDecisionMakerFactory = synchronicityDecisionMakerFactory;
        _rootReferenceGenerator = referenceGeneratorFactory.Create();

        _name = _rootReferenceGenerator.Generate("ITransientScope");

        _containerAdapterName = _rootReferenceGenerator.Generate("ContainerTransientScopeAdapter");
    }


    public void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation)
    {
        foreach (var (parameter, label, reference, key, handle) in _pastQueuedItems)
        {
            var multiSynchronicityFunctionCallResolution = implementation.EnqueueRangedInstanceResolution(
                parameter,
                label,
                reference,
                new (() => _synchronicityDecisionMakers[key]));
            _functionCycleTracker.TrackFunctionCall(handle, multiSynchronicityFunctionCallResolution.FunctionResolutionBuilderHandle);
        }

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
        if (!_rangedInstanceReferences.TryGetValue(referenceKey, out var tuple))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
            tuple = (
                _rootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix),
                new FunctionResolutionBuilderHandle(new object(), "asdf"));
            
            _rangedInstanceReferences[referenceKey] = tuple;
        }

        var (reference, handle) = tuple;

        var key = $"{referenceKey}:::{string.Join(":::", currentParameters.Select(p => p.Value.Item1.FullName()))}";
        if (!_rangedInstanceReferenceResolutions.TryGetValue(key, out var interfaceDeclaration))
        {
            var queueItem = new RangedInstanceResolutionsQueueItem(
                parameter,
                label,
                reference,
                key,
                handle);

            _pastQueuedItems.Add(queueItem);

            var synchronicityDecisionMaker = _synchronicityDecisionMakerFactory();
            _synchronicityDecisionMakers[key] = synchronicityDecisionMaker;

            foreach (var implementation in _implementations)
            {
                var multiSynchronicityFunctionCallResolution = implementation.EnqueueRangedInstanceResolution(
                    queueItem.Parameter,
                    queueItem.Label,
                    queueItem.Reference,
                    new (() => synchronicityDecisionMaker));
                _functionCycleTracker.TrackFunctionCall(handle, multiSynchronicityFunctionCallResolution.FunctionResolutionBuilderHandle);
            }

            interfaceDeclaration = new InterfaceFunctionDeclarationResolution(
                reference,
                implementationType.FullName(),
                _wellKnownTypes.Task1.Construct(implementationType).FullName(),
                _wellKnownTypes.ValueTask1.Construct(implementationType).FullName(),
                currentParameters.Select(t =>
                    new ParameterResolution(_rootReferenceGenerator.Generate(t.Value.Item1), t.Value.Item1.FullName())).ToList(),
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
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Value.Item2.Reference))
                    .ToList())
                { Await = false },
            new(returnReference,
                _wellKnownTypes.Task1.Construct(implementationType).FullName(),
                interfaceDeclaration.TypeFullName,
                interfaceDeclaration.Reference,
                owningObjectReference,
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Value.Item2.Reference))
                    .ToList()),
            new(returnReference,
                _wellKnownTypes.ValueTask1.Construct(implementationType).FullName(),
                interfaceDeclaration.TypeFullName,
                interfaceDeclaration.Reference,
                owningObjectReference,
                interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Value.Item2.Reference))
                    .ToList()),
            _synchronicityDecisionMakers[key].Decision,
            handle);
    }
}