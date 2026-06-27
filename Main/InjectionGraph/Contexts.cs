using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class ScopeNodeConfigContext : IScopeRoot
{
    internal required ICheckTypeProperties CheckTypeProperties { get; init; }
    internal required UserDefinedElements UserDefinedElements { get; init; }
    
}