using System.Globalization;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class EnumerableNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteEnumerableNode>, IContainerInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ScopeNodeContext.Container _containerScopeNodeContext;
    private readonly ContextGenerator _contextGenerator;
    private readonly KeyUtility _keyUtility;

    internal EnumerableNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ScopeNodeContext.Container containerScopeNodeContext,
        ContextGenerator contextGenerator,
        KeyUtility keyUtility)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _containerScopeNodeContext = containerScopeNodeContext;
        _contextGenerator = contextGenerator;
        _keyUtility = keyUtility;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteEnumerableNode concreteNode)
    {
        /*if (concreteNode.CollectionCases.Count > 1)
        {
            // ToDo implement this
            throw new NotImplementedException("More than one sequence found in enumerable node.");
        }*/

        var isArray = concreteNode.Data.Enumerable is IArrayTypeSymbol;

        // TODO: Fix this workaround - should use the correct ScopeNodeContext for the current scope being generated,
        // not just the first entry. The issue is that CollectionCases is keyed by ScopeNodeContext (Container/TransientScope/Scope)
        // but we're always using Container, which may not exist if the enumerable is only used in other scopes.
        var cases = concreteNode.CollectionCases.First().Value;
        var maybeDefaultCase = cases.TryGetValue(new KeyContext.None1(), out var foundDefaultCase)
            ? foundDefaultCase
            : null;
        var keyedCases = cases
            .Where(kvp => kvp.Key != new KeyContext.None1())
            .Select(kvp => (KeyContext: (KeyContext.Single)kvp.Key, Result: kvp.Value))
            .ToImmutableArray();

        foreach (var keyedCase in keyedCases)
        {
            var keyLiteral = _keyUtility.GenerateKeyLiteral(keyedCase.KeyContext.Type, keyedCase.KeyContext.Value);
            code.AppendLine(
                $$"""
                  if ({{_contextGenerator.ParameterName}}.{{_contextGenerator.KeyPropertyName}} == {{keyLiteral}})
                  {
                  """);

            GenerateForResult(code, concreteNode, keyedCase.Result, isArray);

            code.AppendLine("}");
        }

        if (maybeDefaultCase is not null)
            GenerateForResult(code, concreteNode, maybeDefaultCase, isArray);

        code.AppendLine("throw new System.Exception(\"Should be impossible\");");

        return "";
    }

    private void GenerateForResult(
        StringBuilder code,
        ConcreteEnumerableNode enumerableNode,
        ConcreteEnumerableResult result,
        bool isArray)
    {
        var references = GetResultReferences(code, enumerableNode, result, isArray);

        code.AppendLine(isArray
            ? $"return new {enumerableNode.Data.Enumerable.FullName()} {{ {string.Join(", ", references)} }};"
            : "yield break;");
    }

    private ImmutableArray<string> GetResultReferences(
        StringBuilder code,
        ConcreteEnumerableNode enumerableNode,
        ConcreteEnumerableResult result,
        bool isArray)
    {
        if (result.PurgeKeyAndChoice)
            code.AppendLine(_contextGenerator.GenerateCopyAssignment(outwardFacingTypeNumber: "0", caseNumber: "0", key: "null"));

        switch (result)
        {
            case ConcreteEnumerableResult.Interface @interface:
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

            case ConcreteEnumerableResult.Key key:
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

            case ConcreteEnumerableResult.SinglePlainItem:
                string singlePlainItemReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, enumerableNode.InnerEdge, enumerableNode.InnerEdge.Target);
                if (!isArray)
                    code.AppendLine($"yield return {singlePlainItemReference};");
                return [singlePlainItemReference];

            default:
                throw new ArgumentOutOfRangeException(nameof(result));
        }
    }
}
