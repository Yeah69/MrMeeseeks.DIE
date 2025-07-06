using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteInterfaceNodeData(INamedTypeSymbol Interface)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Interface, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteInterfaceNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Interface, other.Interface))
            return false;
        return true;
    }
}

internal record ConcreteInterfaceNodeImplementationData(INamedTypeSymbol Implementation, IReadOnlyList<Decoration> Decorations)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Implementation, CustomSymbolEqualityComparer.IncludeNullability);
        foreach (var decoration in Decorations)
            hash.Add(decoration);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteInterfaceNodeImplementationData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Implementation, other.Implementation))
            return false;
        if (Decorations.Count != other.Decorations.Count)
            return false;
        for (var i = 0; i < Decorations.Count; i++)
        {
            switch (Decorations[i], other.Decorations[i])
            {
                case (Decoration.Decorator, Decoration.Interceptor):
                case (Decoration.Interceptor, Decoration.Decorator):
                case (Decoration.Decorator decorator, Decoration.Decorator otherDecorator) 
                    when !CustomSymbolEqualityComparer.IncludeNullability.Equals(decorator.Type, otherDecorator.Type):
                case (Decoration.Interceptor interceptor, Decoration.Interceptor otherInterceptor) 
                    when !CustomSymbolEqualityComparer.IncludeNullability.Equals(interceptor.Type, otherInterceptor.Type):
                    return false;
            }
        }
        return true;
    }
}

internal class ConcreteInterfaceNodeManager(Func<ConcreteInterfaceNodeData, ConcreteInterfaceNode> factory)
    : ConcreteNodeManagerBase<ConcreteInterfaceNodeData, ConcreteInterfaceNode>(factory), IContainerInstance;

internal class ConcreteInterfaceNode : IConcreteNode
{
    private readonly IdRegister _idRegister;
    private readonly TypeNodeManager _typeNodeManager;
    private readonly Func<IConcreteNode, TypeNode, TypeEdge> _typeEdgeFactory;
    private readonly Dictionary<DomainContext, int> _defaultImplementationsCaseNumbers = [];
    private readonly Dictionary<int, ImmutableArray<(TypeEdge Edge, int Id, int NextId)>> _implementationDataToCases = [];
    private readonly Dictionary<(DomainContext Domain, object KeyObject), int> _defaultKeyObjectToCaseNumbers = [];

    internal ConcreteInterfaceNode(
        // parameters
        ConcreteInterfaceNodeData data,

        // dependencies
        IdRegister idRegister,
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        _idRegister = idRegister;
        _typeNodeManager = typeNodeManager;
        _typeEdgeFactory = typeEdgeFactory;
        Data = data;
        Number = idRegister.GetOutwardFacingTypeId(data.Interface);
    }
    internal int Number { get; }
    internal ConcreteInterfaceNodeData Data { get; }
    internal IEnumerable<(TypeEdge Edge, int Id, int NextId)> Cases => _implementationDataToCases.SelectMany(kvp => kvp.Value);
    internal IReadOnlyDictionary<DomainContext, int> DefaultImplementationsCaseNumbers => _defaultImplementationsCaseNumbers;
    internal IReadOnlyDictionary<(DomainContext Domain, object KeyObject), int> KeyObjectToCaseNumbers => _defaultKeyObjectToCaseNumbers;
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteInterfaceNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(
        EdgeContext context, 
        ConcreteInterfaceNodeImplementationData implementationData,
        bool isDefaultInjection,
        object? keyObject)
    {
        var initialCaseId = _idRegister.GetInitialCaseId(context.Domain, implementationData.Implementation);
        if (!_implementationDataToCases.TryGetValue(initialCaseId, out var cases))
        {
            var ids = Enumerable.Range(0, implementationData.Decorations.Count)
                .Select(_ => _idRegister.GetUnusedCaseId())
                .Prepend(initialCaseId)
                .ToImmutableArray();
            var tempCases = new (TypeEdge Edge, int Id, int NextId)[ids.Length];
            foreach (var (decoration, i) in implementationData.Decorations.Select((d, i) => (d, i)))
            {
                var decorationTypeEdge = _typeEdgeFactory(this, _typeNodeManager.GetOrAddNode(decoration switch
                    {
                        Decoration.Decorator decorator => decorator.Type,
                        Decoration.Interceptor interceptor => interceptor.Type,
                        _ => throw new ArgumentException("Unknown decoration type")
                    }));
                tempCases[i] = (decorationTypeEdge, ids[i], ids[i + 1]);
            }
            var implementationTypeEdge = _typeEdgeFactory(this, _typeNodeManager.GetOrAddNode(implementationData.Implementation));
            tempCases[^1] = (implementationTypeEdge, ids[^1], 0);
            cases = [..tempCases];
            _implementationDataToCases[initialCaseId] = cases;
        }
        
        if (keyObject is not null)
        {
            var keyObjectTuple = (context.Domain, keyObject);
            if (!_defaultKeyObjectToCaseNumbers.TryGetValue(keyObjectTuple, out var caseNumber))
            {
                caseNumber = cases[0].Id;
                _defaultKeyObjectToCaseNumbers[keyObjectTuple] = caseNumber;
            }
        }
        else if (isDefaultInjection) 
            _defaultImplementationsCaseNumbers[context.Domain] = cases[0].Id;
        
        var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, Location Location)>();
        foreach (var (edge, _, _) in cases)
            if (edge.AddContext(context))
                notYetConnectedTypeNodes.Add((edge.Target, Location.None));
        return notYetConnectedTypeNodes;
    }
}