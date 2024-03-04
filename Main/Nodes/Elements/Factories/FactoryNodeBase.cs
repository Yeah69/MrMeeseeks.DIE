using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryNodeBase : IElementNode, IAwaitableNode
{
    string Name { get; }
    string? AsyncTypeFullName { get; }
}

internal abstract class FactoryNodeBase : IFactoryNodeBase
{
    private readonly IFunctionNode _parentFunction;

    internal FactoryNodeBase(
        ITypeSymbol referenceType,
        ISymbol symbol,
        
        IFunctionNode parentFunction,
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext)
    {
        _parentFunction = parentFunction;
        var wellKnownTypes = containerWideContext.WellKnownTypes;
        Name = symbol.Name;
        Reference = referenceGenerator.Generate(referenceType);
        TypeFullName = referenceType.FullName();
        
        if ((wellKnownTypes.ValueTask1 is not null && CustomSymbolEqualityComparer.IncludeNullability.Equals(referenceType.OriginalDefinition, wellKnownTypes.ValueTask1)
             || CustomSymbolEqualityComparer.IncludeNullability.Equals(referenceType.OriginalDefinition, wellKnownTypes.Task1))
            && referenceType is INamedTypeSymbol namedReferenceType)
        {
            Awaited = true;
            AsyncTypeFullName = namedReferenceType.TypeArguments.First().FullName();
        }
    }
    
    public virtual void Build(PassedContext passedContext)
    {
        _parentFunction.RegisterAwaitableNode(this);
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    public string Name { get; }
    public bool Awaited { get; }
    public string? AsyncTypeFullName { get; }
}