using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements.Factories;

internal interface IFactoryNodeBase : IElementNode, IPotentiallyAwaitedNode
{
    string Name { get; }
}

internal abstract class FactoryNodeBase : IFactoryNodeBase
{
    private readonly ITypeSymbol _referenceType;
    private readonly IFunctionNode _parentFunction;
    private readonly WellKnownTypes _wellKnownTypes;

    internal FactoryNodeBase(
        ITypeSymbol referenceType,
        ISymbol symbol,
        IFunctionNode parentFunction,
        IReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _referenceType = referenceType;
        _parentFunction = parentFunction;
        _wellKnownTypes = wellKnownTypes;
        Name = symbol.Name;
        Reference = referenceGenerator.Generate(referenceType);
        TypeFullName = referenceType.FullName();
    }
    
    public virtual void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        if ((CustomSymbolEqualityComparer.IncludeNullability.Equals(_referenceType.OriginalDefinition, _wellKnownTypes.ValueTask1)
             || CustomSymbolEqualityComparer.IncludeNullability.Equals(_referenceType.OriginalDefinition, _wellKnownTypes.Task1))
            && _referenceType is INamedTypeSymbol namedReferenceType)
        {
            Awaited = true;
            AsyncReference = Reference;
            SynchronicityDecision = CustomSymbolEqualityComparer.IncludeNullability.Equals(_referenceType.OriginalDefinition, _wellKnownTypes.ValueTask1)
                ? SynchronicityDecision.AsyncValueTask
                : SynchronicityDecision.AsyncTask;
            AsyncTypeFullName = namedReferenceType.TypeArguments.First().FullName();
            _parentFunction.OnAwait(this);
        }
    }

    public abstract void Accept(INodeVisitor nodeVisitor);

    public string TypeFullName { get; }
    public string Reference { get; }
    public string Name { get; }
    public bool Awaited { get; set; }
    public string? AsyncReference { get; private set; }
    public string? AsyncTypeFullName { get; private set; }
    public SynchronicityDecision SynchronicityDecision { get; private set; } = SynchronicityDecision.Sync;
}