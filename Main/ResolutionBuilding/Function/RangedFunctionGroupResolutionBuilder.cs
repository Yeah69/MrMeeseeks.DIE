using MrMeeseeks.DIE.Utility;

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
    private readonly bool _isTransientScopeInstance;

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
    private readonly string? _isCreatedForStructs;

    private readonly Dictionary<string, IRangedFunctionResolutionBuilder> _overloads = new();
    private readonly List<IRangedFunctionResolutionBuilder> _functionQueue = new();

    internal RangedFunctionGroupResolutionBuilder(
        // parameter
        string label,
        string? reference,
        INamedTypeSymbol implementationType,
        string decorationSuffix,
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        bool isTransientScopeInstance,
        
        // dependencies
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<
            IRangeResolutionBaseBuilder, 
            string, 
            ForConstructorParameter, 
            IFunctionResolutionSynchronicityDecisionMaker, 
            object, 
            IRangedFunctionResolutionBuilder> rangedFunctionResolutionBuilderFactory)
    {
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
        _isTransientScopeInstance = isTransientScopeInstance;
        _rangedFunctionResolutionBuilderFactory = rangedFunctionResolutionBuilderFactory;
        var rootReferenceGenerator = referenceGeneratorFactory.Create();
        _reference = reference ?? rootReferenceGenerator.Generate($"Get{label}Instance", implementationType, decorationSuffix);
        _typeFullName = implementationType.FullName();
        _fieldReference =
            rootReferenceGenerator.Generate($"_{label.ToLower()}InstanceField", implementationType, decorationSuffix);
        _lockReference = rootReferenceGenerator.Generate($"_{label.ToLower()}InstanceLock{decorationSuffix}");
        _isCreatedForStructs = implementationType.IsValueType ? rootReferenceGenerator.Generate("isCreated") : null;
    }

    public IRangedFunctionResolutionBuilder GetInstanceFunction(
        ForConstructorParameter parameter,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker) =>
        FunctionResolutionUtility.GetOrCreateFunction(
            _overloads, 
            parameter.ImplementationType, 
            parameter.CurrentParameters,
            () =>
            {
                var newFunction = _rangedFunctionResolutionBuilderFactory(
                    _rangeResolutionBaseBuilder,
                    _reference,
                    parameter,
                    synchronicityDecisionMaker.Value,
                    this);
                _functionQueue.Add(newFunction);
                return newFunction;
            });

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
            _lockReference,
            _isCreatedForStructs,
            _isTransientScopeInstance);
}