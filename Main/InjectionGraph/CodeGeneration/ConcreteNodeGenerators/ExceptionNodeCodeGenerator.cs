using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class ExceptionNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteExceptionNode>, IContainerInstance
{
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly WellKnownTypes _wellKnownTypes;

    internal ExceptionNodeCodeGenerator(
        ReferenceGenerator referenceGenerator,
        WellKnownTypes wellKnownTypes)
    {
        _referenceGenerator = referenceGenerator;
        _wellKnownTypes = wellKnownTypes;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteExceptionNode concreteNode)
    {
        var reference = _referenceGenerator.Generate(typeNode.Type);
        code.AppendLine($"{typeNode.Type.FullName()} {reference} = default!;");
        code.AppendLine($"throw new {_wellKnownTypes.Exception.FullName()}(\"Failed to resolve type {typeNode.Type.FullName()} during code generation.\");");
        return reference;
    }
}
