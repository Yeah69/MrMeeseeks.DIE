﻿namespace MrMeeseeks.DIE;

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
    string RangeName,
    DisposalHandling DisposalHandling) : Resolvable(Reference, TypeFullName);

internal record TransientScopeAsDisposableResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record ErrorTreeItem(
    string Message) : Resolvable("error_99_99", "Error");

internal record InterfaceResolution(
    string Reference,
    string TypeFullName,
    ResolutionTreeItem Dependency) : Resolvable(Reference, TypeFullName);

internal interface ITypeInitializationResolution {}

internal record SyncTypeInitializationResolution(
    string TypeFullName,
    string MethodName) : ITypeInitializationResolution;

internal record TaskBaseTypeInitializationResolution(
    string TypeFullName,
    string MethodName,
    string TaskTypeFullName,
    string TaskReference) : ITypeInitializationResolution
{
    internal bool Await { get; set; } = true;
}

internal record TaskTypeInitializationResolution(
    string TypeFullName,
    string MethodName,
    string TaskTypeFullName,
    string TaskReference) : TaskBaseTypeInitializationResolution(TypeFullName, MethodName, TaskTypeFullName, TaskReference);

internal record ValueTaskTypeInitializationResolution(
    string TypeFullName,
    string MethodName,
    string TaskTypeFullName,
    string TaskReference) : TaskBaseTypeInitializationResolution(TypeFullName, MethodName, TaskTypeFullName, TaskReference);

internal record ConstructorResolution(
    string Reference,
    string TypeFullName,
    DisposableCollectionResolution? DisposableCollectionResolution,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameter,
    IReadOnlyList<(string Name, Resolvable Dependency)> InitializedProperties,
    ITypeInitializationResolution? Initialization) : Resolvable(Reference, TypeFullName);

internal record SyntaxValueTupleResolution(
    string Reference,
    string TypeFullName,
    IReadOnlyList<Resolvable> Items) : Resolvable(Reference, TypeFullName);

internal record TransientScopeRootResolution(
    string Reference,
    string TypeFullName,
    string TransientScopeReference,
    string TransientScopeTypeFullName,
    string ContainerInstanceScopeReference,
    IReadOnlyList<ParameterResolution> Parameter,
    DisposableCollectionResolution DisposableCollectionResolution,
    ScopeRootFunction ScopeRootFunction) : Resolvable(Reference, TypeFullName);

internal record ScopeRootResolution(
    string Reference,
    string TypeFullName,
    string ScopeReference,
    string ScopeTypeFullName,
    string ContainerInstanceScopeReference,
    string TransientInstanceScopeReference,
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

internal record TransientScopeInstanceInterfaceFunction(
    IReadOnlyList<ParameterResolution> Parameter,
    string Reference,
    string TypeFullName);

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
        Array.Empty<(string Name, Resolvable Dependency)>(),
        null);

internal abstract record RangeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances,
    string ContainerReference) : ResolutionTreeItem;

internal record TransientScopeInterfaceResolution(
    IReadOnlyList<TransientScopeInstanceInterfaceFunction> Functions,
    string Name,
    string ContainerAdapterName);

internal record ScopeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances,
    string ContainerReference,
    string ContainerParameterReference,
    string TransientScopeReference,
    string TransientScopeParameterReference,
    string Name) 
    : RangeResolution(RootResolutions, DisposalHandling, RangedInstances, ContainerReference);

internal record TransientScopeResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances,
    string ContainerReference,
    string ContainerParameterReference,
    string Name) 
    : RangeResolution(RootResolutions, DisposalHandling, RangedInstances, ContainerReference);

internal record ContainerResolution(
    IReadOnlyList<RootResolutionFunction> RootResolutions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstance> RangedInstances,
    TransientScopeInterfaceResolution TransientScopeInterface,
    string TransientScopeAdapterReference,
    IReadOnlyList<TransientScopeResolution> TransientScopes,
    IReadOnlyList<ScopeResolution> Scopes)
    : RangeResolution(RootResolutions, DisposalHandling, RangedInstances, "this");

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

internal abstract record TaskBaseResolution(
    Resolvable WrappedResolvable,
    string TaskReference,
    string TaskFullName) : Resolvable(TaskReference, TaskFullName);

internal record TaskFromTaskResolution(
    Resolvable WrappedResolvable,
    TaskTypeInitializationResolution Initialization,
    string TaskReference,
    string TaskFullName) : TaskBaseResolution(WrappedResolvable, TaskReference, TaskFullName);
internal record TaskFromValueTaskResolution(
    Resolvable WrappedResolvable,
    ValueTaskTypeInitializationResolution Initialization,
    string TaskReference,
    string TaskFullName) : TaskBaseResolution(WrappedResolvable, TaskReference, TaskFullName);
internal record TaskFromSyncResolution(
    Resolvable WrappedResolvable,
    string TaskReference,
    string TaskFullName) : TaskBaseResolution(WrappedResolvable, TaskReference, TaskFullName);
    
internal record ValueTaskFromTaskResolution(
    Resolvable WrappedResolvable,
    TaskTypeInitializationResolution Initialization,
    string ValueTaskReference,
    string ValueTaskFullName) : TaskBaseResolution(WrappedResolvable, ValueTaskReference, ValueTaskFullName);
internal record ValueTaskFromValueTaskResolution(
    Resolvable WrappedResolvable,
    ValueTaskTypeInitializationResolution Initialization,
    string ValueTaskReference,
    string ValueTaskFullName) : TaskBaseResolution(WrappedResolvable, ValueTaskReference, ValueTaskFullName);
internal record ValueTaskFromSyncResolution(
    Resolvable WrappedResolvable,
    string ValueTaskReference,
    string ValueTaskFullName) : TaskBaseResolution(WrappedResolvable, ValueTaskReference, ValueTaskFullName);