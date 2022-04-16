using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeResolutionBuilder : ITransientScopeImplementationResolutionBuilder, IRangeResolutionBaseBuilder, IResolutionBuilder<TransientScopeResolution>
{
    TransientScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
}

internal class TransientScopeResolutionBuilder : RangeResolutionBaseBuilder, ITransientScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly IScopeManager _scopeManager;
    private readonly Func<IRangeResolutionBaseBuilder, IScopeRootParameter, IScopeRootCreateFunctionResolutionBuilder> _scopeRootCreateFunctionResolutionBuilderFactory;
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly Dictionary<string, IScopeRootCreateFunctionResolutionBuilder> _transientScopeRootFunctionResolutions = new ();

    internal TransientScopeResolutionBuilder(
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
    }

    public override MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, "this");

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
                "this",
                currentParameters);

    public override void RegisterDisposalType(DisposalType disposalType) => _containerResolutionBuilder.RegisterDisposalType(disposalType);

    public bool HasWorkToDo => 
        _transientScopeRootFunctionResolutions.Values.Any(f => f.HasWorkToDo) 
        || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo);

    public TransientScopeRootResolution AddCreateResolveFunction(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{parameter.KeySuffix()}";
        if (!_transientScopeRootFunctionResolutions.TryGetValue(
                key,
                out var function))
        {
            function = _scopeRootCreateFunctionResolutionBuilderFactory(this, parameter);
            _transientScopeRootFunctionResolutions[key] = function;
        }

        var transientScopeRootReference = RootReferenceGenerator.Generate("transientScopeRoot");

        return new TransientScopeRootResolution(
            transientScopeRootReference,
            Name,
            containerInstanceScopeReference,
            function.BuildFunctionCall(currentParameters, transientScopeRootReference));
    }

    public void DoWork()
    {
        while (HasWorkToDo)
        {
            foreach (var functionResolutionBuilder in _transientScopeRootFunctionResolutions.Values.Where(f => f.HasWorkToDo).ToList())
            {
                functionResolutionBuilder.DoWork();
            }

            DoRangedInstancesWork();
        }
    }

    public TransientScopeResolution Build() =>
        new(_transientScopeRootFunctionResolutions
                .Values
                .Select(f => f.Build())
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
            Name);

    public void EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker) => CreateRangedInstanceReferenceResolution(
        parameter,
        label,
        reference,
        "Doesn't Matter, because for interface",
        synchronicityDecisionMaker);
}