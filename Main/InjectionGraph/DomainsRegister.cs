using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph;

internal sealed class DomainsRegister : IContainerInstance
{
    private readonly Func<Domain> _domainFactory;
    private readonly Dictionary<string, Domain> _domains = [];

    internal DomainsRegister(Func<Domain>  domainFactory)
    {
        _domainFactory = domainFactory;
        ContainerDomain = domainFactory();
    }
    
    internal Domain ContainerDomain { get; }
    internal Domain this[string name] => _domains[name];

    internal void RegisterContainerInstance(TypeNode node) => 
        ContainerDomain.AddScopedInstance(node);

    internal void RegisterScopedInstance(string scopeName, TypeNode node)
    {
        if (!_domains.TryGetValue(scopeName, out var scopedInstances))
        {
            scopedInstances = _domainFactory();
            _domains[scopeName] = scopedInstances;
        }
        scopedInstances.AddScopedInstance(node);
    }
}