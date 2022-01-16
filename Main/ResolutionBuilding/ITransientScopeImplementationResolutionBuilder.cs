namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeImplementationResolutionBuilder
{
    void EnqueueRangedInstanceResolution(RangedInstanceResolutionsQueueItem item);
}