namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IRangedFunctionGroupResolutionBuilder
{
    IRangedFunctionResolutionBuilder GetInstanceFunction(ForConstructorParameter parameter, Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker);
    
    bool HasWorkToDo { get; }

    void DoWork();

    RangedInstanceFunctionGroupResolution Build();
}

internal class RangedFunctionGroupResolutionBuilder : IRangedFunctionGroupResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;

    private readonly Func<
        IRangeResolutionBaseBuilder, 
        string, 
        ForConstructorParameter, 
        IFunctionResolutionSynchronicityDecisionMaker, 
        object,
        IRangedFunctionResolutionBuilder> _rangedFunctionResolutionBuilderFactory;
    private readonly string _reference;
    private readonly string _typeFullName;
    private readonly string _fieldReference;
    private readonly string _lockReference;

    private readonly Dictionary<string, IRangedFunctionResolutionBuilder> _overloads = new();
    private readonly List<IRangedFunctionResolutionBuilder> _functionQueue = new();

    internal RangedFunctionGroupResolutionBuilder(
        // parameter
        string label,
        string? reference,
        INamedTypeSymbol implementationType,
        string decorationSuffix,
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        
        // dependencies
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, string, ForConstructorParameter, IFunctionResolutionSynchronicityDecisionMaker, object, IRangedFunctionResolutionBuilder> rangedFunctionResolutionBuilderFactory)
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

    public IRangedFunctionResolutionBuilder GetInstanceFunction(
        ForConstructorParameter parameter,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker)
    {
        var listedParameterTypes = string.Join(",", parameter.CurrentParameters.Select(p => p.Value.Item2.TypeFullName));
        if (!_overloads.TryGetValue(listedParameterTypes, out var function))
        {
            function = _rangedFunctionResolutionBuilderFactory(
                _rangeResolutionBaseBuilder, 
                _reference, 
                parameter, 
                synchronicityDecisionMaker.Value,
                this);
            _overloads[listedParameterTypes] = function;
            _functionQueue.Add(function);
        }
        return function;
    }

    public bool HasWorkToDo => _functionQueue.Any(f => f.HasWorkToDo);
    
    public void DoWork()
    {
        while (_functionQueue.Any(f => f.HasWorkToDo))
            foreach (var function in _functionQueue.Where(f => f.HasWorkToDo).ToList())
                function.DoWork();
    }

    public RangedInstanceFunctionGroupResolution Build() =>
        new(_typeFullName,
            _functionQueue
                .Select(function => function.Build())
                .Select(functionResolution => new RangedInstanceFunctionResolution(
                    functionResolution.Reference, 
                    functionResolution.TypeFullName, 
                    functionResolution.Resolvable,
                    functionResolution.Parameter, 
                    functionResolution.SynchronicityDecision))
                .ToList(),
            _fieldReference,
            _lockReference);
}