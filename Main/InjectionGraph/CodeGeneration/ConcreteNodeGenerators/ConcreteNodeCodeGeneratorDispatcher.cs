using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class ConcreteNodeCodeGeneratorDispatcher : IContainerInstance
{
    private const string NotAvailable = "null!"; // ToDo change value to "not_available" as soon as correct behavior is required

    private readonly ExceptionNodeCodeGenerator _exceptionNodeCodeGenerator;
    private readonly OverrideNodeCodeGenerator _overrideNodeCodeGenerator;
    private readonly ImplementationNodeCodeGenerator _implementationNodeCodeGenerator;
    private readonly FunctorNodeCodeGenerator _functorNodeCodeGenerator;
    private readonly InterfaceNodeCodeGenerator _interfaceNodeCodeGenerator;
    private readonly KeyValuePairNodeCodeGenerator _keyValuePairNodeCodeGenerator;
    private readonly EnumerableNodeCodeGenerator _enumerableNodeCodeGenerator;

    internal ConcreteNodeCodeGeneratorDispatcher(
        ExceptionNodeCodeGenerator exceptionNodeCodeGenerator,
        OverrideNodeCodeGenerator overrideNodeCodeGenerator,
        ImplementationNodeCodeGenerator implementationNodeCodeGenerator,
        FunctorNodeCodeGenerator functorNodeCodeGenerator,
        InterfaceNodeCodeGenerator interfaceNodeCodeGenerator,
        KeyValuePairNodeCodeGenerator keyValuePairNodeCodeGenerator,
        EnumerableNodeCodeGenerator enumerableNodeCodeGenerator)
    {
        _exceptionNodeCodeGenerator = exceptionNodeCodeGenerator;
        _overrideNodeCodeGenerator = overrideNodeCodeGenerator;
        _implementationNodeCodeGenerator = implementationNodeCodeGenerator;
        _functorNodeCodeGenerator = functorNodeCodeGenerator;
        _interfaceNodeCodeGenerator = interfaceNodeCodeGenerator;
        _keyValuePairNodeCodeGenerator = keyValuePairNodeCodeGenerator;
        _enumerableNodeCodeGenerator = enumerableNodeCodeGenerator;
    }

    internal string GenerateForInjectionNode(StringBuilder code, TypeNode node)
    {
        if (node.Outgoing.Count > 1)
            return NotAvailable; // ToDo this is wrong, adjust as soon a correct behavior is required

        var innerNode = node.Outgoing.Select(e => e.Target).FirstOrDefault();

        return innerNode switch
        {
            ConcreteExceptionNode exceptionNode => _exceptionNodeCodeGenerator.Generate(code, node, exceptionNode),
            ConcreteOverrideNode overrideNode => _overrideNodeCodeGenerator.Generate(code, node, overrideNode),
            ConcreteImplementationNode implementationNode => _implementationNodeCodeGenerator.Generate(code, node, implementationNode),
            ConcreteFunctorNode functorNode => _functorNodeCodeGenerator.Generate(code, node, functorNode),
            ConcreteInterfaceNode interfaceNode => _interfaceNodeCodeGenerator.Generate(code, node, interfaceNode),
            ConcreteKeyValuePairNode keyValuePairNode => _keyValuePairNodeCodeGenerator.Generate(code, node, keyValuePairNode),
            ConcreteEnumerableNode enumerableNode => _enumerableNodeCodeGenerator.Generate(code, node, enumerableNode),
            _ => NotAvailable
        };
    }
}
