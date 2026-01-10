using System.Globalization;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class InterfaceNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteInterfaceNode>, IContainerInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly ContextGenerator _contextGenerator;
    private readonly KeyUtility _keyUtility;
    private readonly WellKnownTypes _wellKnownTypes;

    internal InterfaceNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ReferenceGenerator referenceGenerator,
        ContextGenerator contextGenerator,
        KeyUtility keyUtility,
        WellKnownTypes wellKnownTypes)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _referenceGenerator = referenceGenerator;
        _contextGenerator = contextGenerator;
        _keyUtility = keyUtility;
        _wellKnownTypes = wellKnownTypes;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteInterfaceNode concreteNode)
    {
        if (concreteNode.DefaultImplementationsCaseNumbers.Any() || concreteNode.KeyObjectToCaseNumbers.Any())
        {
            code.AppendLine(
                $$"""
                  if ({{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} != {{concreteNode.Number}})
                  {
                  """);

            var firstKeyObjectToCaseNumber = true;
            foreach (var keyObjectToCaseNumber in concreteNode.KeyObjectToCaseNumbers)
            {
                var caseNumber = keyObjectToCaseNumber.NextId;
                if (firstKeyObjectToCaseNumber)
                    firstKeyObjectToCaseNumber = false;
                else
                    code.Append("else ");

                var keyLiteral = _keyUtility.GenerateKeyLiteral(keyObjectToCaseNumber.KeyType, keyObjectToCaseNumber.KeyObject);
                code.AppendLine(
                    $$"""
                      if ({{_contextGenerator.ParameterName}}.{{_contextGenerator.KeyPropertyName}}?.Equals({{keyLiteral}}) ?? false)
                      {
                      {{_contextGenerator.GenerateCopyAssignment(
                          key: "null",
                          outwardFacingTypeNumber: concreteNode.Number.ToString(CultureInfo.InvariantCulture),
                          caseNumber: caseNumber.ToString(CultureInfo.InvariantCulture))}}
                      }
                      """);
            }

            if (concreteNode.DefaultImplementationsCaseNumbers.Any())
            {
                var line = _contextGenerator.GenerateCopyAssignment(
                    outwardFacingTypeNumber: concreteNode.Number.ToString(CultureInfo.InvariantCulture),
                    caseNumber: concreteNode.DefaultImplementationsCaseNumbers.First().NextId.ToString(CultureInfo.InvariantCulture));
                code.AppendLine(
                    concreteNode.KeyObjectToCaseNumbers.Any()
                        ? $$"""
                            else
                            {
                            {{line}}
                            }
                            """
                        : line);
            }

            code.AppendLine("}");
        }

        var reference = _referenceGenerator.Generate(concreteNode.Data.Interface);
        code.AppendLine($"{concreteNode.Data.Interface.FullName()} {reference};");

        var first = true;
        foreach (var interfaceNodeCase in concreteNode.Cases)
        {
            var ifKeyword = "else if";
            if (first)
            {
                ifKeyword = "if";
                first = false;
            }

            code.AppendLine(
                $$"""
                  {{ifKeyword}} ({{_contextGenerator.ParameterName}}.{{_contextGenerator.CaseNumberPropertyName}} == {{interfaceNodeCase.Id}})
                  {
                  """);

            var newInterfaceNumber = interfaceNodeCase.NextId == 0 ? 0 : concreteNode.Number;
            code.AppendLine(_contextGenerator.GenerateCopyAssignment(
                outwardFacingTypeNumber: newInterfaceNumber.ToString(CultureInfo.InvariantCulture),
                caseNumber: interfaceNodeCase.NextId.ToString(CultureInfo.InvariantCulture)));

            var innerReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, interfaceNodeCase.Edge, interfaceNodeCase.Edge.Target);

            code.AppendLine(
                $$"""
                  {{reference}} = ({{concreteNode.Data.Interface.FullName()}}) {{innerReference}};
                  }
                  """);
        }

        code.AppendLine(
            $$"""
              else
              {
              throw new {{_wellKnownTypes.Exception.FullName()}}("Bug in DIE generator. This exception should be impossible. Please (re)open an issue ticket with the UUID {{new Guid("E8922F27-2F80-4334-B152-667441D8D3C3").ToString()}} in https://github.com/Yeah69/MrMeeseeks.DIE/issues .");
              }
              """);

        return reference;
    }
}
