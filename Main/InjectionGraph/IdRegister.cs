using System.Threading;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class IdRegister : IContainerInstance
{
    private int _outwardFacingTypeIdCounter;
    private readonly Dictionary<ITypeSymbol, int> _outwardFacingTypeIdMap = new(CustomSymbolEqualityComparer.Default);
    private int _initialCaseIdCounter;
    private readonly Dictionary<(DomainContext Domain, ITypeSymbol Implementation), int> _implementationCaseStartIdMap = 
            new(new ValueTupleEqualityComparer<DomainContext, ITypeSymbol>(EqualityComparer<DomainContext>.Default, CustomSymbolEqualityComparer.Default));
    private readonly Dictionary<(DomainContext Domain, int CaseId), ITypeSymbol> _implementationCaseIdMap = [];

    internal int GetOutwardFacingTypeId(ITypeSymbol outwardFacingType) =>
        _outwardFacingTypeIdMap.TryGetValue(outwardFacingType, out var id)
            ? id
            : _outwardFacingTypeIdMap[outwardFacingType] = Interlocked.Increment(ref _outwardFacingTypeIdCounter);

    internal int GetInitialCaseId(DomainContext domain, ITypeSymbol type)
    {
        if (_implementationCaseStartIdMap.TryGetValue((domain, type), out var initialCaseId))
            return initialCaseId;

        initialCaseId = Interlocked.Increment(ref _initialCaseIdCounter);
        _implementationCaseStartIdMap[(domain, type)] = initialCaseId;
        _implementationCaseIdMap[(domain, initialCaseId)] = type;
        return initialCaseId;
    }

    internal ITypeSymbol GetTypeByInitialCaseId(DomainContext domain, int caseId) =>
        _implementationCaseIdMap.TryGetValue((domain, caseId), out var implementation)
            ? implementation
            : throw new InvalidOperationException($"No implementation found for case ID {caseId} in domain {domain}.");
    
    internal int GetUnusedCaseId() => Interlocked.Increment(ref _initialCaseIdCounter);
}