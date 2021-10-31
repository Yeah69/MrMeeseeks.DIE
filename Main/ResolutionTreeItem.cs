using System;
using System.Collections.Generic;

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

    internal record FuncParameterResolution(
        string Reference,
        string TypeFullName) : Resolvable(Reference, TypeFullName);

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
        DisposableCollectionResolution DisposableCollection) : Resolvable(RootResolution.Reference, RootResolution.TypeFullName);
}
