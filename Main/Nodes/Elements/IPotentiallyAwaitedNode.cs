using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IPotentiallyAwaitedNode
{
    bool Awaited { get; set; }
    string? AsyncReference { get; }
    string? AsyncTypeFullName { get; }
    SynchronicityDecision SynchronicityDecision { get; }
}