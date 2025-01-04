using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IFunction
{
    Accessibility? Accessibility { get; }
    INamedTypeSymbol? ExplicitInterface { get; }
    ITypeParameterSymbol[] TypeParameters { get; }
    TypeNode RootNode { get; }
    bool IsAsync { get; }
}

internal class Function(TypeNode rootElement) : IFunction
{
    public Accessibility? Accessibility { get; init; }
    public INamedTypeSymbol? ExplicitInterface { get; init; }
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public TypeNode RootNode { get; } = rootElement;
    public bool IsAsync { get; }
}