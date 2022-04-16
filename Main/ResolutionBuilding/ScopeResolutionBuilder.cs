using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IScopeResolutionBuilder : IRangeResolutionBaseBuilder, IResolutionBuilder<ScopeResolution>
{
    ScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        string transientInstanceScopeReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
}

internal class ScopeResolutionBuilder : RangeResolutionBaseBuilder, IScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly IScopeManager _scopeManager;
    private readonly Func<IRangeResolutionBaseBuilder, IScopeRootParameter, IScopeRootCreateFunctionResolutionBuilder> _scopeRootCreateFunctionResolutionBuilderFactory;
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
        IUserProvidedScopeElements userProvidedScopeElements, 
        ICheckTypeProperties checkTypeProperties,
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<IRangeResolutionBaseBuilder, IScopeRootParameter, IScopeRootCreateFunctionResolutionBuilder> scopeRootCreateFunctionResolutionBuilderFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory) 
        : base(
            name,
            checkTypeProperties,
            userProvidedScopeElements,
            wellKnownTypes, 
            referenceGeneratorFactory,
            rangedFunctionGroupResolutionBuilderFactory,
            synchronicityDecisionMakerFactory)
    {
        _containerResolutionBuilder = containerResolutionBuilder;
        _transientScopeInterfaceResolutionBuilder = transientScopeInterfaceResolutionBuilder;
        _scopeManager = scopeManager;
        _scopeRootCreateFunctionResolutionBuilderFactory = scopeRootCreateFunctionResolutionBuilderFactory;
        _containerReference = RootReferenceGenerator.Generate("_container");
        _containerParameterReference = RootReferenceGenerator.Generate("container");
        _transientScopeReference = RootReferenceGenerator.Generate("_transientScope");
        _transientScopeParameterReference = RootReferenceGenerator.Generate("transientScope");
    }

    public override MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, _transientScopeReference);

    public override TransientScopeRootResolution CreateTransientScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
        _scopeManager
            .GetTransientScopeBuilder(rootType)
            .AddCreateResolveFunction(
                parameter,
                rootType,
                _containerReference,
                currentParameters);

    public override ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType, 
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters) =>
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
        || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo);

    public ScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        string transientInstanceScopeReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{parameter.KeySuffix()}";
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
            foreach (var functionResolutionBuilder in _scopeRootFunctionResolutions.Values.Where(f => f.HasWorkToDo))
            {
                functionResolutionBuilder.DoWork();
            }

            DoRangedInstancesWork();
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
                    f.LocalFunctions,
                    f.SynchronicityDecision))
                .ToList(),
            BuildDisposalHandling(),
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _containerReference,
            _containerParameterReference,
            _transientScopeReference,
            _transientScopeParameterReference,
            Name);
}