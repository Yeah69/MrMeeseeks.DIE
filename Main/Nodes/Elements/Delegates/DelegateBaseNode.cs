using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements.Delegates;

internal interface IDelegateBaseNode : IElementNode
{
    string MethodGroup { get; }
}

internal abstract class DelegateBaseNode : IDelegateBaseNode
{
    internal DelegateBaseNode(
        INamedTypeSymbol delegateType,
        ILocalFunctionNode function,
        IReferenceGenerator referenceGenerator)
    {
        MethodGroup = function.Name;
        Reference = referenceGenerator.Generate(delegateType);
        TypeFullName = delegateType.FullName();
    }

    public void Build()
    {
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    
    public string MethodGroup { get; }
}