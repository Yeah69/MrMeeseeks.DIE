using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeImplementationResolutionBuilder
{
    MultiSynchronicityFunctionCallResolution EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker);
}