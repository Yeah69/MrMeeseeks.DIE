namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeInterfaceResolutionBuilder
{
    void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation);
    RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string containerReference);

    TransientScopeInterfaceResolution Build();
}

internal class TransientScopeInterfaceResolutionBuilder : ITransientScopeInterfaceResolutionBuilder
{
    private readonly IReferenceGenerator _rootReferenceGenerator;
    private readonly IDictionary<string, RangedInstanceFunction> _rangedInstanceReferenceResolutions =
        new Dictionary<string, RangedInstanceFunction>();
    private readonly HashSet<(RangedInstanceFunction, string)> _rangedInstanceQueuedOverloads = new ();

    private readonly HashSet<RangedInstanceResolutionsQueueItem> _pastQueuedItems = new();
    private readonly IList<ITransientScopeImplementationResolutionBuilder> _implementations =
        new List<ITransientScopeImplementationResolutionBuilder>();

    private readonly string _name;
    private readonly string _containerAdapterName;


    public TransientScopeInterfaceResolutionBuilder(
        IReferenceGeneratorFactory referenceGeneratorFactory)
    {
        _rootReferenceGenerator = referenceGeneratorFactory.Create();

        _name = _rootReferenceGenerator.Generate("ITransientScope");

        _containerAdapterName = _rootReferenceGenerator.Generate("ContainerTransientScopeAdapter");
    }


    public void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation)
    {
        foreach (var item in _pastQueuedItems)
            implementation.EnqueueRangedInstanceResolution(item);

        _implementations.Add(implementation);
    }

    public RangedInstanceReferenceResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "TransientScope",
            containerReference);

    public TransientScopeInterfaceResolution Build() => new(
        _pastQueuedItems
            .Select(i => new TransientScopeInstanceInterfaceFunction(
                i.Parameters.Select(p => new ParameterResolution(p.Item2.Reference, p.Item2.TypeFullName)).ToList(),
                i.Function.Reference,
                i.Function.TypeFullName))
            .ToList(),
        _name,
        _containerAdapterName);

    private RangedInstanceReferenceResolution CreateRangedInstanceReferenceResolution(
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
        if (!_rangedInstanceReferenceResolutions.TryGetValue(
                key,
                out RangedInstanceFunction function))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
            function = new RangedInstanceFunction(
                _rootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix),
                implementationType.FullName(),
                _rootReferenceGenerator.Generate($"_{label.ToLower()}InstanceField", implementationType, decorationSuffix),
                _rootReferenceGenerator.Generate($"_{label.ToLower()}InstanceLock{decorationSuffix}"));
            _rangedInstanceReferenceResolutions[key] = function;
        }

        var listedParameterTypes = string.Join(",", currentParameters.Select(p => p.Item2.TypeFullName));
        if (!_rangedInstanceQueuedOverloads.Contains((function, listedParameterTypes)))
        {
            var tempParameter = currentParameters
                .Select(t => (t.Type, new ParameterResolution(_rootReferenceGenerator.Generate(t.Type), t.Type.FullName())))
                .ToList();
            var queueItem = new RangedInstanceResolutionsQueueItem(function, tempParameter, implementationType, interfaceExtension);
            foreach (var implementation in _implementations)
                implementation.EnqueueRangedInstanceResolution(queueItem);
            _pastQueuedItems.Add(queueItem);
            _rangedInstanceQueuedOverloads.Add((function, listedParameterTypes));
        }

        return new RangedInstanceReferenceResolution(
            _rootReferenceGenerator.Generate(implementationType),
            function,
            currentParameters.Select(t => t.Resolution).ToList(),
            owningObjectReference);
    }
}