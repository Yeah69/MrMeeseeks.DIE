using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MrMeeseeks.DIE
{
    internal enum ResolutionStage
    {
        Prefix,
        Postfix
    }

    internal record DependencyWrapper(
        ResolutionStage ResolutionStage, 
        int Id, 
        INamedTypeSymbol InjectedType, 
        INamedTypeSymbol ImplementationType,
        string ImplementationTypeFullName,
        IReadOnlyList<int> ParameterIds);

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
        IReadOnlyList<ResolutionBase> Dependencies) : ResolutionBase(Reference, TypeFullName);
}
