using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class ConcreteNodeCodeGeneratorDispatcher : IContainerInstance
{
    private readonly ContextGenerator _contextGenerator;
    private readonly SharedNameRegistry _sharedNameRegistry;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly KeyUtility _keyUtility;
    private readonly ContainerInfo _containerInfo;
    private readonly ExceptionNodeCodeGenerator _exceptionNodeCodeGenerator;
    private readonly OverrideNodeCodeGenerator _overrideNodeCodeGenerator;
    private readonly ImplementationNodeCodeGenerator _implementationNodeCodeGenerator;
    private readonly FunctorNodeCodeGenerator _functorNodeCodeGenerator;
    private readonly InterfaceNodeCodeGenerator _interfaceNodeCodeGenerator;
    private readonly KeyValuePairNodeCodeGenerator _keyValuePairNodeCodeGenerator;
    private readonly EnumerableNodeCodeGenerator _enumerableNodeCodeGenerator;

    internal ConcreteNodeCodeGeneratorDispatcher(
        ContextGenerator contextGenerator,
        SharedNameRegistry sharedNameRegistry,
        ReferenceGenerator referenceGenerator,
        KeyUtility keyUtility,
        ContainerInfo containerInfo,
        ExceptionNodeCodeGenerator exceptionNodeCodeGenerator,
        OverrideNodeCodeGenerator overrideNodeCodeGenerator,
        ImplementationNodeCodeGenerator implementationNodeCodeGenerator,
        FunctorNodeCodeGenerator functorNodeCodeGenerator,
        InterfaceNodeCodeGenerator interfaceNodeCodeGenerator,
        KeyValuePairNodeCodeGenerator keyValuePairNodeCodeGenerator,
        EnumerableNodeCodeGenerator enumerableNodeCodeGenerator)
    {
        _contextGenerator = contextGenerator;
        _sharedNameRegistry = sharedNameRegistry;
        _referenceGenerator = referenceGenerator;
        _keyUtility = keyUtility;
        _containerInfo = containerInfo;
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
        var edgeAndTargets = node.Outgoing.Select(e => (Edges: e, ConcreteNode: e.Target)).ToList();
        var maybeOverride = edgeAndTargets.Select(t => t.ConcreteNode).OfType<ConcreteOverrideNode>().SingleOrDefault();
        var nonOverrides = edgeAndTargets.Where(t => t.ConcreteNode is not ConcreteOverrideNode).ToList();
        var reference = _referenceGenerator.Generate("ref");
        
        if (maybeOverride is {} concreteOverrideNode)
        {
            if (nonOverrides.Count == 0)
                return _overrideNodeCodeGenerator.Generate(code, node, concreteOverrideNode);

            code.AppendLine($"{node.Type.FullName()} {reference};");
            code.AppendLine($"if ({_contextGenerator.ParameterName}.{_contextGenerator.OverridesPropertyName} is {_sharedNameRegistry.IOverrideInterfaceName}<{concreteOverrideNode.Data.Type.FullName()}>)");
            code.AppendLine("{");
            _overrideNodeCodeGenerator.Generate(code, node, concreteOverrideNode, reference: reference);
            code.AppendLine("}");
            code.AppendLine("else");
            code.AppendLine("{");
            GenerateForNonOverrides();
            code.AppendLine("}");
            return reference;
        }
        
        GenerateForNonOverrides();
        
        return reference;

        void GenerateForNonOverrides()
        {
            if (nonOverrides is [var single])
                GenerateSwitchBody(node, single.ConcreteNode);
            else
            {
                for (var i = 0; i < nonOverrides.Count; i++)
                {
                    var (edge, concreteNode) = nonOverrides[i];
                    var condition = string.Join(" || ", edge.Contexts.Select(GenerateContextMatchCondition));

                    if (i == 0)
                        code.AppendLine($"if ({condition})");
                    else if (i < nonOverrides.Count - 1)
                        code.AppendLine($"else if ({condition})");
                    else
                        code.AppendLine("else");

                    code.AppendLine("{");
                    GenerateSwitchBody(node, concreteNode);
                    code.AppendLine("}");
                }
            }
        }
        
        void GenerateSwitchBody(TypeNode typeNode, IConcreteNode concreteNode) =>
            _ = concreteNode switch
            {
                ConcreteExceptionNode exceptionNode => _exceptionNodeCodeGenerator.Generate(code, typeNode, exceptionNode, reference),
                ConcreteImplementationNode implementationNode => _implementationNodeCodeGenerator.Generate(code, typeNode, implementationNode, reference),
                ConcreteFunctorNode functorNode => _functorNodeCodeGenerator.Generate(code, typeNode, functorNode, reference),
                ConcreteInterfaceNode interfaceNode => _interfaceNodeCodeGenerator.Generate(code, typeNode, interfaceNode, reference),
                ConcreteKeyValuePairNode keyValuePairNode => _keyValuePairNodeCodeGenerator.Generate(code, typeNode, keyValuePairNode, reference),
                ConcreteEnumerableNode enumerableNode => _enumerableNodeCodeGenerator.Generate(code, typeNode, enumerableNode, reference),
                ConcreteOverrideNode overrideNode => _overrideNodeCodeGenerator.Generate(code, typeNode, overrideNode, reference),
                _ => ""
            };
        
        string GenerateContextMatchCondition(EdgeContext context)
        {
            var scopeNodeName = context.ScopeNode switch
            {
                ScopeNodeContext.Container => _containerInfo.Name,
                ScopeNodeContext.Scope scope => scope.ScopeName,
                ScopeNodeContext.TransientScope transientScope => transientScope.TransientScopeName,
                _ => throw new ArgumentOutOfRangeException(nameof(context.ScopeNode))
            };

            var conditions = new List<string> { $"{_contextGenerator.ParameterName}.{_contextGenerator.ScopeNodeNamePropertyName} == \"{scopeNodeName}\"" };
            
            if (context.CaseChoice is CaseChoiceContext.Single(var outwardFacingTypeId, var caseId))
            {
                conditions.Add($"{_contextGenerator.ParameterName}.{_contextGenerator.OutwardFacingTypeNumberPropertyName} == {outwardFacingTypeId} && {_contextGenerator.ParameterName}.{_contextGenerator.CaseNumberPropertyName} == {caseId}");
            }

            if (context.Key is KeyContext.Single(var type, Value: var value))
            {
                var keyLiteral = _keyUtility.GenerateKeyLiteral(type, value);
                conditions.Add($"{_contextGenerator.ParameterName}.{_contextGenerator.KeyPropertyName}?.Equals({keyLiteral}) == true");
            }

            return string.Join(" && ", conditions);
        }
    }
}
