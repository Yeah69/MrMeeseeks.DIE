using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
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

    internal record SingleInstance(
        SingleInstanceFunction Function,
        Resolvable Dependency);

    internal record FuncParameterResolution(
        string Reference,
        string TypeFullName) : Resolvable(Reference, TypeFullName);

    internal record SingleInstanceFunction(
        string Reference,
        string TypeFullName,
        INamedTypeSymbol Type,
        string FieldReference,
        string LockReference);

    internal record SingleInstanceReferenceResolution(
        string Reference,
        SingleInstanceFunction Function) : Resolvable(Reference, Function.TypeFullName);

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

    internal record ContainerResolution(
        Resolvable RootResolution,
        ContainerResolutionDisposalHandling DisposalHandling,
        IReadOnlyList<SingleInstance> SingleInstanceResolutions) : Resolvable(RootResolution.Reference, RootResolution.TypeFullName);

    internal record ContainerResolutionDisposalHandling(
        DisposableCollectionResolution DisposableCollection,
        string DisposedFieldReference,
        string DisposedLocalReference,
        string DisposedPropertyReference,
        string DisposableLocalReference);
}
