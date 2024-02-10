using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;

internal interface IFunctionCallNode : IElementNode, IAwaitableNode
{
    string? OwnerReference { get; }
    string FunctionName { get; }
    IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    IFunctionNode CalledFunction { get; }
}

internal abstract class FunctionCallNode : IFunctionCallNode
{
    private readonly IFunctionNode _calledFunction;

    public FunctionCallNode(
        string? ownerReference,
        IFunctionNode calledFunction,
        ITypeSymbol callSideType,
        IReadOnlyList<(IParameterNode, IParameterNode)> parameters,
        IReadOnlyList<ITypeSymbol> typeParameters,
        
        IReferenceGenerator referenceGenerator)
    {
        _calledFunction = calledFunction;
        OwnerReference = ownerReference;
        Parameters = parameters;
        TypeParameters = typeParameters;
        FunctionName = calledFunction.Name;
        TypeFullName = callSideType.FullName();
        Reference = referenceGenerator.Generate("functionCallResult");
    }

    public virtual void Build(PassedContext passedContext) { }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    public string FunctionName { get; }
    public virtual string? OwnerReference { get; }
    public IReadOnlyList<(IParameterNode, IParameterNode)> Parameters { get; }
    public IReadOnlyList<ITypeSymbol> TypeParameters { get; }
    public IFunctionNode CalledFunction => _calledFunction;

    public bool Awaited => _calledFunction.SynchronicityDecision is not SynchronicityDecision.Sync;
}