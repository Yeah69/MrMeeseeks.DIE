using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.Nodes;

internal sealed record ConcreteInterfaceNodeData(INamedTypeSymbol Interface)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Interface, CustomSymbolEqualityComparer.IncludeNullability);
        return hash.ToHashCode();
    }

    public bool Equals(ConcreteInterfaceNodeData? other)
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

internal sealed class ConcreteInterfaceNodeManager(Func<ConcreteInterfaceNodeData, ConcreteInterfaceNode> factory)
    : ConcreteNodeManagerBase<ConcreteInterfaceNodeData, ConcreteInterfaceNode>(factory), IContainerInstance;

internal sealed class ConcreteInterfaceNode : IConcreteNode
{
    private readonly IdRegister _idRegister;
    private readonly TypeNodeManager _typeNodeManager;
    private readonly Func<IConcreteNode, TypeNode, TypeEdge> _typeEdgeFactory;
    private readonly Dictionary<int, InnerCaseIdResponse.Success> _currentChainCaseToNextChainCase = [];
    private readonly Dictionary<ScopeNodeContext, InnerCaseIdResponse.Success> _scopeNodeToNextCase = [];
    private readonly Dictionary<(ScopeNodeContext Node, ITypeSymbol KeyType, object KeyObject), InnerCaseIdResponse.Success> _keyToNextChainCase = [];
    private readonly Dictionary<int, TypeEdge> _typeCaseToEdge = [];

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
    internal IEnumerable<(int CurrentChainCase, int NextChainCase, int NextTypeCase)> NextChainCases => 
        _currentChainCaseToNextChainCase.Select(kvp => (kvp.Key, NextCaseId: kvp.Value.NextChainCase, NextTypeCase: kvp.Value.CurrentTypeCase))
            .OrderBy(x => x.Key);
    internal IEnumerable<(ScopeNodeContext Node, int NextChainCase, int NextTypeCase)> InitialChainCase =>
        _scopeNodeToNextCase.Select(kvp => (kvp.Key, NextCaseId: kvp.Value.NextChainCase, NextTypeCase: kvp.Value.CurrentTypeCase))
            .OrderBy(x => x.NextCaseId);
    internal IEnumerable<(ScopeNodeContext Node, ITypeSymbol KeyType, object KeyObject, int NextChainCase, int NextTypeCase)> KeyObjectToChainCase =>
        _keyToNextChainCase.Select(kvp => (kvp.Key.Node, kvp.Key.KeyType, kvp.Key.KeyObject, NextCaseId: kvp.Value.NextChainCase, NextTypeCase: kvp.Value.CurrentTypeCase));
    internal IEnumerable<(int TypeCase, TypeEdge Edge)> TypeCases =>
        _typeCaseToEdge.Select(kvp => (kvp.Key, kvp.Value))
            .OrderBy(x => x.Key);
    
    public override int GetHashCode() => 
        Data.GetHashCode();
    public override bool Equals(object? obj) => 
        obj is ConcreteInterfaceNode node && Data.Equals(node.Data);

    private abstract record InnerCaseIdResponse
    {
        internal sealed record Success(TypeEdge Edge, int NextChainCase, int CurrentTypeCase) : InnerCaseIdResponse;
        internal sealed record Error(string ErrorMessage) : InnerCaseIdResponse;
    }

    internal abstract record CaseIdResponse
    {
        internal sealed record Success(TypeNode TypeNode, Location Location, EdgeContext EdgeContext) :  CaseIdResponse;
        internal sealed record Error(string ErrorMessage) : CaseIdResponse;
        internal sealed record None3 : CaseIdResponse;
    }
    public CaseIdResponse ConnectIfNotAlready(EdgeContext context)
    {
        var innerCaseIdResponse = context switch
        {
            { ScopeNode: var node, Key: KeyContext.Single { Type: {} keyType, Value: { } keyValue } } =>
                GetKeyedDefault(node, keyType, keyValue),
            { CaseChoice: CaseChoiceContext.Single { OutwardFacingTypeId: var outwardFacingTypeId, CaseId: var caseId } }
                when outwardFacingTypeId == Number =>
                GetNextCase(caseId),
            { ScopeNode: var node } =>
                GetDefault(node)
        };
        if (context.Key is not KeyContext.None1)
            context = context with { Key = new KeyContext.None1() };
        switch (innerCaseIdResponse)
        {
            case InnerCaseIdResponse.Success { NextChainCase: var nextCaseId, Edge: var nextEdge}:
                context = context with { CaseChoice = nextCaseId is 0 
                    ? new CaseChoiceContext.None2() 
                    : new CaseChoiceContext.Single(Number, nextCaseId) };

                return nextEdge.AddContext(context) 
                    ? new CaseIdResponse.Success(nextEdge.Target, Location.None, context)
                    : new CaseIdResponse.None3();
            case InnerCaseIdResponse.Error { ErrorMessage: var errorMessage }:
                return new CaseIdResponse.Error(errorMessage);
            default:
                return new CaseIdResponse.Error("Unexpected case");
        }
        
        InnerCaseIdResponse GetKeyedDefault(ScopeNodeContext scopeNode, ITypeSymbol keyType, object keyValue)
        {
            if (_keyToNextChainCase.TryGetValue((scopeNode, keyType, keyValue), out var found))
                return found;
            
            var targetImplementationResult =
                // If there is a registered composite type for the current interface type, we use that as the implementation
                context.ScopeNode.CheckTypeProperties.ShouldBeComposite(Data.Interface) 
                && context.ScopeNode.CheckTypeProperties.GetCompositeFor(Data.Interface) is { } compositeType 
                    ? new ImplementationResult.Single(compositeType)
                    : context.ScopeNode.CheckTypeProperties.MapToSingleFittingImplementation(Data.Interface, injectionKey: new InjectionKey(keyType, keyValue));
        
            if (targetImplementationResult is not ImplementationResult.Single { Implementation: var targetImplementation })
            {
                var logMessage = targetImplementationResult switch
                {
                    ImplementationResult.None5 => $"Interface: No implementation registered for \"{Data.Interface.FullName()}\".",
                    ImplementationResult.Multiple { Implementations: var implementations} => $"Interface: Multiple implementations registered for \"{Data.Interface.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                    _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
                };
                return new InnerCaseIdResponse.Error(logMessage);
            }
            
            switch (_idRegister.GetInitialChainCase(scopeNode, Data.Interface, targetImplementation))
            {
                case IdRegister.ChainCaseIdResponse.Success { NextChainCase: var currentChainCase }:
                {
                    var nextChainCaseResult = _idRegister.GetNextChainCase(currentChainCase);
                    if (nextChainCaseResult is not IdRegister.ChainCaseIdResponse.Success { NextChainCase: var nextChainCase })
                        return new InnerCaseIdResponse.Error(((IdRegister.ChainCaseIdResponse.Error)nextChainCaseResult).ErrorMessage);
                    var response = HandleChainCase(currentChainCase, nextChainCase);
                    if (response is not InnerCaseIdResponse.Success success)
                        return response;
                    _keyToNextChainCase[(scopeNode, keyType, keyValue)] = success;
                    return success;
                }
                case IdRegister.ChainCaseIdResponse.Error { ErrorMessage: var message }:
                    return new InnerCaseIdResponse.Error(message);
                default:
                    return new InnerCaseIdResponse.Error("Unknown error");
            }
        }
        
        InnerCaseIdResponse GetNextCase(int chainCase)
        {
            if (_currentChainCaseToNextChainCase.TryGetValue(chainCase, out var found)) 
                return found;
            
            switch (_idRegister.GetNextChainCase(chainCase))
            {
                case IdRegister.ChainCaseIdResponse.Success { NextChainCase: var nextChainCase }:
                {
                    var response = HandleChainCase(chainCase, nextChainCase);
                    if (response is not InnerCaseIdResponse.Success success)
                        return response;
                    _currentChainCaseToNextChainCase[chainCase] = success;
                    return success;
                }
                case IdRegister.ChainCaseIdResponse.Error { ErrorMessage: var message }:
                    return new InnerCaseIdResponse.Error(message);
                default:
                    return new InnerCaseIdResponse.Error("Unknown error");
            }
        }
        
        InnerCaseIdResponse GetDefault(ScopeNodeContext scopeNode)
        {
            if (_scopeNodeToNextCase.TryGetValue(scopeNode, out var found))
                return found;
            
            var targetImplementationResult =
                // If there is a registered composite type for the current interface type, we use that as the implementation
                context.ScopeNode.CheckTypeProperties.ShouldBeComposite(Data.Interface) 
                && context.ScopeNode.CheckTypeProperties.GetCompositeFor(Data.Interface) is { } compositeType 
                    ? new ImplementationResult.Single(compositeType)
                    : context.ScopeNode.CheckTypeProperties.MapToSingleFittingImplementation(Data.Interface, injectionKey: null);
        
            if (targetImplementationResult is not ImplementationResult.Single { Implementation: var targetImplementation })
            {
                var logMessage = targetImplementationResult switch
                {
                    ImplementationResult.None5 => $"Interface: No implementation registered for \"{Data.Interface.FullName()}\".",
                    ImplementationResult.Multiple { Implementations: var implementations} => $"Interface: Multiple implementations registered for \"{Data.Interface.FullName()}\": {string.Join(", ", implementations.Select(i => i.FullName()))}.",
                    _ => throw new InvalidOperationException("Unexpected SingleImplementationResult")
                };
                return new InnerCaseIdResponse.Error(logMessage);
            }
            
            switch (_idRegister.GetInitialChainCase(scopeNode, Data.Interface, targetImplementation))
            {
                case IdRegister.ChainCaseIdResponse.Success { NextChainCase: var currentChainCase }:
                {
                    var nextChainCaseResult = _idRegister.GetNextChainCase(currentChainCase);
                    if (nextChainCaseResult is not IdRegister.ChainCaseIdResponse.Success { NextChainCase: var nextChainCase })
                        return new InnerCaseIdResponse.Error(((IdRegister.ChainCaseIdResponse.Error)nextChainCaseResult).ErrorMessage);
                    var response = HandleChainCase(currentChainCase, nextChainCase);
                    if (response is not InnerCaseIdResponse.Success success)
                        return response;
                    _scopeNodeToNextCase[scopeNode] = success;
                    return success;
                }
                case IdRegister.ChainCaseIdResponse.Error { ErrorMessage: var message }:
                    return new InnerCaseIdResponse.Error(message);
                default:
                    return new InnerCaseIdResponse.Error("Unknown error");
            }
        }

        InnerCaseIdResponse HandleChainCase(int currentChainCase, int nextChainCase)
        {
            int typeCase;
            switch (_idRegister.GetTypeCase(currentChainCase))
            {
                case IdRegister.ChainCaseIdResponse.Success { NextChainCase: var tc }:
                    typeCase = tc;
                    break;
                case IdRegister.ChainCaseIdResponse.Error error:
                    return new InnerCaseIdResponse.Error(error.ErrorMessage);
                default:
                    return new InnerCaseIdResponse.Error("Unknown error");
            }

            if (_typeCaseToEdge.TryGetValue(typeCase, out var foundEdge)) 
                return new InnerCaseIdResponse.Success(foundEdge, nextChainCase, typeCase);
                    
            var type = _idRegister.GetTypeOfTypeCase(typeCase);
            var typeEdge = _typeEdgeFactory(this, _typeNodeManager.GetOrAddNode(type));
            _typeCaseToEdge[typeCase] = typeEdge;

            return new InnerCaseIdResponse.Success(typeEdge, nextChainCase, typeCase);
        }
    }
}