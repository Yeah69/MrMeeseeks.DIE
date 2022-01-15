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
    IReadOnlyList<ParameterResolution> Parameter,
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
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameter,
    IReadOnlyList<(string Name, Resolvable Dependency)> InitializedProperties) : Resolvable(Reference, TypeFullName);

internal record SyntaxValueTupleResolution(
    string Reference,
    string TypeFullName,
    IReadOnlyList<Resolvable> Items) : Resolvable(Reference, TypeFullName);

internal record ScopeRootResolution(
    string Reference,
    string TypeFullName,
    string ScopeReference,
    string ScopeTypeFullName,
    string ContainerInstanceScopeReference,
    IReadOnlyList<ParameterResolution> Parameter,
    DisposableCollectionResolution DisposableCollectionResolution,
    ScopeRootFunction ScopeRootFunction) : Resolvable(Reference, TypeFullName);

internal record ScopeRootFunction(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record RangedInstance(
    RangedInstanceFunction Function,
    IReadOnlyList<RangedInstanceFunctionOverload> Overloads,
    DisposalHandling DisposalHandling);

internal record ParameterResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record RangedInstanceFunction(
    string Reference,
    string TypeFullName,
    string FieldReference,
    string LockReference);

internal record RangedInstanceFunctionOverload(
    Resolvable Dependency,
    IReadOnlyList<ParameterResolution> Parameter);

internal record RangedInstanceReferenceResolution(
    string Reference,
    RangedInstanceFunction Function,
    IReadOnlyList<Resolvable> Parameter,
    string OwningObjectReference) : Resolvable(Reference, Function.TypeFullName);

internal record FuncResolution(
    string Reference,
    string TypeFullName,
    IReadOnlyList<ParameterResolution> Parameter,
    ResolutionTreeItem Dependency) : Resolvable(Reference, TypeFullName);

internal record FactoryResolution(
    string Reference,
    string TypeFullName,
    string FunctionName,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameter) : Resolvable(Reference, TypeFullName);

internal record CollectionResolution(
    string Reference,
    string TypeFullName,
    string ItemFullName,
    IReadOnlyList<ResolutionTreeItem> Parameter) : Resolvable(Reference, TypeFullName);

internal record DisposableCollectionResolution(
    string Reference,
    string TypeFullName) 
    : ConstructorResolution(
        Reference,
        TypeFullName, 
        null, 
        Array.Empty<(string Name, Resolvable Dependency)>(), 
        Array.Empty<(string Name, Resolvable Dependency)>());

internal abstract record RangeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances) : ResolutionTreeItem;

internal record ScopeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances,
    string ContainerReference,
    string ContainerParameterReference,
    string Name) 
    : RangeResolution(RootResolutions, DisposalHandling, RangedInstances);

internal record ContainerResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances,
    ScopeResolution DefaultScope)
    : RangeResolution(RootResolutions, DisposalHandling, RangedInstances);

internal record DisposalHandling(
    DisposableCollectionResolution DisposableCollection,
    string RangeName,
    string DisposedFieldReference,
    string DisposedLocalReference,
    string DisposedPropertyReference,
    string DisposableLocalReference);

internal record FieldResolution(
    string Reference,
    string TypeFullName,
    string FieldName) : Resolvable(Reference, TypeFullName);