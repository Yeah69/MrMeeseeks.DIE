namespace MrMeeseeks.DIE.ResolutionBuilding.Function;

internal abstract partial class FunctionResolutionBuilder
{
    private record SwitchInterfaceAfterScopeRootParameter(
        INamedTypeSymbol InterfaceType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack);

    private record CreateInterfaceParameter(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack);

    private record CreateInterfaceParameterAsComposition(
            INamedTypeSymbol InterfaceType,
            INamedTypeSymbol ImplementationType,
            IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
            IImmutableStack<INamedTypeSymbol> ImplementationStack,
            CompositionInterfaceExtension Composition)
        : CreateInterfaceParameter(InterfaceType, ImplementationType, CurrentParameters, ImplementationStack);

    private record SwitchClassParameter(
        INamedTypeSymbol TypeSymbol,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        IImmutableStack<INamedTypeSymbol> ImplementationStack);
    
    private record SwitchImplementationParameterWithDecoration(
            INamedTypeSymbol ImplementationType,
            IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
            IImmutableStack<INamedTypeSymbol> ImplementationStack,
            DecorationInterfaceExtension Decoration)
        : SwitchImplementationParameter(ImplementationType, CurrentParameters, ImplementationStack);
    
    private record SwitchImplementationParameterWithComposition(
            INamedTypeSymbol ImplementationType,
            IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
            IImmutableStack<INamedTypeSymbol> ImplementationStack,
            CompositionInterfaceExtension Composition)
        : SwitchImplementationParameter(ImplementationType, CurrentParameters, ImplementationStack);

    private record SwitchTaskParameter((Resolvable, ITaskConsumableResolution?) InnerResolution) : Parameter;

    private record SwitchValueTaskParameter((Resolvable, ITaskConsumableResolution?) InnerResolution) : Parameter;

    private enum TaskType
    {
        Task,
        ValueTask
    }
}