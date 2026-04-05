using System.Collections.Concurrent;
using System.Threading;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class IdRegister(LocalDiagLogger logger)
    : IContainerInstance
{
    private int _outwardFacingTypeIdCounter;
    private readonly Dictionary<ITypeSymbol, int> _outwardFacingTypeIdMap = new(CustomSymbolEqualityComparer.Default);

    internal int GetOutwardFacingTypeId(ITypeSymbol outwardFacingType) =>
        _outwardFacingTypeIdMap.TryGetValue(outwardFacingType, out var id)
            ? id
            : _outwardFacingTypeIdMap[outwardFacingType] = Interlocked.Increment(ref _outwardFacingTypeIdCounter);

    internal abstract record ChainCaseIdResponse
    {
        internal sealed record Success(int NextChainCase) : ChainCaseIdResponse;
        internal sealed record Error(string ErrorMessage) : ChainCaseIdResponse;
    }

    private sealed record ChainKey(ImmutableArray<INamedTypeSymbol> Chain)
    {
        public override int GetHashCode() =>
            Chain.Aggregate(new HashCode(), (hc, nts) =>
            {
                hc.Add(nts, CustomSymbolEqualityComparer.IncludeNullability);
                return hc;
            }).ToHashCode();

        public bool Equals(ChainKey? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Chain.Length == other.Chain.Length && Chain.SequenceEqual(other.Chain);
        }
    }
    
    private readonly ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<ChainKey, ImmutableArray<int>>> _chainToChainCases = new(CustomSymbolEqualityComparer.Default);
    private readonly ConcurrentDictionary<int, int> _chainCaseToNextChainCase = [];
    private readonly ConcurrentDictionary<INamedTypeSymbol, ConcurrentDictionary<INamedTypeSymbol, int>> _typeToTypeCase = new(CustomSymbolEqualityComparer.Default);
    private readonly ConcurrentDictionary<int, int> _chainCaseToTypeCase = [];
    private readonly ConcurrentDictionary<int, INamedTypeSymbol> _typeCaseToType = [];
    private int _chainCaseCounter;
    private int _typeCaseCounter;
    
    internal ChainCaseIdResponse GetInitialChainCase(ScopeNodeContext scopeNode, INamedTypeSymbol interfaceType, INamedTypeSymbol implementationType)
    {
        var chain = scopeNode.CheckTypeProperties.GetDecorationSequenceFor(interfaceType, implementationType)
            .Select(d => d switch
            {
                Decoration.Decorator decorator => decorator.Type,
                Decoration.Interceptor interceptor => interceptor.Type,
                _ => throw new ArgumentOutOfRangeException(nameof(d))
            })
            .Prepend(implementationType)
            .ToImmutableArray();
        var scopeNodeName = scopeNode switch
        {
            ScopeNodeContext.Container => "Container",
            ScopeNodeContext.Scope scope => scope.ScopeName,
            ScopeNodeContext.TransientScope transientScope => transientScope.TransientScopeName,
            _ => throw new ArgumentOutOfRangeException(nameof(scopeNode))
        };
        logger.Warning(WarningLogData.Logging($"Chain({scopeNodeName},{interfaceType.Name},{implementationType.Name}): {string.Join(",", chain.Select(x => x.Name))}"), Location.None);
        var chainKey = new ChainKey(chain);

        var fistChainCase = _chainToChainCases
            .GetOrAdd(interfaceType, [])
            .GetOrAdd(chainKey, AddChainCases)
            .First();
        
        return new ChainCaseIdResponse.Success(fistChainCase);

        ImmutableArray<int> AddChainCases(ChainKey key)
        {
            var reversed = key.Chain.Reverse().ToImmutableArray();
            logger.Warning(WarningLogData.Logging($"Reversed({scopeNodeName},{interfaceType.Name},{implementationType.Name}): {string.Join(",", reversed.Select(x => x.Name))}"), Location.None);
            var ret = reversed.Select(_ => Interlocked.Increment(ref _chainCaseCounter)).Append(0).ToImmutableArray();
            logger.Warning(WarningLogData.Logging($"Ret({scopeNodeName},{interfaceType.Name},{implementationType.Name}): {string.Join(",", ret)}"), Location.None);

            var zip = ret.Zip(ret.Skip(1), (current, next) => (current, next)).ToImmutableArray();
            logger.Warning(WarningLogData.Logging($"Zip({scopeNodeName},{interfaceType.Name},{implementationType.Name}): {string.Join(",", zip)}"), Location.None);
            
            foreach (var t in zip)
               _chainCaseToNextChainCase.AddOrUpdate(t.current, t.next, (_, next) => next);
            
            foreach (var type in reversed)
            {
                _typeToTypeCase.GetOrAdd(interfaceType, _ => new ConcurrentDictionary<INamedTypeSymbol, int>(CustomSymbolEqualityComparer.IncludeNullability))
                    .GetOrAdd(type, t =>
                    {
                        var typeCase = Interlocked.Increment(ref _typeCaseCounter);
                        logger.Warning(WarningLogData.Logging($"typeToTypeCase({scopeNodeName},{interfaceType.Name},{implementationType.Name}): {t.Name},{typeCase}"), Location.None);
                        _typeCaseToType.AddOrUpdate(typeCase, t, (_, tt) => tt);
                        return typeCase;
                    });
            }

            for (var i = 0; i < reversed.Length; i++)
            {
                var type = reversed[i];
                var chainCase = ret[i];
                var typeCase = _typeToTypeCase[interfaceType][type];
                logger.Warning(WarningLogData.Logging($"chainCaseToTypeCase({scopeNodeName},{interfaceType.Name},{implementationType.Name}): {type.Name},{chainCase},{typeCase}"), Location.None);
                _chainCaseToTypeCase.AddOrUpdate(chainCase, typeCase, (_, tc) => tc);
            }
            
            return ret;
        }
    }

    internal ChainCaseIdResponse GetNextChainCase(int currentChainCase) =>
        _chainCaseToNextChainCase.TryGetValue(currentChainCase, out var nextChainCase)
            ? new ChainCaseIdResponse.Success(nextChainCase)
            : new ChainCaseIdResponse.Error("Next decoration chain not found.");
    
    internal INamedTypeSymbol GetTypeOfTypeCase(int typeCase) =>
        _typeCaseToType[typeCase];

    internal ChainCaseIdResponse GetTypeCase(int currentChainCase) =>
        _chainCaseToTypeCase.TryGetValue(currentChainCase, out var typeCase)
            ? new ChainCaseIdResponse.Success(typeCase)
            : new ChainCaseIdResponse.Error("Next decoration chain not found.");
}