namespace MrMeeseeks.DIE.ResolutionBuilding;

internal abstract record InterfaceExtension(
    INamedTypeSymbol InterfaceType)
{
    internal string KeySuffix() =>
        this switch
        {
            DecorationInterfaceExtension decoration => $":::{decoration.ImplementationType.FullName()}",
            CompositionInterfaceExtension composition => $":::{string.Join(":::", composition.ImplementationTypes.Select(i => i.FullName()))}",
            _ => throw new ArgumentException()
        };
        
    internal string RangedNameSuffix() =>
        this switch
        {
            DecorationInterfaceExtension decoration => $"_{decoration.ImplementationType.Name}",
            CompositionInterfaceExtension =>  "_Composite",
            _ => throw new ArgumentException()
        };
}

internal record DecorationInterfaceExtension(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        INamedTypeSymbol DecoratorType,
        InterfaceResolution CurrentInterfaceResolution)
    : InterfaceExtension(InterfaceType);
internal record CompositionInterfaceExtension(
        INamedTypeSymbol InterfaceType,
        IReadOnlyList<INamedTypeSymbol> ImplementationTypes,
        INamedTypeSymbol CompositeType,
        IReadOnlyList<InterfaceResolution> InterfaceResolutionComposition)
    : InterfaceExtension(InterfaceType);

internal record Parameter;

internal record SwitchTypeParameter(
        ITypeSymbol Type,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters)
    : Parameter;

internal record SwitchInterfaceParameter(
        ITypeSymbol Type,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters)
    : Parameter;

internal record SwitchInterfaceAfterScopeRootParameter(
    INamedTypeSymbol InterfaceType,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);


internal record SwitchInterfaceForSpecificImplementationParameter(
    INamedTypeSymbol InterfaceType,
    INamedTypeSymbol ImplementationType,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);

internal record CreateInterfaceParameter(
    INamedTypeSymbol InterfaceType,
    INamedTypeSymbol ImplementationType,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);

internal record CreateInterfaceParameterAsComposition(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        CompositionInterfaceExtension Composition)
    : CreateInterfaceParameter(InterfaceType, ImplementationType, CurrentParameters);

internal record SwitchClassParameter(
    INamedTypeSymbol TypeSymbol,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);
    
internal record SwitchImplementationParameter(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters)
{
    public INamedTypeSymbol ReturnType => ImplementationType;
}
    
internal record SwitchImplementationParameterWithDecoration(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        DecorationInterfaceExtension Decoration)
    : SwitchImplementationParameter(ImplementationType, CurrentParameters);
    
internal record SwitchImplementationParameterWithComposition(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        CompositionInterfaceExtension Composition)
    : SwitchImplementationParameter(ImplementationType, CurrentParameters);

internal record ForConstructorParameter(
    INamedTypeSymbol ImplementationType,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters);

internal abstract record ForConstructorParameterWithInterfaceExtension(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters,
        InterfaceExtension InterfaceExtension)
    : ForConstructorParameter(ImplementationType, CurrentFuncParameters);

internal record ForConstructorParameterWithDecoration(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters,
        DecorationInterfaceExtension Decoration)
    : ForConstructorParameterWithInterfaceExtension(ImplementationType, CurrentFuncParameters, Decoration);

internal record ForConstructorParameterWithComposition(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentFuncParameters,
        CompositionInterfaceExtension Composition)
    : ForConstructorParameterWithInterfaceExtension(ImplementationType, CurrentFuncParameters, Composition);

//(RangedInstanceFunction, IReadOnlyList<(ITypeSymbol, ParameterResolution)>, INamedTypeSymbol, InterfaceExtension?)
internal record RangedInstanceResolutionsQueueItem(
    ForConstructorParameter Parameter,
    string Label,
    string Reference,
    string Key);

internal record SwitchTaskParameter((Resolvable, ITaskConsumableResolution?) InnerResolution) : Parameter;

internal record SwitchValueTaskParameter((Resolvable, ITaskConsumableResolution?) InnerResolution) : Parameter;

internal enum TaskType
{
    Task,
    ValueTask
}