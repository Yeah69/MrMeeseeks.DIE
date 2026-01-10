using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class KeyValuePairNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteKeyValuePairNode>, IContainerInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly ContextGenerator _contextGenerator;

    internal KeyValuePairNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ReferenceGenerator referenceGenerator,
        ContextGenerator contextGenerator)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _referenceGenerator = referenceGenerator;
        _contextGenerator = contextGenerator;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteKeyValuePairNode concreteNode)
    {
        var reference = _referenceGenerator.Generate(concreteNode.Data.KeyValuePairType);
        var keyReference = $"({concreteNode.KeyType.FullName()}) {_contextGenerator.ParameterName}.{_contextGenerator.KeyPropertyName}!";
        var valueReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, concreteNode.ValueEdge, concreteNode.ValueEdge.Target);
        code.AppendLine($"{concreteNode.Data.KeyValuePairType.FullName()} {reference} = new {concreteNode.Data.KeyValuePairType.FullName()}({keyReference}, {valueReference});");
        return reference;
    }
}
