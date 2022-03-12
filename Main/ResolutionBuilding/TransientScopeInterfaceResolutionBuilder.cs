namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeInterfaceResolutionBuilder
{
    void AddImplementation(ITransientScopeImplementationResolutionBuilder implementation);
    FunctionCallResolution CreateTransientScopeInstanceReferenceResolution(
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
    private readonly Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> _rangedFunctionGroupResolutionBuilderFactory;

    private readonly string _name;
    private readonly string _containerAdapterName;


    public TransientScopeInterfaceResolutionBuilder(
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory)
    {
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

    public FunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter, string containerReference) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "TransientScope",
            containerReference);

    public TransientScopeInterfaceResolution Build() => new(
        RangedInstanceReferenceResolutions.Values.ToList(),
        _name,
        _containerAdapterName);

    private FunctionCallResolution CreateRangedInstanceReferenceResolution(
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
        
        return new(_rootReferenceGenerator.Generate("ret"),
            interfaceDeclaration.TypeFullName,
            interfaceDeclaration.Reference,
            owningObjectReference,
            interfaceDeclaration.Parameter.Zip(currentParameters, (p, cp) => (p.Reference, cp.Resolution.Reference)).ToList());
    }
}