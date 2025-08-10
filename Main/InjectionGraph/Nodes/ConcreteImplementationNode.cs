using MrMeeseeks.DIE.Configuration;
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
    private readonly IdRegister _idRegister;
    private readonly TypeSymbolUtility _typeSymbolUtility;
    private readonly bool _isContextPassing;

    internal record Dependency(string Name, TypeEdge Edge, Location Location, ITypeSymbol Type)
    {
        internal int? ContextSwitchOutwardFacingTypeId { get; set; }
    }

    internal ConcreteImplementationNode(
        // parameters
        ConcreteImplementationNodeData data,

        // dependencies
        IContainerCheckTypeProperties containerCheckTypeProperties,
        TypeNodeManager typeNodeManager,
        IdRegister idRegister,
        TypeSymbolUtility typeSymbolUtility,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        _idRegister = idRegister;
        _typeSymbolUtility = typeSymbolUtility;
        Data = data;
        
        ConstructorParameters = [..data.Constructor.Parameters.Select(Dependency)];
        ObjectInitializerAssignments = [..data.ObjectInitializerProperties.Select(Dependency)];
        
        _isContextPassing = containerCheckTypeProperties.IsContextPassingType(data.Implementation);
        return;

        Dependency Dependency(ISymbol symbol)
        {
            var type = symbol switch
            {
                IParameterSymbol parameter => parameter.Type,
                IPropertySymbol property => property.Type,
                _ => throw new InvalidOperationException($"Unexpected symbol type: {symbol.GetType()}")
            };
            return new Dependency(symbol.Name, typeEdgeFactory(this, typeNodeManager.GetOrAddNode(type)), symbol.Locations.FirstOrDefault() ?? Location.None, type);
        }
    }
    internal ConcreteImplementationNodeData Data { get; }
    internal ImmutableArray<Dependency> ConstructorParameters { get; }
    internal ImmutableArray<Dependency> ObjectInitializerAssignments { get; }
    internal bool ContextPurging { get; private set; } 
    internal bool OriginalContextNeeded => ContextPurging && ConstructorParameters.Concat(ObjectInitializerAssignments)
        .Any(d => d.ContextSwitchOutwardFacingTypeId is not null);
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteImplementationNode node && Data.Equals(node.Data);

    public IReadOnlyList<(TypeNode TypeNode, Location Location)> ConnectIfNotAlready(EdgeContext context)
    {
        var originalContext = context;
        if (!_isContextPassing 
            && context is { InitialInitialCaseChoice: InitialCaseChoiceContext.Single { OutwardFacingTypeId: > 0 or < 0, InitialCaseId: > 0 or < 0 } } 
                or { Key: KeyContext.Single })
        {
            ContextPurging = true;
            context = context with
            {
                InitialInitialCaseChoice = new InitialCaseChoiceContext.None(),
                Key = new KeyContext.None()
            };
        }
        var notYetConnectedTypeNodes = new List<(TypeNode TypeNode, Location Location)>();
        foreach (var dependency in ConstructorParameters.Concat(ObjectInitializerAssignments))
        {
            if (dependency.Edge.AddContext(context))
                notYetConnectedTypeNodes.Add((dependency.Edge.Target, dependency.Location));
            if (!_isContextPassing 
                && originalContext.InitialInitialCaseChoice is InitialCaseChoiceContext.Single { OutwardFacingTypeId: var initialCaseId }
                && _idRegister.GetOutwardFacingTypeId(_typeSymbolUtility.GetUnwrappedType(dependency.Type)) != initialCaseId)
            {
                dependency.ContextSwitchOutwardFacingTypeId = initialCaseId;
            }
        }
        return notYetConnectedTypeNodes;
    }
}