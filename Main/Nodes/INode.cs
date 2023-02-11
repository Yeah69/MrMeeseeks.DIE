using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes;

internal interface INode
{
    void Build(ImmutableStack<INamedTypeSymbol> implementationStack);
    void Accept(INodeVisitor nodeVisitor);
}