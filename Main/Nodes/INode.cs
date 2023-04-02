namespace MrMeeseeks.DIE.Nodes;

internal partial interface INode
{
    void Build(ImmutableStack<INamedTypeSymbol> implementationStack);
}