using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal record ConcreteImplementationNodeData(
    INamedTypeSymbol Implementation,
    IMethodSymbol Constructor,
    IReadOnlyList<IPropertySymbol> ObjectInitializerProperties)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Implementation, CustomSymbolEqualityComparer.IncludeNullability);
        hash.Add(Constructor, CustomSymbolEqualityComparer.IncludeNullability);
        foreach (var property in ObjectInitializerProperties)
            hash.Add(property, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ConcreteImplementationNodeData? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Implementation, other.Implementation))
            return false;
        if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(Constructor, other.Constructor))
            return false;
        if (ObjectInitializerProperties.Count != other.ObjectInitializerProperties.Count)
            return false;
        for (var i = 0; i < ObjectInitializerProperties.Count; i++)
            if (!CustomSymbolEqualityComparer.IncludeNullability.Equals(ObjectInitializerProperties[i], other.ObjectInitializerProperties[i]))
                return false;
        return true;
    }
}

internal class ConcreteImplementationNodeManager(Func<ConcreteImplementationNodeData, ConcreteImplementationNode> factory)
    : ConcreteNodeManagerBase<ConcreteImplementationNodeData, ConcreteImplementationNode>(factory), IContainerInstance;

internal class ConcreteImplementationNode : IConcreteNode
{
    internal record Dependency(string Name, TypeEdge Edge, Location Location, ITypeSymbol Type)
    {
        internal int? PassOriginalChoiceContextId { get; set; }
    }
    
    private readonly IdRegister _idRegister;
    private readonly TypeSymbolUtility _typeSymbolUtility;

    internal ConcreteImplementationNode(
        // parameters
        ConcreteImplementationNodeData data,

        // dependencies
        TypeNodeManager typeNodeManager,
        IdRegister idRegister,
        TypeSymbolUtility typeSymbolUtility,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        _idRegister = idRegister;
        _typeSymbolUtility = typeSymbolUtility;
        Data = data;

        var constructorParameters = data.Constructor.Parameters
            .Select(p => new Dependency(
                p.Name,
                typeEdgeFactory(this, typeNodeManager.GetOrAddNode(p.Type)),
                p.Locations.FirstOrDefault() ?? Location.None,
                p.Type));
        ConstructorParameters = [..constructorParameters];
        var objectInitializerAssignments = data.ObjectInitializerProperties
            .Select(p => new Dependency(
                p.Name, 
                typeEdgeFactory(this, typeNodeManager.GetOrAddNode(p.Type)),
                p.Locations.FirstOrDefault() ?? Location.None,
                p.Type));
        ObjectInitializerAssignments = [..objectInitializerAssignments];
    }
    internal ConcreteImplementationNodeData Data { get; }
    internal ImmutableArray<Dependency> ConstructorParameters { get; }
    internal ImmutableArray<Dependency> ObjectInitializerAssignments { get; }
    private IEnumerable<Dependency> AllDependencies => ConstructorParameters.Concat(ObjectInitializerAssignments);
    
    internal bool NeedsPurge { get; private set; }
    internal bool NeedsOriginalContextReference => AllDependencies.Any(dep => dep.PassOriginalChoiceContextId is not null);
    
    public override int GetHashCode() => 
        Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteImplementationNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location, EdgeContext Context)> ConnectIfNotAlready(EdgeContext context)
    {
        var originalContext = context;
        context = context.CaseChoice is CaseChoiceContext.None
            ? context
            : context with { CaseChoice = new CaseChoiceContext.None() };
        if (originalContext.CaseChoice is not CaseChoiceContext.None)
            NeedsPurge = true;
        var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, Location Location, EdgeContext Context)>();
        foreach (var dependency in ConstructorParameters.Concat(ObjectInitializerAssignments))
        {
            var pickedContext = context;
            if (originalContext.CaseChoice is CaseChoiceContext.Single { OutwardFacingTypeId: var outwardFacingTypeId }
                && _idRegister.GetOutwardFacingTypeId(_typeSymbolUtility.GetUnwrappedType(dependency.Type)) == outwardFacingTypeId)
            {
                pickedContext = originalContext;
                dependency.PassOriginalChoiceContextId = outwardFacingTypeId;
            }
            
            if (dependency.Edge.AddContext(pickedContext))
                notYetConnectedTypeNodes.Add((dependency.Edge.Target, dependency.Location, pickedContext));
        }
        return notYetConnectedTypeNodes;
    }
}