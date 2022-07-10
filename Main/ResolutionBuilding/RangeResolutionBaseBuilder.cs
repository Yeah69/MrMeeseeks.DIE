using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface IRangeResolutionBaseBuilder
{
    ICheckTypeProperties CheckTypeProperties { get; }
    IUserDefinedElements UserDefinedElements { get; }
    
    IErrorContext ErrorContext { get; }
    
    MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(
        ForConstructorParameter parameter);

    MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);

    MultiSynchronicityFunctionCallResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter);
    
    TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);
    
    ScopeRootResolution CreateScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);

    void RegisterDisposalType(DisposalType disposalType);

    IFunctionResolutionBuilder CreateLocalFunctionResolution(
        INamedTypeSymbol type,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);
}

internal abstract class RangeResolutionBaseBuilder : IRangeResolutionBaseBuilder
{
    public ICheckTypeProperties CheckTypeProperties { get; }
    public IUserDefinedElements UserDefinedElements { get; }
    public abstract IErrorContext ErrorContext { get; }

    protected IMethodSymbol? AddForDisposal { get; }
    protected IMethodSymbol? AddForDisposalAsync { get; }

    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> _rangedFunctionGroupResolutionBuilderFactory;
    private readonly Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>, ILocalFunctionResolutionBuilder> _localFunctionResolutionBuilderFactory;
    private readonly Func<IFunctionResolutionSynchronicityDecisionMaker> _synchronicityDecisionMakerFactory;

    protected readonly IReferenceGenerator RootReferenceGenerator;
    protected readonly IDictionary<string, IRangedFunctionGroupResolutionBuilder> RangedInstanceReferenceResolutions =
        new Dictionary<string, IRangedFunctionGroupResolutionBuilder>();
    
    protected readonly IDictionary<string, IFunctionResolutionBuilder> LocalFunctions = new Dictionary<string, IFunctionResolutionBuilder>();
    
    protected readonly string Name;

    protected RangeResolutionBaseBuilder(
        // parameters
        string name,
        ICheckTypeProperties checkTypeProperties,
        IUserDefinedElements userDefinedElements,
        
        // dependencies
        WellKnownTypes wellKnownTypes,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        Func<string, string?, INamedTypeSymbol, string, IRangeResolutionBaseBuilder, IRangedFunctionGroupResolutionBuilder> rangedFunctionGroupResolutionBuilderFactory,
        Func<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMakerFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
    {
        CheckTypeProperties = checkTypeProperties;
        UserDefinedElements = userDefinedElements;
        _wellKnownTypes = wellKnownTypes;
        _rangedFunctionGroupResolutionBuilderFactory = rangedFunctionGroupResolutionBuilderFactory;
        _synchronicityDecisionMakerFactory = synchronicityDecisionMakerFactory;
        _localFunctionResolutionBuilderFactory = localFunctionResolutionBuilderFactory;

        RootReferenceGenerator = referenceGeneratorFactory.Create();
        
        Name = name;
        
        AddForDisposal = userDefinedElements.AddForDisposal;
        AddForDisposalAsync = userDefinedElements.AddForDisposalAsync;
    }

    public abstract MultiSynchronicityFunctionCallResolution CreateContainerInstanceReferenceResolution(ForConstructorParameter parameter);

    public abstract MultiSynchronicityFunctionCallResolution CreateTransientScopeInstanceReferenceResolution(ForConstructorParameter parameter);

    public MultiSynchronicityFunctionCallResolution CreateScopeInstanceReferenceResolution(
        ForConstructorParameter parameter) =>
        CreateRangedInstanceReferenceResolution(
            parameter,
            "Scope",
            null,
            Constants.ThisKeyword,
            new (_synchronicityDecisionMakerFactory));
    
    public abstract TransientScopeRootResolution CreateTransientScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);
    
    public abstract ScopeRootResolution CreateScopeRootResolution(
        SwitchImplementationParameter parameter,
        INamedTypeSymbol rootType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters);

    public abstract void RegisterDisposalType(DisposalType disposalType);
    public IFunctionResolutionBuilder CreateLocalFunctionResolution(INamedTypeSymbol type, ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> currentParameters)
    {
        var key = $"{type.FullName()}:::{string.Join(":::", currentParameters.Select(p => p.Value.Item2.TypeFullName))}";
        if (!LocalFunctions.TryGetValue(key, out var localFunction))
        {
            localFunction = _localFunctionResolutionBuilderFactory(this, type, currentParameters);
            LocalFunctions[key] = localFunction;
        }

        return localFunction;
    }

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

    protected void DoLocalFunctionsWork()
    {
        while (LocalFunctions.Values.Any(lf => lf.HasWorkToDo))
        {
            foreach (var localFunction in LocalFunctions.Values.Where(lf => lf.HasWorkToDo).ToList())
            {
                localFunction.DoWork();
            }
        }
    }

    protected DisposalHandling BuildDisposalHandling()
    {
        if (AddForDisposal is { })
        {
            RegisterDisposalType(DisposalType.Sync);
        }

        if (AddForDisposalAsync is { })
        {
            RegisterDisposalType(DisposalType.Async);
        }
        
        return new(new SyncDisposableCollectionResolution(
                RootReferenceGenerator.Generate(_wellKnownTypes.ConcurrentBagOfSyncDisposable),
                _wellKnownTypes.ConcurrentBagOfSyncDisposable.FullName()),
            new AsyncDisposableCollectionResolution(
                RootReferenceGenerator.Generate(_wellKnownTypes.ConcurrentBagOfAsyncDisposable),
                _wellKnownTypes.ConcurrentBagOfAsyncDisposable.FullName()),
            Name,
            RootReferenceGenerator.Generate("_disposed"),
            RootReferenceGenerator.Generate("disposed"),
            RootReferenceGenerator.Generate("Disposed"),
            RootReferenceGenerator.Generate("disposable"));
    }
}