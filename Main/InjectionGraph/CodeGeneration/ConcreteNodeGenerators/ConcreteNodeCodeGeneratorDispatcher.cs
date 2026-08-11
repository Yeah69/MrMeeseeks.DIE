using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class ConcreteNodeCodeGeneratorDispatcher(
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
    EnumerableNodeCodeGenerator enumerableNodeCodeGenerator,
    AsyncEnumerableNodeCodeGenerator asyncEnumerableNodeCodeGenerator,
    TaskNodeCodeGenerator taskNodeCodeGenerator)
    : IScopeInstance
{
    internal string GenerateForInjectionNode(StringBuilder code, TypeNode node, bool sync)
    {
        var edgeAndTargets = node.OutgoingConcreteEdges
            .Where(e => sync && e is ConcreteSyncEdge || !sync && e is ConcreteAsyncEdge)
            .Select(e => (Edges: e, ConcreteNode: e.Target))
            .ToList();
        var maybeOverride = edgeAndTargets.Select(t => t.ConcreteNode).OfType<ConcreteOverrideNode>().SingleOrDefault();
        var nonOverrides = edgeAndTargets.Where(t => t.ConcreteNode is not ConcreteOverrideNode).ToList();
        
        if (maybeOverride is null && nonOverrides.Count == 1)
            return GenerateSwitchBody(node, nonOverrides.Single().ConcreteNode, null);
        if (maybeOverride is {} onlyOverride && nonOverrides.Count == 0)
            return overrideNodeCodeGenerator.Generate(code, node, onlyOverride, sync: sync);
        
        var reference = referenceGenerator.Generate("ref");
        code.AppendLine($"{node.Type.FullName()} {reference};");
        
        if (maybeOverride is {} concreteOverrideNode)
        {
            code.AppendLine($"if ({contextGenerator.ParameterName}.{contextGenerator.OverridesPropertyName} is {sharedNameRegistry.IOverrideInterfaceName}<{concreteOverrideNode.Data.Type.FullName()}>)");
            code.AppendLine("{");
            overrideNodeCodeGenerator.Generate(code, node, concreteOverrideNode, sync: sync, reference: reference);
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
                GenerateSwitchBody(node, single.ConcreteNode, reference);
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
                    GenerateSwitchBody(node, concreteNode, reference);
                    code.AppendLine("}");
                }
            }
        }
        
        string GenerateSwitchBody(TypeNode typeNode, IConcreteNode concreteNode, string? maybeReference) =>
            concreteNode switch
            {
                ConcreteExceptionNode exceptionNode => exceptionNodeCodeGenerator.Generate(code, typeNode, exceptionNode, sync: sync, reference: maybeReference),
                ConcreteImplementationNode implementationNode => implementationNodeCodeGenerator.Generate(code, typeNode, implementationNode, sync: sync, reference: maybeReference),
                ConcreteFunctorNode functorNode => functorNodeCodeGenerator.Generate(code, typeNode, functorNode, sync: sync, reference: maybeReference),
                ConcreteInterfaceNode interfaceNode => interfaceNodeCodeGenerator.Generate(code, typeNode, interfaceNode, sync: sync, reference: maybeReference),
                ConcreteKeyValuePairNode keyValuePairNode => keyValuePairNodeCodeGenerator.Generate(code, typeNode, keyValuePairNode, sync: sync, reference: maybeReference),
                ConcreteEnumerableNode enumerableNode => enumerableNodeCodeGenerator.Generate(code, typeNode, enumerableNode, sync: sync, reference: maybeReference),
                ConcreteAsyncEnumerableNode asyncEnumerableNode => asyncEnumerableNodeCodeGenerator.Generate(code, typeNode, asyncEnumerableNode, sync: sync, reference: maybeReference),
                ConcreteOverrideNode overrideNode => overrideNodeCodeGenerator.Generate(code, typeNode, overrideNode, sync: sync, reference: maybeReference),
                ConcreteTaskNode taskNode => taskNodeCodeGenerator.Generate(code, typeNode, taskNode, sync: sync, reference: maybeReference),
                _ => ""
            };
        
        string GenerateContextMatchCondition(EdgeContext context)
        {
            var scopeNodeName = context.ScopeNode switch
            {
                ScopeNodeContext.Container => containerInfo.Name,
                ScopeNodeContext.Scope scope => scope.ScopeName,
                ScopeNodeContext.TransientScope transientScope => transientScope.TransientScopeName,
                _ => throw new ArgumentOutOfRangeException(nameof(context.ScopeNode))
            };

            var conditions = new List<string> { $"{contextGenerator.ParameterName}.{contextGenerator.ScopeNodeNamePropertyName} == \"{scopeNodeName}\"" };
            
            if (context.CaseChoice is CaseChoiceContext.Single(var outwardFacingTypeId, var caseId))
            {
                conditions.Add($"{contextGenerator.ParameterName}.{contextGenerator.OutwardFacingTypeNumberPropertyName} == {outwardFacingTypeId} && {contextGenerator.ParameterName}.{contextGenerator.CaseNumberPropertyName} == {caseId}");
            }

            if (context.Key is KeyContext.Single(var type, Value: var value))
            {
                var keyLiteral = keyUtility.GenerateKeyLiteral(type, value);
                conditions.Add($"{contextGenerator.ParameterName}.{contextGenerator.KeyPropertyName}?.Equals({keyLiteral}) == true");
            }

            return string.Join(" && ", conditions);
        }
    }
}
