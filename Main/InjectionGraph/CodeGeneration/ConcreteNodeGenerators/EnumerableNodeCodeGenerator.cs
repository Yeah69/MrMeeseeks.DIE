using System.Globalization;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal abstract class EnumerableNodeCodeGeneratorBase(
    Lazy<InjectionNodeGenerator> injectionNodeGenerator,
    ContextGenerator contextGenerator,
    KeyUtility keyUtility)
{
    protected ImmutableArray<string> GetResultReferences(
        StringBuilder code,
        ConcreteEnumerableNodeBase enumerableNode,
        ConcreteEnumerableNodeData nodeData,
        bool isArray,
        bool sync)
    {
        if (nodeData.PurgeKeyAndChoice)
            code.AppendLine(contextGenerator.GenerateCopyAssignment(outwardFacingTypeNumber: "0", caseNumber: "0", key: "null"));

        switch (nodeData)
        {
            case ConcreteEnumerableNodeData.Interface @interface:
                var interfacedSequence = @interface.Choices.Select(single =>
                {
                    code.AppendLine(contextGenerator.GenerateCopyAssignment(
                        outwardFacingTypeNumber: single.OutwardFacingTypeId.ToString(CultureInfo.InvariantCulture),
                        caseNumber: single.CaseId.ToString(CultureInfo.InvariantCulture),
                        key: "null"));
                    var reference = injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge.Target, enumerableNode.InnerEdge.Source, sync: sync);
                    if (!isArray)
                        code.AppendLine($"yield return {reference};");
                    return reference;
                });
                return [.. interfacedSequence];

            case ConcreteEnumerableNodeData.Key key:
                var keyedSequence = key.KeyValues.Select(value =>
                {
                    string keyLiteral = keyUtility.GenerateKeyLiteral(key.KeyType, value);
                    code.AppendLine(contextGenerator.GenerateCopyAssignment(outwardFacingTypeNumber: "0", caseNumber: "0", key: keyLiteral));
                    var reference = injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge.Target, enumerableNode.InnerEdge.Source, sync: sync);
                    if (!isArray)
                        code.AppendLine($"yield return {reference};");
                    return reference;
                });
                return [.. keyedSequence];

            case ConcreteEnumerableNodeData.SinglePlainItem:
                var singlePlainItemReference = injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge.Target, enumerableNode.InnerEdge.Source, sync: sync);
                if (!isArray)
                    code.AppendLine($"yield return {singlePlainItemReference};");
                return [singlePlainItemReference];

            default:
                throw new ArgumentOutOfRangeException(nameof(nodeData));
        }
    }
}

internal sealed class EnumerableNodeCodeGenerator(
    Lazy<InjectionNodeGenerator> injectionNodeGenerator,
    ContextGenerator contextGenerator,
    KeyUtility keyUtility) 
    : EnumerableNodeCodeGeneratorBase(injectionNodeGenerator, contextGenerator, keyUtility), IConcreteNodeCodeGenerator<ConcreteEnumerableNode>, IScopeInstance
{
    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteEnumerableNode concreteNode, bool sync, string? reference = null)
    {
        var isArray = concreteNode.Data.EnumerableType is IArrayTypeSymbol;
        GenerateForResult(code, concreteNode, concreteNode.Data, isArray, sync: sync);
        return "";
    }

    private void GenerateForResult(
        StringBuilder code,
        ConcreteEnumerableNode enumerableNode,
        ConcreteEnumerableNodeData nodeData,
        bool isArray,
        bool sync)
    {
        var references = GetResultReferences(code, enumerableNode, nodeData, isArray, sync: sync);

        code.AppendLine(isArray
            ? $"return new {enumerableNode.Data.EnumerableType.FullName()} {{ {string.Join(", ", references)} }};"
            : "yield break;");
    }
}

internal sealed class AsyncEnumerableNodeCodeGenerator(
    Lazy<InjectionNodeGenerator> injectionNodeGenerator,
    ContextGenerator contextGenerator,
    KeyUtility keyUtility) 
    : EnumerableNodeCodeGeneratorBase(injectionNodeGenerator, contextGenerator, keyUtility), IConcreteNodeCodeGenerator<ConcreteAsyncEnumerableNode>, IScopeInstance
{
    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteAsyncEnumerableNode concreteNode, bool sync, string? reference = null)
    {
        var isArray = sync;
        GenerateForResult(code, concreteNode, concreteNode.Data, isArray, sync: sync);
        return "";
    }

    private void GenerateForResult(
        StringBuilder code,
        ConcreteAsyncEnumerableNode enumerableNode,
        ConcreteEnumerableNodeData nodeData,
        bool isArray,
        bool sync)
    {
        var references = GetResultReferences(code, enumerableNode, nodeData, isArray, sync: sync);

        code.AppendLine(isArray
            ? $"return new {enumerableNode.Data.EnumerableType.FullName()} {{ {string.Join(", ", references)} }}.ToAsyncEnumerable();"
            : "yield break;");
    }
}
