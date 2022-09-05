﻿using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE;

internal abstract record ResolutionTreeItem;

internal abstract record Resolvable(string Reference, string TypeFullName) : ResolutionTreeItem;

internal record DeferringResolvable() : Resolvable("", "")
{
    internal Resolvable? Resolvable { get; set; }
}

internal record NewReferenceResolvable(
    string Reference,
    string TypeFullName,
    Resolvable Resolvable) : Resolvable(Reference, TypeFullName);

internal record ProxyResolvable(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record FunctionResolution(
    string Reference,
    string TypeFullName,
    string AccessModifier,
    Resolvable Resolvable,
    IReadOnlyList<ParameterResolution> Parameter,
    SynchronicityDecision SynchronicityDecision) : Resolvable(Reference, TypeFullName);

internal record CreateFunctionResolution(
    string Reference,
    string TypeFullName,
    string AccessModifier,
    Resolvable Resolvable,
    IReadOnlyList<ParameterResolution> Parameter,
    SynchronicityDecision SynchronicityDecision) 
    : FunctionResolution(Reference, TypeFullName, AccessModifier, Resolvable, Parameter, SynchronicityDecision);

internal record RangedInstanceFunctionResolution(
    string Reference,
    string TypeFullName,
    Resolvable Resolvable,
    IReadOnlyList<ParameterResolution> Parameter,
    SynchronicityDecision SynchronicityDecision) 
    : FunctionResolution(Reference, TypeFullName, Constants.PrivateKeyword, Resolvable, Parameter, SynchronicityDecision);

internal record RangedInstanceFunctionGroupResolution(
    string TypeFullName,
    IReadOnlyList<RangedInstanceFunctionResolution> Overloads,
    string FieldReference,
    string LockReference,
    string? IsCreatedForStructs,
    bool IsTransientScopeInstance);

internal record TransientScopeAsDisposableResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record TransientScopeAsAsyncDisposableResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record ErrorTreeItem(
    Diagnostic Diagnostic) : Resolvable("error_99_99", "Error");

internal record InterfaceResolution(
    string Reference,
    string TypeFullName,
    ResolutionTreeItem Dependency) : Resolvable(Reference, TypeFullName);

internal interface IInitializationResolution
{
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameters { get; }
    UserDefinedInitializerParametersInjectionResolution? UserDefinedInitializerParametersInjectionResolution { get; } 
}

internal record SyncInitializationResolution(
    string TypeFullName,
    string MethodName,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameters,
    UserDefinedInitializerParametersInjectionResolution? UserDefinedInitializerParametersInjectionResolution) : IInitializationResolution;

internal interface IAwaitableResolution
{
    bool Await { get; }
}

internal interface ITaskConsumableResolution {}

internal record TaskBaseInitializationResolution(
    string TypeFullName,
    string MethodName,
    string TaskTypeFullName,
    string TaskReference,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameters,
    UserDefinedInitializerParametersInjectionResolution? UserDefinedInitializerParametersInjectionResolution) : IInitializationResolution, IAwaitableResolution
{
    public bool Await { get; set; } = true;
}

internal record TaskInitializationResolution(
    string TypeFullName,
    string MethodName,
    string TaskTypeFullName,
    string TaskReference,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameters,
    UserDefinedInitializerParametersInjectionResolution? UserDefinedInitializerParametersInjectionResolution) 
    : TaskBaseInitializationResolution(TypeFullName, MethodName, TaskTypeFullName, TaskReference, Parameters, UserDefinedInitializerParametersInjectionResolution);

internal record ValueTaskInitializationResolution(
    string TypeFullName,
    string MethodName,
    string TaskTypeFullName,
    string TaskReference,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameters,
    UserDefinedInitializerParametersInjectionResolution? UserDefinedInitializerParametersInjectionResolution) 
    : TaskBaseInitializationResolution(TypeFullName, MethodName, TaskTypeFullName, TaskReference, Parameters, UserDefinedInitializerParametersInjectionResolution);

internal record ConstructorResolution(
    string Reference,
    string TypeFullName,
    DisposalType DisposalType,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameter,
    IReadOnlyList<(string Name, Resolvable Dependency)> InitializedProperties,
    IInitializationResolution? Initialization,
    UserDefinedConstructorParametersInjectionResolution? UserDefinedConstructorParametersInjection,
    UserDefinedPropertiesInjectionResolution? UserDefinedPropertiesInjection) : Resolvable(Reference, TypeFullName), ITaskConsumableResolution;

internal record LazyResolution(
    string Reference,
    string TypeFullName,
    IFactoryResolution Inner) : Resolvable(Reference, TypeFullName);

internal record SyntaxValueTupleResolution(
    string Reference,
    string TypeFullName,
    IReadOnlyList<Resolvable> Items) : Resolvable(Reference, TypeFullName);

internal record TransientScopeRootResolution(
    string TransientScopeReference,
    string TransientScopeTypeFullName,
    string ContainerInstanceScopeReference,
    MultiSynchronicityFunctionCallResolution ScopeRootFunction) : Resolvable(ScopeRootFunction.Reference, ScopeRootFunction.TypeFullName);

internal record ScopeRootResolution(
    string ScopeReference,
    string ScopeTypeFullName,
    string ContainerInstanceScopeReference,
    string TransientInstanceScopeReference,
    MultiSynchronicityFunctionCallResolution ScopeRootFunction) : Resolvable(ScopeRootFunction.Reference, ScopeRootFunction.Sync.OriginalTypeFullName);

internal record ScopeRootFunction(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record ParameterResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record InterfaceFunctionDeclarationResolution(
    string Reference,
    string TypeFullName,
    string TaskTypeFullName,
    string ValueTaskTypeFullName,
    IReadOnlyList<ParameterResolution> Parameter,
    Lazy<SynchronicityDecision> SynchronicityDecision);

internal record FuncResolution(
    string Reference,
    string TypeFullName,
    IReadOnlyList<ParameterResolution> LambdaParameters,
    IFactoryResolution Inner) : Resolvable(Reference, TypeFullName);

internal record FactoryResolution(
    string Reference,
    string TypeFullName,
    string FunctionName,
    IReadOnlyList<(string Name, Resolvable Dependency)> Parameter,
    bool Await) : Resolvable(Reference, TypeFullName);

internal record OutParameterResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal abstract record UserDefinedInjectionResolution(
    string FunctionName,
    IReadOnlyList<(string Name, Resolvable Dependency, bool isOut)> Parameter) : Resolvable("foo", "bar");

internal record UserDefinedConstructorParametersInjectionResolution(
    string FunctionName,
    IReadOnlyList<(string Name, Resolvable Dependency, bool isOut)> Parameter) : UserDefinedInjectionResolution(FunctionName, Parameter);

internal record UserDefinedPropertiesInjectionResolution(
    string FunctionName,
    IReadOnlyList<(string Name, Resolvable Dependency, bool isOut)> Parameter) : UserDefinedInjectionResolution(FunctionName, Parameter);

internal record UserDefinedInitializerParametersInjectionResolution(
    string FunctionName,
    IReadOnlyList<(string Name, Resolvable Dependency, bool isOut)> Parameter) : UserDefinedInjectionResolution(FunctionName, Parameter);

internal record CollectionResolution(
    string Reference,
    string TypeFullName,
    string ItemFullName,
    IReadOnlyList<ResolutionTreeItem> Parameter) : Resolvable(Reference, TypeFullName);

internal record SyncDisposableCollectionResolution(
        string Reference,
        string TypeFullName) 
    : ConstructorResolution(
        Reference,
        TypeFullName, 
        DisposalType.None,
        Array.Empty<(string Name, Resolvable Dependency)>(), 
        Array.Empty<(string Name, Resolvable Dependency)>(),
        null,
        null,
        null);

internal record AsyncDisposableCollectionResolution(
        string Reference,
        string TypeFullName) 
    : ConstructorResolution(
        Reference,
        TypeFullName, 
        DisposalType.None,
        Array.Empty<(string Name, Resolvable Dependency)>(), 
        Array.Empty<(string Name, Resolvable Dependency)>(),
        null,
        null,
        null);

internal abstract record RangeResolution(
    IReadOnlyList<CreateFunctionResolution> CreateFunctions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstanceFunctionGroupResolution> RangedInstanceFunctionGroups,
    string ContainerReference,
    IMethodSymbol? AddForDisposal,
    IMethodSymbol? AddForDisposalAsync) : ResolutionTreeItem;

internal record TransientScopeInterfaceResolution(
    IReadOnlyList<InterfaceFunctionDeclarationResolution> Functions,
    string Name,
    string ContainerAdapterName);

internal record ScopeResolution(
    IReadOnlyList<CreateFunctionResolution> CreateFunctions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstanceFunctionGroupResolution> RangedInstanceFunctionGroups,
    string ContainerReference,
    string ContainerParameterReference,
    string TransientScopeReference,
    string TransientScopeParameterReference,
    string Name,
    IMethodSymbol? AddForDisposal,
    IMethodSymbol? AddForDisposalAsync) 
    : RangeResolution(
        CreateFunctions, 
        DisposalHandling, 
        RangedInstanceFunctionGroups, 
        ContainerReference,
        AddForDisposal,
        AddForDisposalAsync);

internal record TransientScopeResolution(
    IReadOnlyList<CreateFunctionResolution> CreateFunctions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstanceFunctionGroupResolution> RangedInstanceFunctionGroups,
    string ContainerReference,
    string ContainerParameterReference,
    string Name,
    IMethodSymbol? AddForDisposal,
    IMethodSymbol? AddForDisposalAsync) 
    : RangeResolution(
        CreateFunctions, 
        DisposalHandling, 
        RangedInstanceFunctionGroups, 
        ContainerReference,
        AddForDisposal,
        AddForDisposalAsync);

internal record ContainerResolution(
    IReadOnlyList<CreateFunctionResolution> CreateFunctions,
    DisposalHandling DisposalHandling,
    IReadOnlyList<RangedInstanceFunctionGroupResolution> RangedInstanceFunctionGroups,
    TransientScopeInterfaceResolution TransientScopeInterface,
    string TransientScopeAdapterReference,
    IReadOnlyList<TransientScopeResolution> TransientScopes,
    IReadOnlyList<ScopeResolution> Scopes,
    DisposalType DisposalType,
    string TransientScopeDisposalReference,
    string TransientScopeDisposalElement,
    NopDisposable NopDisposable,
    NopAsyncDisposable NopAsyncDisposable,
    SyncToAsyncDisposable SyncToAsyncDisposable,
    IMethodSymbol? AddForDisposal,
    IMethodSymbol? AddForDisposalAsync)
    : RangeResolution(
        CreateFunctions,
        DisposalHandling, 
        RangedInstanceFunctionGroups, 
        Constants.ThisKeyword,
        AddForDisposal,
        AddForDisposalAsync);

internal record NopDisposable(
    string ClassName,
    string InstanceReference);

internal record NopAsyncDisposable(
    string ClassName,
    string InstanceReference);

internal record SyncToAsyncDisposable(
    string ClassName,
    string DisposableParameterReference,
    string DisposableFieldReference);

internal record DisposalHandling(
    SyncDisposableCollectionResolution SyncDisposableCollection,
    AsyncDisposableCollectionResolution AsyncDisposableCollection,
    string RangeName,
    string DisposedFieldReference,
    string DisposedLocalReference,
    string DisposedPropertyReference,
    string DisposableLocalReference);

internal record NullResolution(
    string Reference,
    string TypeFullName) : Resolvable(Reference, TypeFullName);

internal record FieldResolution(
    string Reference,
    string TypeFullName,
    string FieldName,
    bool Await) : Resolvable(Reference, TypeFullName);

internal abstract record TaskBaseResolution(
    Resolvable WrappedResolvable,
    string TaskReference,
    string TaskFullName) : Resolvable(TaskReference, TaskFullName);

internal record TaskFromTaskResolution(
    Resolvable WrappedResolvable,
    TaskInitializationResolution Initialization,
    string TaskReference,
    string TaskFullName) : TaskBaseResolution(WrappedResolvable, TaskReference, TaskFullName);
internal record TaskFromValueTaskResolution(
    Resolvable WrappedResolvable,
    ValueTaskInitializationResolution Initialization,
    string TaskReference,
    string TaskFullName) : TaskBaseResolution(WrappedResolvable, TaskReference, TaskFullName);
internal record TaskFromSyncResolution(
    Resolvable WrappedResolvable,
    string TaskReference,
    string TaskFullName) : TaskBaseResolution(WrappedResolvable, TaskReference, TaskFullName), IFactoryResolution;
    
internal record ValueTaskFromTaskResolution(
    Resolvable WrappedResolvable,
    TaskInitializationResolution Initialization,
    string ValueTaskReference,
    string ValueTaskFullName) : TaskBaseResolution(WrappedResolvable, ValueTaskReference, ValueTaskFullName);
internal record ValueTaskFromValueTaskResolution(
    Resolvable WrappedResolvable,
    ValueTaskInitializationResolution Initialization,
    string ValueTaskReference,
    string ValueTaskFullName) : TaskBaseResolution(WrappedResolvable, ValueTaskReference, ValueTaskFullName);
internal record ValueTaskFromSyncResolution(
    Resolvable WrappedResolvable,
    string ValueTaskReference,
    string ValueTaskFullName) : TaskBaseResolution(WrappedResolvable, ValueTaskReference, ValueTaskFullName), IFactoryResolution;
    
internal record TaskFromWrappedValueTaskResolution(
    Resolvable WrappedResolvable,
    string Reference,
    string FullName) : Resolvable(Reference, FullName);
    
internal record ValueTaskFromWrappedTaskResolution(
    Resolvable WrappedResolvable,
    string Reference,
    string FullName) : Resolvable(Reference, FullName);

internal record MultiTaskResolution(
    Resolvable Sync,
    Resolvable AsyncTask,
    Resolvable AsyncValueTask,
    Lazy<SynchronicityDecision> LazySynchronicityDecision) : Resolvable(Sync.Reference, "")
{
    internal Resolvable SelectedResolvable =>
        LazySynchronicityDecision.Value switch
        {
            SynchronicityDecision.Sync => Sync,
            SynchronicityDecision.AsyncTask => AsyncTask,
            SynchronicityDecision.AsyncValueTask => AsyncValueTask,
            _ => throw new ArgumentException("Synchronicity not decided yet")
        };
}

internal interface IFactoryResolution {}

internal record AsyncFactoryCallResolution(
    string Reference,
    string TypeFullName,
    MultiSynchronicityFunctionCallResolution FunctionCall,
    SynchronicityDecision FirstWrapping) : Resolvable(Reference, TypeFullName), IFactoryResolution
{

    internal Resolvable Inner
    {
        get
        {
            FunctionCall.SelectedFunctionCall.Await = false;
            switch (FunctionCall.LazySynchronicityDecision.Value, FirstWrapping)
            {
                case (SynchronicityDecision.Sync, SynchronicityDecision.AsyncValueTask):
                    return new ValueTaskFromSyncResolution(FunctionCall.SelectedFunctionCall, Reference, TypeFullName);
                case (SynchronicityDecision.Sync, SynchronicityDecision.AsyncTask):
                    return new TaskFromSyncResolution(FunctionCall.SelectedFunctionCall, Reference, TypeFullName);
                case (SynchronicityDecision.AsyncTask, SynchronicityDecision.AsyncValueTask):
                    return new ValueTaskFromWrappedTaskResolution(FunctionCall.SelectedFunctionCall, Reference, TypeFullName);
                case (SynchronicityDecision.AsyncTask, SynchronicityDecision.AsyncTask):
                    return new NewReferenceResolvable(Reference, TypeFullName, FunctionCall.SelectedFunctionCall);
                case (SynchronicityDecision.AsyncValueTask, SynchronicityDecision.AsyncValueTask):
                    return new NewReferenceResolvable(Reference, TypeFullName, FunctionCall.SelectedFunctionCall);
                case (SynchronicityDecision.AsyncValueTask, SynchronicityDecision.AsyncTask):
                    return new TaskFromWrappedValueTaskResolution(FunctionCall.SelectedFunctionCall, Reference, TypeFullName);
                default:
                    throw new ArgumentOutOfRangeException(); // todo
            }
        }
    }
};

internal record MultiSynchronicityFunctionCallResolution(
    FunctionCallResolution Sync,
    FunctionCallResolution AsyncTask,
    FunctionCallResolution AsyncValueTask,
    Lazy<SynchronicityDecision> LazySynchronicityDecision,
    FunctionResolutionBuilderHandle FunctionResolutionBuilderHandle)
    : Resolvable(Sync.Reference, ""), IAwaitableResolution, ITaskConsumableResolution, IFactoryResolution
{
    internal FunctionCallResolution SelectedFunctionCall =>
        LazySynchronicityDecision.Value switch
        {
            SynchronicityDecision.Sync => Sync,
            SynchronicityDecision.AsyncTask => AsyncTask,
            SynchronicityDecision.AsyncValueTask => AsyncValueTask,
            _ => throw new ArgumentException("Synchronicity not decided yet")
        };
    
    public bool Await => SelectedFunctionCall.Await;
}

internal record FunctionCallResolution(
    string Reference,
    string TypeFullName,
    string OriginalTypeFullName,
    string FunctionReference,
    string? OwnerReference,
    IReadOnlyList<(string Name, string Reference)> Parameters)
    : Resolvable(Reference, TypeFullName), IAwaitableResolution
{
    public bool Await { get; set; } = true;

    public string SelectedTypeFullName => Await ? OriginalTypeFullName : TypeFullName;
}