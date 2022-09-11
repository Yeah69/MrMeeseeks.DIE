using MrMeeseeks.DIE.Configuration.Attributes;

namespace GettingStarted;

[ImplementationAggregation(
    typeof(Logger), 
    typeof(MrMeeseeks))]

[CreateFunction(typeof(MrMeeseeks), "Create")]
internal sealed partial class Container
{
    
}