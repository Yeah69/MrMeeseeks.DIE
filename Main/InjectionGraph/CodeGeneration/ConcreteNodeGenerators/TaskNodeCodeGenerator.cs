using System.Threading.Tasks;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class TaskNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteTaskNode>, IScopeInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly GraphTypeHolder _graphTypeHolder;
    private readonly WellKnownTypes _wellKnownTypes;

    internal TaskNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ReferenceGenerator referenceGenerator,
        GraphTypeHolder graphTypeHolder,
        WellKnownTypes wellKnownTypes)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _referenceGenerator = referenceGenerator;
        _graphTypeHolder = graphTypeHolder;
        _wellKnownTypes = wellKnownTypes;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteTaskNode concreteNode, string? reference = null)
    {
        var actualReference = reference ?? _referenceGenerator.Generate(concreteNode.Data.TaskType);
        var innerReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, concreteNode.InnerEdge, concreteNode.InnerEdge.Target);
        var prefix = reference is null ? $"{concreteNode.Data.TaskType.FullName()} " : "";
        if (_graphTypeHolder.Type is GraphType.Sync)
        {
            if (concreteNode.InnerEdge.Type is FunctionEdgeType { Function: AsyncTypeNodeFunction { AsyncReturnType: { } asyncReturnType } })
                WrapIntoTaskTypeFromAsyncFunctionCall(asyncReturnType);
            else
                code.AppendLine(CustomSymbolEqualityComparer.Default.Equals(concreteNode.Data.TaskType.OriginalDefinition, _wellKnownTypes.ValueTask1)
                    ? $"{prefix}{actualReference} = new {concreteNode.Data.TaskType.FullName()}({_wellKnownTypes.Task.FullName()}.FromResult({innerReference}));"
                    : $"{prefix}{actualReference} = {_wellKnownTypes.Task.FullName()}.{nameof(Task.FromResult)}({innerReference});");
        }
        else if (concreteNode.InnerEdge.Type is FunctionEdgeType { Function: AsyncTypeNodeFunction { AsyncReturnType: { } asyncReturnType } })
        {
            code.AppendLine($"await {_wellKnownTypes.Task.FullName()}.{nameof(Task.Yield)}();");
            WrapIntoTaskTypeFromAsyncFunctionCall(asyncReturnType);
        }
        else
            throw new ArgumentException();
        return actualReference;

        void WrapIntoTaskTypeFromAsyncFunctionCall(INamedTypeSymbol asyncReturnType)
        {
            var innerIsValueTask = CustomSymbolEqualityComparer.Default.Equals(asyncReturnType.OriginalDefinition, _wellKnownTypes.ValueTask1);
            var outerIsValueTask = CustomSymbolEqualityComparer.Default.Equals(concreteNode.Data.TaskType.OriginalDefinition, _wellKnownTypes.ValueTask1);

            var assignedValue = (innerIsValueTask, outerIsValueTask) switch
            {
                (true, true) or (false, false) => innerReference,
                (true, false) => $"{innerReference}.{nameof(ValueTask<>.AsTask)}()",
                (false, true) => $"new {concreteNode.Data.TaskType.FullName()}({innerReference})"
            };
            
            code.AppendLine($"{prefix}{actualReference} = {assignedValue};");
        }
    }
}
