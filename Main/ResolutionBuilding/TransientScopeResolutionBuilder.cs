using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeResolutionBuilder : ITransientScopeImplementationResolutionBuilder, IRangeResolutionBaseBuilder, IResolutionBuilder<TransientScopeResolution>
{
    TransientScopeRootResolution AddCreateResolveFunction(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);
}

internal class TransientScopeResolutionBuilder : RangeResolutionBaseBuilder, ITransientScopeResolutionBuilder
{
    private readonly IContainerResolutionBuilder _containerResolutionBuilder;
    private readonly ITransientScopeInterfaceResolutionBuilder _transientScopeInterfaceResolutionBuilder;
    private readonly IScopeManager _scopeManager;
    private readonly Func<IRangeResolutionBaseBuilder, SwitchImplementationParameter, IScopeRootCreateFunctionResolutionBuilder> _scopeRootCreateFunctionResolutionBuilderFactory;
    private readonly string _containerReference;
    private readonly string _containerParameterReference;
    
    private readonly Dictionary<string, IScopeRootCreateFunctionResolutionBuilder> _transientScopeRootFunctionResolutions = new ();

    internal TransientScopeResolutionBuilder(
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
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, bool, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory, 
        Func<
            IRangeResolutionBaseBuilder, 
            INamedTypeSymbol, 
            ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>, 
            string,
            ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory) 
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
    }

    public override IErrorContext ErrorContext { get; }

    public override MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _containerResolutionBuilder.CreateContainerInstanceReferenceResolution(parameter, _containerReference);

    public override MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter) =>
        _transientScopeInterfaceResolutionBuilder.CreateTransientScopeInstanceReferenceResolution(parameter, $"({Constants.ThisKeyword} as {_transientScopeInterfaceResolutionBuilder.Name})");

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
                Constants.ThisKeyword,
                currentParameters);

    public override void RegisterDisposalType(DisposalType disposalType) => _containerResolutionBuilder.RegisterDisposalType(disposalType);

    public bool HasWorkToDo => 
        _transientScopeRootFunctionResolutions.Values.Any(f => f.HasWorkToDo) 
        || RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo)
        || LocalFunctions.Values.Any(r => r.HasWorkToDo);

    public TransientScopeRootResolution AddCreateResolveFunction(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        string containerInstanceScopeReference,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters)
    {
        var key = $"{rootType.FullName()}{(currentParameters.Any() ? $"_{string.Join(";", currentParameters)}" : "")}";
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
            DoLocalFunctionsWork();
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
                    f.SynchronicityDecision))
                .ToList(),
            LocalFunctions
                .Values
                .Select(lf => lf.Build())
                .Select(f => new LocalFunctionResolution(
                    f.Reference,
                    f.TypeFullName,
                    f.AccessModifier,
                    f.Resolvable,
                    f.Parameter,
                    f.SynchronicityDecision))
                .ToList(),
            BuildDisposalHandling(),
            RangedInstanceReferenceResolutions.Values.Select(b => b.Build()).ToList(),
            _containerReference,
            _containerParameterReference,
            Name,
            AddForDisposal,
            AddForDisposalAsync);

    public MultiSynchronicityFunctionCallResolution EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker) => CreateRangedInstanceReferenceResolution(
        parameter,
        label,
        reference,
        "Doesn't Matter, because for interface",
        synchronicityDecisionMaker,
        true);
}