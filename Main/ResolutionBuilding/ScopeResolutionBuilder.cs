using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IScopeResolutionBuilder : IRangeResolutionBaseBuilder, IResolutionBuilder<ScopeResolution>
{
    ScopeRootResolution AddCreateResolveFunction(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        string transientInstanceScopeReference,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);
}

internal class ScopeResolutionBuilder : RangeResolutionBaseBuilder, IScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly IScopeManager _scopeManager;
    private readonly Func<IRangeResolutionBaseBuilder, SwitchImplementationParameter, IScopeRootCreateFunctionResolutionBuilder> _scopeRootCreateFunctionResolutionBuilderFactory;
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    private readonly string _transientScopeReference;
    private readonly string _transientScopeParameterReference;
    
    private readonly Dictionary<string, IScopeRootCreateFunctionResolutionBuilder> _scopeRootFunctionResolutions = new ();
    
    internal ScopeResolutionBuilder(
        // parameter
        string name,
        IContainerResolutionBuilder containerResolutionBuilder,
        ITransientScopeInterfaceResolutionBuilder transientScopeInterfaceResolutionBuilder,
        IScopeManager scopeManager,
        IUserDefinedElements userDefinedElements, 
        ICheckTypeProperties checkTypeProperties,
        IErrorContext errorContext,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, SwitchImplementationParameter, IScopeRootCreateFunctionResolutionBuilder> scopeRootCreateFunctionResolutionBuilderFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory) 
        : base(
            name,
            checkTypeProperties,
            userDefinedElements,
            wellKnownTypes, 
            referenceGeneratorFactory,
            rangedFunctionGroupResolutionBuilderFactory,
            synchronicityDecisionMakerFactory,
            localFunctionResolutionBuilderFactory)
    {
        ErrorContext = errorContext;
        _containerResolutionBuilder = containerResolutionBuilder;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _scopeManager = scopeManager;
        _scopeRootCreateFunctionResolutionBuilderFactory = scopeRootCreateFunctionResolutionBuilderFactory;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
        _transientScopeReference = RootReferenceGenerator.Generate("_transientScope");
        _transientScopeParameterReference = RootReferenceGenerator.Generate("transientScope");
    }

    public override IErrorContext ErrorContext { get; }

    public override MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, _transientScopeReference);

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters) =>
        _scopeManager
            .GetTransientScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType,
                _containerReference,
                currentParameters);

    public override ScopeRootResolution CreateScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType, 
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters) =>
        _scopeManager
            .GetScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType, 
                _containerReference, 
                _transientScopeReference,
                currentParameters);

    public override void RegisterDisposalType(DisposalType disposalType) => _containerResolutionBuilder.RegisterDisposalType(disposalType);

    public bool HasWorkToDo => 
        _scopeRootFunctionResolutions.Values.Any(f => f.HasWorkToDo) 
        || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo)
        || LocalFunctions.Values.Any(r => r.HasWorkToDo);

    public ScopeRootResolution AddCreateResolveFunction(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        string transientInstanceScopeReference,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{(currentParameters.Any() ? $"_{string.Join(";", currentParameters)}" : "")}";
        if (!_scopeRootFunctionResolutions.TryGetValue(
                key,
                out var function))
        {
            function = _scopeRootCreateFunctionResolutionBuilderFactory(this, parameter);
            _scopeRootFunctionResolutions[key] = function;
        }

        var scopeRootReference = RootReferenceGenerator.Generate("scopeRoot");

        return new ScopeRootResolution(
            scopeRootReference,
            Name,
            containerInstanceScopeReference,
            transientInstanceScopeReference,
            function.BuildFunctionCall(currentParameters, scopeRootReference));
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            foreach (var functionResolutionBuilder in _scopeRootFunctionResolutions.Values.Where(f => f.HasWorkToDo).ToList())
            {
                functionResolutionBuilder.DoWork();
            }

            DoRangedInstancesWork();
            DoLocalFunctionsWork();
        }
    }

    public ScopeResolution Build() =>
        new(_scopeRootFunctionResolutions
                .Values
                .Select(fr => fr.Build())
                .Select(f => new RootResolutionFunction(
                    f.Reference,
                    f.TypeFullName,
                    "internal",
                    f.Resolvable,
                    f.Parameter,
                    f.SynchronicityDecision))
                .ToList(),
            LocalFunctions
                .Values
                .Select(lf => lf.Build())
                .Select(f => new LocalFunctionResolution(
                    f.Reference,
                    f.TypeFullName,
                    f.Resolvable,
                    f.Parameter,
                    f.SynchronicityDecision))
                .ToList(),
            BuildDisposalHandling(),
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _containerReference,
            _containerParameterReference,
            _transientScopeReference,
            _transientScopeParameterReference,
            Name,
            AddForDisposal,
            AddForDisposalAsync);
}