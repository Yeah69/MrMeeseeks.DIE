using System.Collections.Concurrent;
using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class IdRegister(
    IContainerCheckTypeProperties containerCheckTypeProperties) 
    : IContainerInstance
{
    private int _outwardFacingTypeIdCounter;
    private readonly Dictionary<ITypeSymbol, int> _outwardFacingTypeIdMap = new(CustomSymbolEqualityComparer.Default);

    internal int GetOutwardFacingTypeId(ITypeSymbol outwardFacingType) =>
        _outwardFacingTypeIdMap.TryGetValue(outwardFacingType, out var id)
            ? id
            : _outwardFacingTypeIdMap[outwardFacingType] = Interlocked.Increment(ref _outwardFacingTypeIdCounter);

    internal abstract record CaseIdResponse
    {
        internal record Success(int NextCaseId, INamedTypeSymbol Type) : CaseIdResponse;
        internal record NoNextCaseId :  CaseIdResponse;
        internal record Error(string ErrorMessage) : CaseIdResponse;
    }

    private record DecorationChainNode(INamedTypeSymbol Type, int CaseId, DecorationChainNode? Next)
    {
        internal ConcurrentDictionary<INamedTypeSymbol, DecorationChainNode> Previous { get; } = new (CustomSymbolEqualityComparer.Default);
    }

    private readonly 
        ConcurrentDictionary<DomainContext, 
            ConcurrentDictionary<INamedTypeSymbol, 
                ConcurrentDictionary<INamedTypeSymbol, DecorationChainNode>>> _initialDecorationChainNode = [];
    private readonly ConcurrentDictionary<int, DecorationChainNode> _caseIdToDecorationChainNode = [];
    private int _caseCounter;
    
    internal CaseIdResponse GetInitialCaseId(DomainContext domain, INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType)
    {
        var initialNode = _initialDecorationChainNode.GetOrAdd(domain, _ => new ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<INamedTypeSymbol, DecorationChainNode>>())
            .GetOrAdd(interfaceType, _ => new ConcurrentDictionary<INamedTypeSymbol, DecorationChainNode>(CustomSymbolEqualityComparer.Default))
            .GetOrAdd(implementationType, AddDecorationChainNode);
        
        return new CaseIdResponse.Success(initialNode.CaseId, implementationType);

        DecorationChainNode AddDecorationChainNode(INamedTypeSymbol implementation)
        {
            var currentDecorationChainNode = new DecorationChainNode(implementation, Interlocked.Increment(ref _caseCounter), null);
            _caseIdToDecorationChainNode[currentDecorationChainNode.CaseId] = currentDecorationChainNode;
        
            var decorationSequence = containerCheckTypeProperties.GetDecorationSequenceFor(interfaceType, implementation);

            for (int i = decorationSequence.Count - 1; i >= 0; i--)
            {
                var current = decorationSequence[i];
                var decorationType = current switch
                {
                    Decoration.Decorator { Type: var type } => type,
                    Decoration.Interceptor { Type: var type } => type,
                    _ => throw new InvalidOperationException("Unexpected Decoration")
                };
                var nextDecorationChainNode = currentDecorationChainNode.Previous.GetOrAdd(decorationType, t =>
                    new DecorationChainNode(decorationType, Interlocked.Increment(ref _caseCounter), currentDecorationChainNode));
                _caseIdToDecorationChainNode[nextDecorationChainNode.CaseId] = nextDecorationChainNode;
                currentDecorationChainNode = nextDecorationChainNode;
            }
        
            return currentDecorationChainNode;
        }
    }

    internal CaseIdResponse GetNextCaseId(int currentCaseId) =>
        _caseIdToDecorationChainNode.TryGetValue(currentCaseId, out var decorationChainNode)
            ? decorationChainNode.Next is { CaseId: var nextCaseId, Type: var type}
                ? new CaseIdResponse.Success(nextCaseId, type)
                : new CaseIdResponse.NoNextCaseId()
            : new CaseIdResponse.Error("Next decoration chain not found.");
    
    internal INamedTypeSymbol GetTypeOfCaseId(int currentCaseId) =>
        _caseIdToDecorationChainNode[currentCaseId].Type;
}