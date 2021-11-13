namespace MrMeeseeks.DIE;

internal abstract record ResolutionTreeItem;
    
internal abstract record Resolvable(
    string Reference,
    string TypeFullName) : ResolutionTreeItem; 

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

internal abstract record RangedInstanceBase<T>(
    T Function,
    Resolvable Dependency) where T : RangedInstanceFunctionBase; 

internal record SingleInstance(
    SingleInstanceFunction Function,
    Resolvable Dependency) : RangedInstanceBase<SingleInstanceFunction>(Function, Dependency);

internal record ScopedInstance(
    ScopedInstanceFunction Function,
    Resolvable Dependency) : RangedInstanceBase<ScopedInstanceFunction>(Function, Dependency);

internal record FuncParameterResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal abstract record RangedInstanceFunctionBase(
    string Reference,
    string TypeFullName,
    INamedTypeSymbol Type,
    string FieldReference,
    string LockReference);

internal record SingleInstanceFunction(
    string Reference,
    string TypeFullName,
    INamedTypeSymbol Type,
    string FieldReference,
    string LockReference) : RangedInstanceFunctionBase(Reference, TypeFullName, Type, FieldReference, LockReference);

internal record ScopedInstanceFunction(
    string Reference,
    string TypeFullName,
    INamedTypeSymbol Type,
    string FieldReference,
    string LockReference) : RangedInstanceFunctionBase(Reference, TypeFullName, Type, FieldReference, LockReference);

internal abstract record RangedInstanceReferenceResolutionBase<T>(
    string Reference,
    T Function) : Resolvable(Reference, Function.TypeFullName) where T : RangedInstanceFunctionBase;

internal record SingleInstanceReferenceResolution(
    string Reference,
    SingleInstanceFunction Function) : RangedInstanceReferenceResolutionBase<SingleInstanceFunction>(Reference, Function);

internal record ScopedInstanceReferenceResolution(
    string Reference,
    ScopedInstanceFunction Function) : RangedInstanceReferenceResolutionBase<ScopedInstanceFunction>(Reference, Function);

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
    IReadOnlyList<(Resolvable, INamedTypeSymbol)> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<ScopedInstance> ScopedInstanceResolutions) : ResolutionTreeItem;

internal record ScopeResolution(
    IReadOnlyList<(Resolvable, INamedTypeSymbol)> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<ScopedInstance> ScopedInstanceResolutions) 
    : RangeResolution(RootResolutions, DisposalHandling, ScopedInstanceResolutions);

internal record ContainerResolution(
    IReadOnlyList<(Resolvable, INamedTypeSymbol)> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<ScopedInstance> ScopedInstanceResolutions,
    IReadOnlyList<SingleInstance> SingleInstanceResolutions,
    ScopeResolution DefaultScope)
    : RangeResolution(RootResolutions, DisposalHandling, ScopedInstanceResolutions);

internal record DisposalHandling(
    DisposableCollectionResolution DisposableCollection,
    string DisposedFieldReference,
    string DisposedLocalReference,
    string DisposedPropertyReference,
    string DisposableLocalReference);