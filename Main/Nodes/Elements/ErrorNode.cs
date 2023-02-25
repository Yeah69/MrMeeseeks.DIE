using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IErrorNode : IElementNode
{
    string Message { get; }
}

internal class ErrorNode : IErrorNode
{
    private readonly IDiagLogger _diagLogger;

    internal ErrorNode(
        string message,
        IDiagLogger diagLogger)
    {
        _diagLogger = diagLogger;
        Message = message;
    }
    
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        _diagLogger.Error(new ResolutionDieException(Message), ExecutionPhase.ResolutionBuilding);
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitErrorNode(this);

    public string Message { get; }
    public string TypeFullName => "Errors have no type";
    public string Reference => "Errors have no reference";
}