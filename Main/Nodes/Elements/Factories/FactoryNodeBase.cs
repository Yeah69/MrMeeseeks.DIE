using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryNodeBase : IElementNode
{
    string Name { get; }
    string? AsyncTypeFullName { get; }
    bool Awaited { get; }
}

internal abstract class FactoryNodeBase : IFactoryNodeBase
{
    internal FactoryNodeBase(
        // parameters
        ITypeSymbol referenceType,
        ISymbol symbol,
        
        // dependencies
        IFunctionNode parentFunction,
        ITaskBasedQueue taskBasedQueue,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        Name = symbol.Name;
        Reference = referenceGenerator.Generate(referenceType);
        TypeFullName = referenceType.FullName();
        
        if ((wellKnownTypes.ValueTask1 is not null && CustomSymbolEqualityComparer.IncludeNullability.Equals(referenceType.OriginalDefinition, wellKnownTypes.ValueTask1)
             || CustomSymbolEqualityComparer.IncludeNullability.Equals(referenceType.OriginalDefinition, wellKnownTypes.Task1))
            && referenceType is INamedTypeSymbol namedReferenceType)
        {
            Awaited = true;
            AsyncTypeFullName = namedReferenceType.TypeArguments.First().FullName();
            taskBasedQueue.EnqueueTaskBasedOnlyFunction(parentFunction);
        }
    }
    
    public virtual void Build(PassedContext passedContext)
    {
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    public string Name { get; }
    public bool Awaited { get; }
    public string? AsyncTypeFullName { get; }
}