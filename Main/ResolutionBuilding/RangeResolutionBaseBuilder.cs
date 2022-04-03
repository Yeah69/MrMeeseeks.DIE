using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IRangeResolutionBaseBuilder
{
    ICheckTypeProperties CheckTypeProperties { get; }
    IUserProvidedScopeElements UserProvidedScopeElements { get; }
    DisposableCollectionResolution DisposableCollectionResolution { get; }
    DisposalHandling DisposalHandling { get; }
    
    MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter);

    MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);

    MultiSynchronicityFunctionCallResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);
    
    TransientScopeRootResolution CreateTransientScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
}

internal abstract class RangeResolutionBaseBuilder : IRangeResolutionBaseBuilder
{
    public ICheckTypeProperties CheckTypeProperties { get; }
    public IUserProvidedScopeElements UserProvidedScopeElements { get; }
    public DisposableCollectionResolution DisposableCollectionResolution { get; }
    public DisposalHandling DisposalHandling { get; }

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
        DisposableCollectionResolution = new DisposableCollectionResolution(
            RootReferenceGenerator.Generate(WellKnownTypes.ConcurrentBagOfDisposable),
            WellKnownTypes.ConcurrentBagOfDisposable.FullName());
        
        Name = name;
        DisposalHandling = new DisposalHandling(
            DisposableCollectionResolution,
            Name,
            RootReferenceGenerator.Generate("_disposed"),
            RootReferenceGenerator.Generate("disposed"),
            RootReferenceGenerator.Generate("Disposed"),
            RootReferenceGenerator.Generate("disposable"));
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
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);
    
    public abstract ScopeRootResolution CreateScopeRootResolution(
        IScopeRootParameter parameter,
        INamedTypeSymbol rootType,
        DisposableCollectionResolution disposableCollectionResolution,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> currentParameters);

    protected MultiSynchronicityFunctionCallResolution CreateRangedInstanceReferenceResolution(
        ForConstructorParameter parameter,
        string label,
        string? reference,
        string owningObjectReference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker)
    {
        var (implementationType, currentParameters) = parameter;
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
}