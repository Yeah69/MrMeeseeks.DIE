using System.Threading.Tasks;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
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
    FunctionUtility functionUtility,
    TaskNodeCodeGenerator taskNodeCodeGenerator,
    WellKnownTypes wellKnownTypes)
    : IScopeInstance
{
    internal string GenerateForInjectionNode(StringBuilder code, TypeNode node, bool sync)
    {
        var edgeAndTargets = node.OutgoingConcreteEdges
            .Where(e => sync && e is ConcreteSyncEdge 
                        || !sync && e is ConcreteAsyncEdge
                        || e.Target is ConcreteTaskNode concreteTaskNode && GetSyncToAsyncForTaskNodeCase(concreteTaskNode) is not null
                        || e.Target is ConcreteAsyncEnumerableNode && GetSyncToAsyncForAsyncEnumerableNodeCase() is not null)
            .Select(e => (Edges: e, ConcreteNode: e.Target))
            .ToList();
        var maybeOverride = edgeAndTargets.Select(t => t.ConcreteNode).OfType<ConcreteOverrideNode>().SingleOrDefault();
        var nonOverrides = edgeAndTargets.Where(t => t.ConcreteNode is not ConcreteOverrideNode).ToList();
        
        if (maybeOverride is null && nonOverrides.Count == 1)
            return GenerateSwitchBody(nonOverrides.Single().ConcreteNode, null);
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
                GenerateSwitchBody(single.ConcreteNode, reference);
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
                    GenerateSwitchBody(concreteNode, reference);
                    code.AppendLine("}");
                }
            }
        }
        
        string GenerateSwitchBody(IConcreteNode concreteNode, string? maybeReference) =>
            concreteNode switch
            {
                ConcreteExceptionNode exceptionNode => exceptionNodeCodeGenerator.Generate(code, node, exceptionNode, sync: sync, reference: maybeReference),
                ConcreteImplementationNode implementationNode => implementationNodeCodeGenerator.Generate(code, node, implementationNode, sync: sync, reference: maybeReference),
                ConcreteFunctorNode functorNode => functorNodeCodeGenerator.Generate(code, node, functorNode, sync: sync, reference: maybeReference),
                ConcreteInterfaceNode interfaceNode => interfaceNodeCodeGenerator.Generate(code, node, interfaceNode, sync: sync, reference: maybeReference),
                ConcreteKeyValuePairNode keyValuePairNode => keyValuePairNodeCodeGenerator.Generate(code, node, keyValuePairNode, sync: sync, reference: maybeReference),
                ConcreteEnumerableNode enumerableNode => enumerableNodeCodeGenerator.Generate(code, node, enumerableNode, sync: sync, reference: maybeReference),
                ConcreteAsyncEnumerableNode asyncEnumerableNode => GenerateSwitchBodyAsyncEnumerableNodeCase(asyncEnumerableNode, maybeReference),
                ConcreteOverrideNode overrideNode => overrideNodeCodeGenerator.Generate(code, node, overrideNode, sync: sync, reference: maybeReference),
                ConcreteTaskNode taskNode => GenerateSwitchBodyTaskNodeCase(taskNode, maybeReference),
                _ => ""
            };

        string GenerateSwitchBodyTaskNodeCase(ConcreteTaskNode taskNode, string? maybeReference)
        {
            if (GetSyncToAsyncForTaskNodeCase(taskNode) is not var (asyncFunction, asyncReturnType))
                return taskNodeCodeGenerator.Generate(code, node, taskNode, sync: sync, reference: maybeReference);
            
            var innerIsValueTask = CustomSymbolEqualityComparer.Default.Equals(asyncReturnType.OriginalDefinition, wellKnownTypes.ValueTask1);
            var outerIsValueTask = CustomSymbolEqualityComparer.Default.Equals(node.Type.OriginalDefinition, wellKnownTypes.ValueTask1);

            var innerReference = CallFunction(asyncFunction, prefix: asyncReturnType.FullName(), await: false, maybeReference);
            
            if (innerIsValueTask == outerIsValueTask)
                return innerReference;
            
            var outerReference = maybeReference ?? referenceGenerator.Generate(node.Type);
            var outerFullName = node.Type.FullName();

            var assignedValue = outerIsValueTask && !innerIsValueTask
                ? $"new {outerFullName}({innerReference})"
                : $"{innerReference}.{nameof(ValueTask<>.AsTask)}()";
            
            code.AppendLine($"{outerFullName} {outerReference} = {assignedValue};");
            return outerReference;
        }

        (ITypeNodeFunction Function, ITypeSymbol ReturnType)? GetSyncToAsyncForTaskNodeCase(ConcreteTaskNode taskNode) =>
            sync && taskNode is { InnerEdge.TargetAsNode: TypeNode { AsyncFunction: { AsyncReturnType: { } asyncReturnType } asyncFunction } }
                ? (asyncFunction, asyncReturnType)
                : null;

        string GenerateSwitchBodyAsyncEnumerableNodeCase(ConcreteAsyncEnumerableNode asyncEnumerableNode, string? maybeReference) =>
            GetSyncToAsyncForAsyncEnumerableNodeCase() is var (asyncFunction, asyncReturnType)
                ? CallFunction(asyncFunction, asyncReturnType.FullName(), await: false, maybeReference)
                : asyncEnumerableNodeCodeGenerator.Generate(code, node, asyncEnumerableNode, sync: sync, reference: maybeReference);

        (ITypeNodeFunction Function, ITypeSymbol ReturnType)? GetSyncToAsyncForAsyncEnumerableNodeCase() =>
            sync && node is { Outgoing: [{ TargetAsNode: ConcreteAsyncEnumerableNode }], AsyncFunction: { AsyncReturnType: {} asyncReturnType } asyncFunction}
                ? (asyncFunction, asyncReturnType)
                : null;

        string CallFunction(ITypeNodeFunction function, string prefix, bool await, string? maybeReference)
        {
            var resultReference = maybeReference ?? referenceGenerator.Generate(function.RootNode.Type);
            var maybeAwait = await
                ? "await "
                : " ";
            var leftSide = maybeReference is not null
                ? resultReference
                : $"{prefix} {resultReference}";
            code.AppendLine($"{leftSide} = {maybeAwait}{functionUtility.GenerateFunctionCall(function, doScopedInstance: true, doScopeRoot: true)};");
            return resultReference;
        }
        
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
