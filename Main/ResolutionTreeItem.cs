using System.Collections.Generic;

namespace MrMeeseeks.DIE
{
    internal abstract record ResolutionTreeItem;

    internal record ErrorTreeItem(
        string Message) : ResolutionTreeItem;
    
    internal abstract record Resolvable(
        string Reference,
        string TypeFullName) : ResolutionTreeItem; 

    internal record InterfaceResolution(
        string Reference,
        string TypeFullName,
        ResolutionTreeItem Dependency) : Resolvable(Reference, TypeFullName);

    internal record ConstructorResolution(
        string Reference,
        string TypeFullName,
        IReadOnlyList<(string name, ResolutionTreeItem Dependency)> Parameter) : Resolvable(Reference, TypeFullName);

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
}
