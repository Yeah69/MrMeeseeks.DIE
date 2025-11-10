using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

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

internal class ConcreteInterfaceNodeManager(Func<ConcreteInterfaceNodeData, ConcreteInterfaceNode> factory)
    : ConcreteNodeManagerBase<ConcreteInterfaceNodeData, ConcreteInterfaceNode>(factory), IContainerInstance;

internal class ConcreteInterfaceNode : IConcreteNode
{
    private readonly IContainerCheckTypeProperties _containerCheckTypeProperties;
    private readonly IdRegister _idRegister;
    private readonly TypeNodeManager _typeNodeManager;
    private readonly Func<IConcreteNode, TypeNode, TypeEdge> _typeEdgeFactory;
    private readonly Dictionary<int, InnerCaseIdResponse.Success> _caseToNextCase = [];
    private readonly Dictionary<DomainContext, InnerCaseIdResponse.Success> _domainToNextCase = [];
    private readonly Dictionary<(DomainContext Domain, ITypeSymbol KeyType, object KeyObject), InnerCaseIdResponse.Success> _keyToNextCase = [];

    internal ConcreteInterfaceNode(
        // parameters
        ConcreteInterfaceNodeData data,

        // dependencies
        IContainerCheckTypeProperties containerCheckTypeProperties,
        IdRegister idRegister,
        TypeNodeManager typeNodeManager,
        Func<IConcreteNode, TypeNode, TypeEdge> typeEdgeFactory)
    {
        _containerCheckTypeProperties = containerCheckTypeProperties;
        _idRegister = idRegister;
        _typeNodeManager = typeNodeManager;
        _typeEdgeFactory = typeEdgeFactory;
        Data = data;
        Number = idRegister.GetOutwardFacingTypeId(data.Interface);
    }
    internal int Number { get; }
    internal ConcreteInterfaceNodeData Data { get; }
    internal IEnumerable<(TypeEdge Edge, int Id, int NextId)> Cases => 
        _caseToNextCase.Select(kvp => (kvp.Value.Edge, kvp.Key, kvp.Value.NextCaseId));
    internal IEnumerable<(DomainContext Domain, int NextId)> DefaultImplementationsCaseNumbers => 
        _domainToNextCase.Select(kvp => (kvp.Key, kvp.Value.NextCaseId));
    internal IEnumerable<(DomainContext Domain, ITypeSymbol KeyType, object KeyObject, int NextId)> KeyObjectToCaseNumbers => 
        _keyToNextCase.Select(kvp => (kvp.Key.Domain, kvp.Key.KeyType, kvp.Key.KeyObject, kvp.Value.NextCaseId));
    
    public override int GetHashCode() => Data.GetHashCode();
    public override bool Equals(object? obj) => obj is ConcreteInterfaceNode node && Data.Equals(node.Data);

    private abstract record InnerCaseIdResponse
    {
        internal sealed record Success(TypeEdge Edge, int NextCaseId) : InnerCaseIdResponse;
        internal sealed record Error(string ErrorMessage) : InnerCaseIdResponse;
    }

    internal abstract record CaseIdResponse
    {
        internal sealed record Success(TypeNode TypeNode, Location Location, EdgeContext EdgeContext) :  CaseIdResponse;
        internal sealed record Error(string ErrorMessage) : CaseIdResponse;
        internal sealed record None : CaseIdResponse;
    }
    public CaseIdResponse ConnectIfNotAlready(EdgeContext context)
    {
        var innerCaseIdResponse = context switch
        {
            { Domain: var domain, Key: KeyContext.Single { Type: {} keyType, Value: { } keyValue } } => 
                GetKeyedDefault(domain, keyType, keyValue),
            { CaseChoice: CaseChoiceContext.Single { OutwardFacingTypeId: var outwardFacingTypeId, CaseId: var caseId } } 
                when outwardFacingTypeId == Number =>
                GetNextCase(caseId),
            { Domain: var domain } =>
                GetDefault(domain)
        };
        if (context.Key != new KeyContext.None())
            context = context with { Key = new KeyContext.None() };
        switch (innerCaseIdResponse)
        {
            case InnerCaseIdResponse.Success { NextCaseId: var nextCaseId, Edge: var nextEdge}:
                context = nextCaseId is 0 
                    ? context with { CaseChoice = new CaseChoiceContext.None() }
                    : context with { CaseChoice = new CaseChoiceContext.Single(Number, nextCaseId) };

                return nextEdge.AddContext(context) 
                    ? new CaseIdResponse.Success(nextEdge.Target, Location.None, context)
                    : new CaseIdResponse.None();
            case InnerCaseIdResponse.Error { ErrorMessage: var errorMessage }:
                return new CaseIdResponse.Error(errorMessage);
            default:
                return new CaseIdResponse.Error("Unexpected case");
        }
        
        InnerCaseIdResponse GetKeyedDefault(DomainContext domain, ITypeSymbol keyType, object keyValue)
        {
            if (_keyToNextCase.TryGetValue((domain, keyType, keyValue), out var success)) 
                return success;
            
            var targetImplementationResult =
                // If there is a registered composite type for the current interface type, we use that as the implementation
                _containerCheckTypeProperties.ShouldBeComposite(Data.Interface) && _containerCheckTypeProperties.GetCompositeFor(Data.Interface) is { } compositeType 
                    ? new ImplementationResult.Single(compositeType)
                    : _containerCheckTypeProperties.MapToSingleFittingImplementation(Data.Interface, injectionKey: new InjectionKey(keyType, keyValue));
        
            if (targetImplementationResult is not ImplementationResult.Single { Implementation: var targetImplementation })
            {
                var logMessage = targetImplementationResult switch
                {
                    ImplementationResult.None => $"Interface: No implementation registered for \"{Data.Interface.FullName()}\".",
                    ImplementationResult.Multiple { Implementations: var implementations} => $"Interface: Multiple implementations registered for \"{Data.Interface.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                    _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
                };
                return new InnerCaseIdResponse.Error(logMessage);
            }
            
            switch (_idRegister.GetInitialCaseId(domain, Data.Interface, targetImplementation))
            {
                case IdRegister.CaseIdResponse.Success { NextCaseId: var keyedCaseId }:
                {
                    var typeEdge = _typeEdgeFactory(this, _typeNodeManager.GetOrAddNode(Data.Interface));
                    success = new InnerCaseIdResponse.Success(typeEdge, keyedCaseId);
                    _keyToNextCase[(domain, keyType, keyValue)] = success;

                    return success;
                }
                case IdRegister.CaseIdResponse.Error { ErrorMessage: var message }:
                    return new InnerCaseIdResponse.Error(message);
                default:
                    return new InnerCaseIdResponse.Error("Unknown error");
            }
        }
        
        InnerCaseIdResponse GetNextCase(int caseId)
        {
            if (_caseToNextCase.TryGetValue(caseId, out var success)) 
                return success;

            var type = _idRegister.GetTypeOfCaseId(caseId);
            var typeEdge = _typeEdgeFactory(this, _typeNodeManager.GetOrAddNode(type));
            var nextCaseId = 0;
            switch (_idRegister.GetNextCaseId(caseId))
            {
                case IdRegister.CaseIdResponse.Error error:
                    return new InnerCaseIdResponse.Error(error.ErrorMessage);
                case IdRegister.CaseIdResponse.NoNextCaseId:
                    // Keep nextCaseId at 0
                    break;
                case IdRegister.CaseIdResponse.Success { NextCaseId: var nci }:
                    nextCaseId = nci;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            success = new InnerCaseIdResponse.Success(typeEdge, nextCaseId);
            _caseToNextCase[caseId] = success;

            return success;
        }
        
        InnerCaseIdResponse GetDefault(DomainContext domain)
        {
            if (_domainToNextCase.TryGetValue(domain, out var success)) 
                return success;
            
            var targetImplementationResult =
                // If there is a registered composite type for the current interface type, we use that as the implementation
                _containerCheckTypeProperties.ShouldBeComposite(Data.Interface) && _containerCheckTypeProperties.GetCompositeFor(Data.Interface) is { } compositeType 
                    ? new ImplementationResult.Single(compositeType)
                    : _containerCheckTypeProperties.MapToSingleFittingImplementation(Data.Interface, injectionKey: null);
        
            if (targetImplementationResult is not ImplementationResult.Single { Implementation: var targetImplementation })
            {
                var logMessage = targetImplementationResult switch
                {
                    ImplementationResult.None => $"Interface: No implementation registered for \"{Data.Interface.FullName()}\".",
                    ImplementationResult.Multiple { Implementations: var implementations} => $"Interface: Multiple implementations registered for \"{Data.Interface.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                    _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
                };
                return new InnerCaseIdResponse.Error(logMessage);
            }
            
            switch (_idRegister.GetInitialCaseId(domain, Data.Interface, targetImplementation))
            {
                case IdRegister.CaseIdResponse.Success { NextCaseId: var domainCaseId }:
                {
                    var typeEdge = _typeEdgeFactory(this, _typeNodeManager.GetOrAddNode(Data.Interface));
                    success = new InnerCaseIdResponse.Success(typeEdge, domainCaseId);
                    _domainToNextCase[domain] = success;

                    return success;
                }
                case IdRegister.CaseIdResponse.Error { ErrorMessage: var message }:
                    return new InnerCaseIdResponse.Error(message);
                default:
                    return new InnerCaseIdResponse.Error("Unknown error");
            }
        }
    }
}