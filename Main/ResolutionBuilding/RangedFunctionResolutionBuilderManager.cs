namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IRangedFunctionGroupResolutionBuilder
{
    IRangedFunctionResolutionBuilder GetInstanceFunction(ForConstructorParameter parameter);
    
    bool HasWorkToDo { get; }

    void DoWork();

    RangedInstanceFunctionGroupResolution Build();
}

internal class RangedFunctionGroupResolutionBuilder : IRangedFunctionGroupResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;
    private readonly Func<IRangeResolutionBaseBuilder, string, ForConstructorParameter, IRangedFunctionResolutionBuilder> _rangedFunctionResolutionBuilderFactory;
    private readonly string _reference;
    private readonly string _typeFullName;
    private readonly string _fieldReference;
    private readonly string _lockReference;

    private readonly Dictionary<string, IRangedFunctionResolutionBuilder> _overloads = new();
    private readonly Queue<IRangedFunctionResolutionBuilder> _functionQueue = new();
    private readonly List<RangedInstanceFunctionResolution> _overloadResolutions = new();

    internal RangedFunctionGroupResolutionBuilder(
        // parameter
        string label,
        string? reference,
        INamedTypeSymbol implementationType,
        string decorationSuffix,
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        
        // dependencies
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, string, ForConstructorParameter, IRangedFunctionResolutionBuilder> rangedFunctionResolutionBuilderFactory)
    {
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
        _rangedFunctionResolutionBuilderFactory = rangedFunctionResolutionBuilderFactory;
        var rootReferenceGenerator = referenceGeneratorFactory.Create();
        _reference = reference ?? rootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix);
        _typeFullName = implementationType.FullName();
        _fieldReference =
            rootReferenceGenerator.Generate($"_{label.ToLower()}InstanceField", implementationType, decorationSuffix);
        _lockReference = rootReferenceGenerator.Generate($"_{label.ToLower()}InstanceLock{decorationSuffix}");
    }

    public IRangedFunctionResolutionBuilder GetInstanceFunction(ForConstructorParameter parameter)
    {
        var listedParameterTypes = string.Join(",", parameter.CurrentFuncParameters.Select(p => p.Item2.TypeFullName));
        if (!_overloads.TryGetValue(listedParameterTypes, out var function))
        {
            function = _rangedFunctionResolutionBuilderFactory(_rangeResolutionBaseBuilder, _reference, parameter);
            _overloads[listedParameterTypes] = function;
            _functionQueue.Enqueue(function);
        }
        return function;
    }

    public bool HasWorkToDo => _functionQueue.Any();
    
    public void DoWork()
    {
        while (_functionQueue.Any())
        {
            var function = _functionQueue.Dequeue();
            var functionResolution = function.Build();
            _overloadResolutions.Add(new RangedInstanceFunctionResolution(
                functionResolution.Reference,
                functionResolution.TypeFullName,
                functionResolution.Resolvable,
                functionResolution.Parameter,
                functionResolution.DisposalHandling,
                functionResolution.LocalFunctions,
                SynchronicityDecision.Sync)); // todo async support
        }
    }

    public RangedInstanceFunctionGroupResolution Build()
    {
        return new RangedInstanceFunctionGroupResolution(
            _typeFullName,
            _overloadResolutions,
            _fieldReference,
            _lockReference);
    }
}