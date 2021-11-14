namespace MrMeeseeks.DIE;

internal abstract record ResolutionTreeItem;
    
internal abstract record Resolvable(
    string Reference,
    string TypeFullName) : ResolutionTreeItem;

internal record RootResolutionFunction(
    string Reference,
    string TypeFullName,
    string AccessModifier,
    Resolvable Resolvable,
    string ExplicitImplementationFullName,
    string RangeName,
    DisposalHandling DisposalHandling) : Resolvable(Reference, TypeFullName);

internal record ErrorTreeItem(
    string Message) : Resolvable("error_99_99", "Error");

internal record InterfaceResolution(
    string Reference,
    string TypeFullName,
    ResolutionTreeItem Dependency) : Resolvable(Reference, TypeFullName);

internal record ConstructorResolution(
    string Reference,
    string TypeFullName,
    DisposableCollectionResolution? DisposableCollectionResolution,
    IReadOnlyList<(string name, Resolvable Dependency)> Parameter) : Resolvable(Reference, TypeFullName);

internal record ScopeRootResolution(
    string Reference,
    string TypeFullName,
    string ScopeReference,
    string ScopeTypeFullName,
    string SingleInstanceScopeReference,
    DisposableCollectionResolution DisposableCollectionResolution,
    ScopeRootFunction ScopeRootFunction) : Resolvable(Reference, TypeFullName);

internal record ScopeRootFunction(
    string Reference,
    string TypeFullName,
    INamedTypeSymbol Type) : Resolvable(Reference, TypeFullName);

internal record RangedInstance(
    RangedInstanceFunction Function,
    Resolvable Dependency,
    DisposalHandling DisposalHandling);

internal record FuncParameterResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record RangedInstanceFunction(
    string Reference,
    string TypeFullName,
    INamedTypeSymbol Type,
    string FieldReference,
    string LockReference);

internal record RangedInstanceReferenceResolution(
    string Reference,
    RangedInstanceFunction Function,
    string owningObjectReference) : Resolvable(Reference, Function.TypeFullName);

internal record FuncResolution(
    string Reference,
    string TypeFullName,
    IReadOnlyList<FuncParameterResolution> Parameter,
    ResolutionTreeItem Dependency) : Resolvable(Reference, TypeFullName);

internal record CollectionResolution(
    string Reference,
    string TypeFullName,
    string ItemFullName,
    IReadOnlyList<ResolutionTreeItem> Parameter) : Resolvable(Reference, TypeFullName);

internal record DisposableCollectionResolution(
    string Reference,
    string TypeFullName) : ConstructorResolution(Reference, TypeFullName, null, Array.Empty<(string name, Resolvable Dependency)>());

internal abstract record RangeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> AllRangedInstances) : ResolutionTreeItem;

internal record ScopeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> ScopedInstanceResolutions,
    string ContainerReference,
    string ContainerParameterReference,
    string Name) 
    : RangeResolution(RootResolutions, DisposalHandling, ScopedInstanceResolutions);

internal record ContainerResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> ScopedInstanceResolutions,
    IReadOnlyList<RangedInstance> SingleInstanceResolutions,
    ScopeResolution DefaultScope)
    : RangeResolution(RootResolutions, DisposalHandling, SingleInstanceResolutions.Concat(ScopedInstanceResolutions).ToList());

internal record DisposalHandling(
    DisposableCollectionResolution DisposableCollection,
    string RangeName,
    string DisposedFieldReference,
    string DisposedLocalReference,
    string DisposedPropertyReference,
    string DisposableLocalReference);