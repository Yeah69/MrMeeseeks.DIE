using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ScopeNodeConfigContext : ITransientScopeRoot
{
    internal required ICheckTypeProperties CheckTypeProperties { get; init; }
    internal required UserDefinedElements UserDefinedElements { get; init; }
    
}