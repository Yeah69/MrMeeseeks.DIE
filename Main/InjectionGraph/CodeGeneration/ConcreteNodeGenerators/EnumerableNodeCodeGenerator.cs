using System.Globalization;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class EnumerableNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteEnumerableNode>, IScopeInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ContextGenerator _contextGenerator;
    private readonly KeyUtility _keyUtility;

    internal EnumerableNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ContextGenerator contextGenerator,
        KeyUtility keyUtility)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _contextGenerator = contextGenerator;
        _keyUtility = keyUtility;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteEnumerableNode concreteNode, string? reference = null)
    {
        var isArray = concreteNode.Data.EnumerableType is IArrayTypeSymbol;
        GenerateForResult(code, concreteNode, concreteNode.Data, isArray);
        return "";
    }

    private void GenerateForResult(
        StringBuilder code,
        ConcreteEnumerableNode enumerableNode,
        ConcreteEnumerableNodeData nodeData,
        bool isArray)
    {
        var references = GetResultReferences(code, enumerableNode, nodeData, isArray);

        code.AppendLine(isArray
            ? $"return new {enumerableNode.Data.EnumerableType.FullName()} {{ {string.Join(", ", references)} }};"
            : "yield break;");
    }

    private ImmutableArray<string> GetResultReferences(
        StringBuilder code,
        ConcreteEnumerableNode enumerableNode,
        ConcreteEnumerableNodeData nodeData,
        bool isArray)
    {
        if (nodeData.PurgeKeyAndChoice)
            code.AppendLine(_contextGenerator.GenerateCopyAssignment(outwardFacingTypeNumber: "0", caseNumber: "0", key: "null"));

        switch (nodeData)
        {
            case ConcreteEnumerableNodeData.Interface @interface:
                var interfacedSequence = @interface.Choices.Select(single =>
                {
                    code.AppendLine(_contextGenerator.GenerateCopyAssignment(
                        outwardFacingTypeNumber: single.OutwardFacingTypeId.ToString(CultureInfo.InvariantCulture),
                        caseNumber: single.CaseId.ToString(CultureInfo.InvariantCulture),
                        key: "null"));
                    var reference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge, enumerableNode.InnerEdge.Target);
                    if (!isArray)
                        code.AppendLine($"yield return {reference};");
                    return reference;
                });
                return [.. interfacedSequence];

            case ConcreteEnumerableNodeData.Key key:
                var keyedSequence = key.KeyValues.Select(value =>
                {
                    string keyLiteral = _keyUtility.GenerateKeyLiteral(key.KeyType, value);
                    code.AppendLine(_contextGenerator.GenerateCopyAssignment(outwardFacingTypeNumber: "0", caseNumber: "0", key: keyLiteral));
                    var reference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge, enumerableNode.InnerEdge.Target);
                    if (!isArray)
                        code.AppendLine($"yield return {reference};");
                    return reference;
                });
                return [.. keyedSequence];

            case ConcreteEnumerableNodeData.SinglePlainItem:
                var singlePlainItemReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge, enumerableNode.InnerEdge.Target);
                if (!isArray)
                    code.AppendLine($"yield return {singlePlainItemReference};");
                return [singlePlainItemReference];

            default:
                throw new ArgumentOutOfRangeException(nameof(nodeData));
        }
    }
}
