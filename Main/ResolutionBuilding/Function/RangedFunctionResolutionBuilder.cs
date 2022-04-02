namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal interface IRangedFunctionResolutionBuilder : IFunctionResolutionBuilder
{
}

internal class RangedFunctionResolutionBuilder : FunctionResolutionBuilder, IRangedFunctionResolutionBuilder
{
    private readonly IRangeResolutionBaseBuilder _rangeResolutionBaseBuilder;
    private readonly ForConstructorParameter _forConstructorParameter;

    public RangedFunctionResolutionBuilder(
        // parameter
        IRangeResolutionBaseBuilder rangeResolutionBaseBuilder,
        string reference,
        ForConstructorParameter forConstructorParameter,
        
        
        // dependencies
        WellKnownTypes wellKnownTypes, 
        IReferenceGeneratorFactory referenceGeneratorFactory, 
        Func<IRangeResolutionBaseBuilder, INamedTypeSymbol, IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)>, ILocalFunctionResolutionBuilder> localFunctionResolutionBuilderFactory)
        : base(rangeResolutionBaseBuilder, forConstructorParameter.ImplementationType, forConstructorParameter.CurrentFuncParameters, wellKnownTypes, referenceGeneratorFactory, localFunctionResolutionBuilderFactory)
    {
        _rangeResolutionBaseBuilder = rangeResolutionBaseBuilder;
        _forConstructorParameter = forConstructorParameter;
        
        Name = reference;
    }

    protected override string Name { get; }

    protected override Resolvable CreateResolvable() => CreateConstructorResolution(_forConstructorParameter).Item1;

    public override FunctionResolution Build()
    {
        AdjustForSynchronicity();
        
        return new(
            Name,
            TypeFullName,
            Resolvable.Value,
            _forConstructorParameter.CurrentFuncParameters.Select(t => t.Resolution).ToList(),
            _rangeResolutionBaseBuilder.DisposalHandling,
            LocalFunctions
                .Select(lf => lf.Build())
                .Select(f => new LocalFunctionResolution(
                    f.Reference,
                    f.TypeFullName,
                    f.Resolvable,
                    f.Parameter,
                    f.DisposalHandling,
                    f.LocalFunctions,
                    f.SynchronicityDecision))
                .ToList(),
            SynchronicityDecision.Value);
    }
}