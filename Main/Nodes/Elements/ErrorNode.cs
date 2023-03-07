using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IErrorNode : IElementNode
{
    string Message { get; }
}

internal class ErrorNode : IErrorNode
{
    private readonly ITypeSymbol _currentType;
    private readonly IRangeNode _parentRange;
    private readonly IDiagLogger _diagLogger;

    internal ErrorNode(
        string message,
        ITypeSymbol currentType,
        ITransientScopeWideContext transientScopeWideContext,
        IDiagLogger diagLogger)
    {
        _currentType = currentType;
        _parentRange = transientScopeWideContext.Range;
        _diagLogger = diagLogger;
        Message = message;
    }
    
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        var enhancedMessage = 
            $"[R:{_parentRange.Name
            }][TS:{(implementationStack.IsEmpty ? "empty" : implementationStack.Peek().FullName())
            }][CT:{_currentType.FullName()}] {Message} [S:{
                (implementationStack.IsEmpty ? "empty" : string.Join("<==", implementationStack.Select(t => t.FullName())))}]";
        _diagLogger.Error(new ResolutionDieException(enhancedMessage), ExecutionPhase.ResolutionBuilding);
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitErrorNode(this);

    public string Message { get; }
    public string TypeFullName => "Errors have no type";
    public string Reference => "Errors have no reference";
}