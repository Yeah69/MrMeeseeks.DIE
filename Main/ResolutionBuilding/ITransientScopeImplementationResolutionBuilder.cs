using MrMeeseeks.DIE.ResolutionBuilding.Function;

namespace MrMeeseeks.DIE.ResolutionBuilding;

internal interface ITransientScopeImplementationResolutionBuilder
{
    void EnqueueRangedInstanceResolution(
        ForConstructorParameter parameter,
        string label,
        string reference,
        Lazy<IFunctionResolutionSynchronicityDecisionMaker> synchronicityDecisionMaker);
}