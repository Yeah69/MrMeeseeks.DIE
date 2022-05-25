using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IRangeResolutionBaseBuilder
{
    ICheckTypeProperties CheckTypeProperties { get; }
    IUserProvidedScopeElements UserProvidedScopeElements { get; }
    
    MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter);

    MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);

    MultiSynchronicityFunctionCallResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);
    
    TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    ScopeRootResolution CreateScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    void RegisterDisposalType(DisposalType disposalType);
}

internal abstract class RangeResolutionBaseBuilder : IRangeResolutionBaseBuilder
{
    public ICheckTypeProperties CheckTypeProperties { get; }
    public IUserProvidedScopeElements UserProvidedScopeElements { get; }

    protected readonly WellKnownTypes WellKnownTypes;
    private readonly Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> _rangedFunctionGroupResolutionBuilderFactory;
    private readonly Func<IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakerFactory;

    protected readonly IReferenceGenerator RootReferenceGenerator;
    protected readonly IDictionary<string, IRangedFunctionGroupResolutionBuilder> RangedInstanceReferenceResolutions =
        new Dictionary<string, IRangedFunctionGroupResolutionBuilder>();
    protected readonly string Name;

    protected RangeResolutionBaseBuilder(
        // parameters
        string name,
        ICheckTypeProperties checkTypeProperties,
        IUserProvidedScopeElements userProvidedScopeElements,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory)
    {
        CheckTypeProperties = checkTypeProperties;
        UserProvidedScopeElements = userProvidedScopeElements;
        WellKnownTypes = wellKnownTypes;
        _rangedFunctionGroupResolutionBuilderFactory = rangedFunctionGroupResolutionBuilderFactory;
        _synchronicityDecisionMakerFactory = synchronicityDecisionMakerFactory;

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        
        Name = name;
    }

    public abstract MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter);

    public abstract MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter);

    public MultiSynchronicityFunctionCallResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Scope",
            null,
            "this",
            new (_synchronicityDecisionMakerFactory));
    
    public abstract TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    public abstract ScopeRootResolution CreateScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    public abstract void RegisterDisposalType(DisposalType disposalType);

    protected MultiSynchronicityFunctionCallResolution CreateRangedInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string label,
        string? reference,
        string owningObjectReference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker)
    {
        var (implementationType, currentParameters, _) = parameter;
        InterfaceExtension? interfaceExtension = parameter switch
        {
            ForConstructorParameterWithComposition withComposition => withComposition.Composition,
            ForConstructorParameterWithDecoration withDecoration => withDecoration.Decoration,
            _ => null
        };
        var key = $"{implementationType.FullName()}{interfaceExtension?.KeySuffix() ?? ""}";
        if (!RangedInstanceReferenceResolutions.TryGetValue(key, out var functionGroup))
        {
            var decorationSuffix = interfaceExtension?.RangedNameSuffix() ?? "";
            functionGroup = _rangedFunctionGroupResolutionBuilderFactory(label, reference, implementationType, decorationSuffix, this);
            RangedInstanceReferenceResolutions[key] = functionGroup;
        }

        return functionGroup
            .GetInstanceFunction(parameter, synchronicityDecisionMaker)
            .BuildFunctionCall(currentParameters, owningObjectReference);
    }

    protected void DoRangedInstancesWork()
    {
        while (RangedInstanceReferenceResolutions.Values.Any(r => r.HasWorkToDo))
        {
            foreach (var builder in RangedInstanceReferenceResolutions.Values.ToList())
            {
                builder.DoWork();
            }
        }
    }

    protected DisposalHandling BuildDisposalHandling() =>
        new(new SyncDisposableCollectionResolution(
                RootReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfSyncDisposable),
                WellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()),
            new AsyncDisposableCollectionResolution(
                RootReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfAsyncDisposable),
                WellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()),
            Name,
            RootReferenceGenerator.Generate("_disposed"),
            RootReferenceGenerator.Generate("disposed"),
            RootReferenceGenerator.Generate("Disposed"),
            RootReferenceGenerator.Generate("disposable"));
}