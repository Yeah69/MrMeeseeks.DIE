using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class OverrideNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteOverrideNode>, IScopeInstance
{
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly ContextGenerator _contextGenerator;
    private readonly SharedNameRegistry _sharedNameRegistry;

    internal OverrideNodeCodeGenerator(
        ReferenceGenerator referenceGenerator,
        ContextGenerator contextGenerator,
        SharedNameRegistry sharedNameRegistry)
    {
        _referenceGenerator = referenceGenerator;
        _contextGenerator = contextGenerator;
        _sharedNameRegistry = sharedNameRegistry;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteOverrideNode concreteNode, string? reference = null)
    {
        var actualReference = reference ?? _referenceGenerator.Generate(concreteNode.Data.Type);
        code.AppendLine($"{(reference is null ? $"{concreteNode.Data.Type.FullName()} " : "")}{actualReference} = (({_sharedNameRegistry.IOverrideInterfaceName}<{concreteNode.Data.Type.FullName()}>) {_contextGenerator.ParameterName}.{_contextGenerator.OverridesPropertyName}).Value();");
        return actualReference;
    }
}
