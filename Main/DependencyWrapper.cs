using System.Collections.Generic;

namespace MrMeeseeks.DIE
{
    internal abstract record ResolutionBase(
        string Reference,
        string TypeFullName); 

    internal record InterfaceResolution(
        string Reference,
        string TypeFullName,
        ResolutionBase Dependency) : ResolutionBase(Reference, TypeFullName);

    internal record ConstructorResolution(
        string Reference,
        string TypeFullName,
        IReadOnlyList<(string name, ResolutionBase Dependency)> Parameter) : ResolutionBase(Reference, TypeFullName);

    internal record FuncParameterResolution(
        string Reference,
        string TypeFullName) : ResolutionBase(Reference, TypeFullName);

    internal record FuncResolution(
        string Reference,
        string TypeFullName,
        IReadOnlyList<FuncParameterResolution> Parameter,
        ResolutionBase Dependency) : ResolutionBase(Reference, TypeFullName);

    internal record CollectionResolution(
        string Reference,
        string TypeFullName,
        string ItemFullName,
        IReadOnlyList<ResolutionBase> Parameter) : ResolutionBase(Reference, TypeFullName);
}
