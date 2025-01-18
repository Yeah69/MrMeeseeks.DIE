using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteFunctorNodeData(INamedTypeSymbol Type)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteFunctorNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Type, other.Type))
            return false;
        return true;
    }
}

internal class ConcreteFunctorNodeManager(Func<ConcreteFunctorNodeData, ConcreteFunctorNode> factory)
    : ConcreteNodeManagerBase<ConcreteFunctorNodeData, ConcreteFunctorNode>(factory), IContainerInstance;

internal enum ConcreteFunctorNodeType { Func, Lazy, ThreadLocal }

internal class ConcreteFunctorNode : IConcreteNode
{
    internal ConcreteFunctorNode(
        // parameters
        ConcreteFunctorNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory,
        WellKnownTypes wellKnownTypes)
    {
        Data = data;

        ITypeSymbol returnedType;
        if (data.Type.FullName().StartsWith("global::System.Func<", StringComparison.Ordinal))
        {
            FunctorType = ConcreteFunctorNodeType.Func;
            FunctorParameterTypes = data.Type.TypeArguments.Take(data.Type.TypeArguments.Length - 1).ToArray();
            returnedType = data.Type.TypeArguments.Last();
        }
        else if (CustomSymbolEqualityComparer.Default.Equals(data.Type.OriginalDefinition, wellKnownTypes.Lazy1))
        {
            FunctorType = ConcreteFunctorNodeType.Lazy;
            FunctorParameterTypes = [];
            returnedType = data.Type.TypeArguments.Single();
        }
        else if (CustomSymbolEqualityComparer.Default.Equals(data.Type.OriginalDefinition, wellKnownTypes.ThreadLocal1))
        {
            FunctorType = ConcreteFunctorNodeType.ThreadLocal;
            FunctorParameterTypes = [];
            returnedType = data.Type.TypeArguments.Single();
        }
        else
            throw new ArgumentException("Invalid functor type");
        ReturnedElement = typeEdgeFactory(this, typeNodeManager.GetOrAddNode(returnedType));
    }
    internal ConcreteFunctorNodeData Data { get; }
    internal ConcreteFunctorNodeType FunctorType { get; }
    internal IReadOnlyList<ITypeSymbol> FunctorParameterTypes { get; }
    internal TypeEdge ReturnedElement { get; }
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteFunctorNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context) => 
        ReturnedElement.AddContext(context) 
            ? [(ReturnedElement.Target, Location.None)] 
            : [];
}