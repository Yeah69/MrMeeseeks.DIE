using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IFunction
{
    Accessibility? Accessibility { get; }
    INamedTypeSymbol? ExplicitInterface { get; }
    ITypeParameterSymbol[] TypeParameters { get; }
    ITypeSymbol ReturnType { get; }
    bool IsAsync { get; }
}

internal interface ITypeNodeFunction : IFunction
{
    TypeNode RootNode { get; }
}

internal class TypeNodeFunction(TypeNode rootElement) : ITypeNodeFunction
{
    public Accessibility? Accessibility { get; init; }
    public INamedTypeSymbol? ExplicitInterface { get; init; }
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public ITypeSymbol ReturnType => RootNode.Type;
    public TypeNode RootNode { get; } = rootElement;
    public bool IsAsync { get; }
}

internal class FunctorEntryFunction(ITypeSymbol returnType) : IFunction
{
    public Accessibility? Accessibility { get; init; }
    public INamedTypeSymbol? ExplicitInterface { get; init; }
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public ITypeSymbol ReturnType => returnType;
    public bool IsAsync { get; }
}