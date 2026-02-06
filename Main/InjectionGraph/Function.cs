using MrMeeseeks.DIE.InjectionGraph.Nodes;

namespace MrMeeseeks.DIE.InjectionGraph;

internal interface IFunction
{
    Accessibility? Accessibility { get; }
    ExplicitInterfaceDescription ExplicitInterface { get; }
    ITypeParameterSymbol[] TypeParameters { get; }
    ITypeSymbol ReturnType { get; }
    bool IsAsync { get; }
}

internal interface ITypeNodeFunction : IFunction
{
    TypeNode RootNode { get; }
}

internal abstract record ExplicitInterfaceDescription
{
    internal sealed record None4 : ExplicitInterfaceDescription
    {
        private None4() {}
        internal static None4 Instance { get; } = new();
    }
    internal sealed record KnownType(INamedTypeSymbol Type) : ExplicitInterfaceDescription;
    internal sealed record Generated(string TypeFullName) : ExplicitInterfaceDescription;
}

internal sealed class TypeNodeFunction(TypeNode rootElement) : ITypeNodeFunction
{
    public Accessibility? Accessibility => Microsoft.CodeAnalysis.Accessibility.Private;
    public ExplicitInterfaceDescription ExplicitInterface => ExplicitInterfaceDescription.None4.Instance;
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public ITypeSymbol ReturnType => RootNode.Type;
    public TypeNode RootNode { get; } = rootElement;
    public bool IsAsync { get; }
}

internal sealed class FunctorEntryFunction(ITypeSymbol returnType) : IFunction
{
    public Accessibility? Accessibility => Microsoft.CodeAnalysis.Accessibility.Private;
    public ExplicitInterfaceDescription ExplicitInterface => ExplicitInterfaceDescription.None4.Instance;
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public ITypeSymbol ReturnType => returnType;
    public bool IsAsync { get; }
}

internal sealed class ScopedInstanceFunction(TypeNode scopedInstanceElement) : ITypeNodeFunction
{
    public Accessibility? Accessibility => null;
    public required ExplicitInterfaceDescription ExplicitInterface { get; init; }
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public ITypeSymbol ReturnType => RootNode.Type;
    public TypeNode RootNode { get; } = scopedInstanceElement;
    public bool IsAsync { get; }
}

internal sealed class ScopeRootFunction(TypeNode scopeRootElement) : ITypeNodeFunction
{
    public Accessibility? Accessibility => Microsoft.CodeAnalysis.Accessibility.Internal;
    public ExplicitInterfaceDescription ExplicitInterface => ExplicitInterfaceDescription.None4.Instance;
    public ITypeParameterSymbol[] TypeParameters { get; } = [];
    public ITypeSymbol ReturnType => RootNode.Type;
    public TypeNode RootNode { get; } = scopeRootElement;
    public bool IsAsync { get; }
}