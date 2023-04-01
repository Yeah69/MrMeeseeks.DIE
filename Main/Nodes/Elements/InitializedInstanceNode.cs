using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IInitializedInstanceNode : IElementNode
{
    bool IsReferenceType { get; }
    IFunctionCallNode BuildCall(IRangeNode range, IFunctionNode callingFunction);
}

internal class InitializedInstanceNode : IInitializedInstanceNode
{
    private readonly INamedTypeSymbol _type;

    internal InitializedInstanceNode(
        INamedTypeSymbol type,
        
        IReferenceGenerator referenceGenerator)
    {
        _type = type;
        TypeFullName = type.FullName();
        Reference = referenceGenerator.Generate("_initialized", type);
        IsReferenceType = type.IsReferenceType;
    }

    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitIInitializedInstanceNode(this);

    public string TypeFullName { get; }
    public string Reference { get; }
    public bool IsReferenceType { get; }
    
    public IFunctionCallNode BuildCall(IRangeNode range, IFunctionNode callingFunction) => 
        range.BuildCreateCall(_type, callingFunction);
}