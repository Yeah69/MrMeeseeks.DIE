using System.Collections.Concurrent;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ConnectedResolutionIdsRegistry : IContainerInstance
{
    private sealed class Grouping
    {
        private readonly List<int> _groupedResolutionIds = [];

        internal Grouping(int initial) => 
            _groupedResolutionIds.Add(initial);

        internal IReadOnlyList<int> GroupedResolutionIds => _groupedResolutionIds;
        
        internal void Add(int resolutionId) => 
            _groupedResolutionIds.Add(resolutionId);
    }
    
    private readonly ConcurrentDictionary<int, Grouping> groups = [];

    internal void RegisterConnection(int resolutionId0, int resolutionId1)
    {
        var group0 = groups.TryGetValue(resolutionId0, out var g0)
            ? g0
            : NewGrouping0();

        var maybeGroup1 = groups.TryGetValue(resolutionId1, out var g1)
            ? g1
            : null;

        if (maybeGroup1 is { } group1)
        {
            foreach (var id in group1.GroupedResolutionIds)
            {
                group0.Add(id);
                groups[id] = group0;
            }
        }
        else
        {
            group0.Add(resolutionId1);
            groups[resolutionId0] = group0;
        }

        return;

        Grouping NewGrouping0()
        {
            var g = new Grouping(resolutionId0);
            groups[resolutionId0] = g;
            return g;
        }
    }

    internal IReadOnlyList<int> GetConnectedResolutionIdsAndSelf(int resolutionId) =>
        groups.TryGetValue(resolutionId, out var group0)
            ? group0.GroupedResolutionIds
            : [resolutionId];
}