using MrMeeseeks.DIE.ResolutionBuilding.Function;

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
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> CurrentParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack)
    : Parameter;
    
internal record SwitchImplementationParameter(
        INamedTypeSymbol ImplementationType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> CurrentParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack)
{
    public INamedTypeSymbol ReturnType => ImplementationType;
}

internal record ForConstructorParameter(
    INamedTypeSymbol ImplementationType,
    ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> CurrentParameters,
    IImmutableStack<INamedTypeSymbol> ImplementationStack);

internal abstract record ForConstructorParameterWithInterfaceExtension(
        INamedTypeSymbol ImplementationType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> CurrentFuncParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack,
        InterfaceExtension InterfaceExtension)
    : ForConstructorParameter(ImplementationType, CurrentFuncParameters, ImplementationStack);

internal record ForConstructorParameterWithDecoration(
        INamedTypeSymbol ImplementationType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> CurrentFuncParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack,
        DecorationInterfaceExtension Decoration)
    : ForConstructorParameterWithInterfaceExtension(ImplementationType, CurrentFuncParameters, ImplementationStack, Decoration);

internal record ForConstructorParameterWithComposition(
        INamedTypeSymbol ImplementationType,
        ImmutableSortedDictionary<string, (ITypeSymbol, ParameterResolution)> CurrentFuncParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack,
        CompositionInterfaceExtension Composition)
    : ForConstructorParameterWithInterfaceExtension(ImplementationType, CurrentFuncParameters, ImplementationStack, Composition);

internal record RangedInstanceResolutionsQueueItem(
    ForConstructorParameter Parameter,
    string Label,
    string Reference,
    string Key,
    FunctionResolutionBuilderHandle Handle);