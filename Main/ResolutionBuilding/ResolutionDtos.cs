namespace MrMeeseeks.DIE.ResolutionBuilding;

internal abstract record InterfaceExtension(
    INamedTypeSymbol InterfaceType)
{
    internal string KeySuffix() =>
        this switch
        {
            DecorationInterfaceExtension decoration => $":::{decoration.ImplementationType.FullName()}",
            CompositionInterfaceExtension composition => string.Join(":::", composition.ImplementationTypes.Select(i => i.FullName())),
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

internal interface IScopeRootParameter
{
    string KeySuffix();
    string RootFunctionSuffix();
}

internal record SwitchInterfaceAfterScopeRootParameter(
        INamedTypeSymbol InterfaceType,
        IReadOnlyList<INamedTypeSymbol> ImplementationTypes,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters) 
    : IScopeRootParameter
{
    public string KeySuffix() => ":::InterfaceAfterRoot";

    public string RootFunctionSuffix() => "_InterfaceAfterRoot";
}


internal record SwitchInterfaceForSpecificImplementationParameter(
    INamedTypeSymbol InterfaceType,
    INamedTypeSymbol ImplementationType,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);

internal record CreateInterfaceParameter(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters)
    : IScopeRootParameter
{
    public virtual string KeySuffix() => ":::NormalInterface";

    public virtual string RootFunctionSuffix() => "_NormalInterface";
}

internal record CreateInterfaceParameterAsComposition(
        INamedTypeSymbol InterfaceType,
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters,
        CompositionInterfaceExtension Composition)
    : CreateInterfaceParameter(InterfaceType, ImplementationType, CurrentParameters)
{
    public override string KeySuffix() => ":::CompositeInterface";

    public override string RootFunctionSuffix() => "_CompositeInterface";
}

internal record SwitchClassParameter(
    ITypeSymbol TypeSymbol,
    IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters);
    
internal record SwitchImplementationParameter(
        INamedTypeSymbol ImplementationType,
        IReadOnlyList<(ITypeSymbol Type, ParameterResolution Resolution)> CurrentParameters)
    : IScopeRootParameter
{
    public virtual string KeySuffix() => ":::Implementation";

    public virtual string RootFunctionSuffix() => "";
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
    RangedInstanceFunction Function,
    IReadOnlyList<(ITypeSymbol, ParameterResolution)> Parameters,
    INamedTypeSymbol ImplementationType,
    InterfaceExtension? InterfaceExtension);

internal record SwitchTaskParameter((Resolvable, ConstructorResolution?) InnerResolution) : Parameter;

internal record SwitchValueTaskParameter((Resolvable, ConstructorResolution?) InnerResolution) : Parameter;

internal enum TaskType
{
    Task,
    ValueTask
}