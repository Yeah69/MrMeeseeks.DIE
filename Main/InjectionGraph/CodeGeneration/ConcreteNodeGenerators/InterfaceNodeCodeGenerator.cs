using System.Globalization;
using MrMeeseeks.DIE.InjectionGraph.Edges;
using MrMeeseeks.DIE.InjectionGraph.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.InjectionGraph.CodeGeneration.ConcreteNodeGenerators;

internal sealed class InterfaceNodeCodeGenerator : IConcreteNodeCodeGenerator<ConcreteInterfaceNode>, IScopeInstance
{
    private readonly Lazy<InjectionNodeGenerator> _injectionNodeGenerator;
    private readonly ReferenceGenerator _referenceGenerator;
    private readonly ContextGenerator _contextGenerator;
    private readonly KeyUtility _keyUtility;
    private readonly ContainerInfo _containerInfo;
    private readonly WellKnownTypes _wellKnownTypes;

    internal InterfaceNodeCodeGenerator(
        Lazy<InjectionNodeGenerator> injectionNodeGenerator,
        ReferenceGenerator referenceGenerator,
        ContextGenerator contextGenerator,
        KeyUtility keyUtility,
        ContainerInfo containerInfo,
        WellKnownTypes wellKnownTypes)
    {
        _injectionNodeGenerator = injectionNodeGenerator;
        _referenceGenerator = referenceGenerator;
        _contextGenerator = contextGenerator;
        _keyUtility = keyUtility;
        _containerInfo = containerInfo;
        _wellKnownTypes = wellKnownTypes;
    }

    public string Generate(StringBuilder code, TypeNode typeNode, ConcreteInterfaceNode concreteNode, string? reference = null)
    {
        var actualReference = reference ?? _referenceGenerator.Generate(concreteNode.Data.Interface);
        var declarationPrefix = reference is null ? $"{concreteNode.Data.Interface.FullName()} " : "";
        if (concreteNode.TypeCases.Count() == 1)
        {
            var typeCase = concreteNode.TypeCases.First();
            var innerReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, typeCase.Edge, typeCase.Edge.Target);
            code.AppendLine($"{declarationPrefix}{actualReference} = ({concreteNode.Data.Interface.FullName()}) {innerReference};");
            return actualReference;
        }
        
        var typeCaseReference = _referenceGenerator.Generate("typeCase");
        var first = true;
        code.AppendLine($"int {typeCaseReference};");
        foreach (var keyObjectToChainCase in concreteNode.KeyObjectToChainCase)
        {
            if (first)
                first = false;
            else
                code.Append("else ");
            var chainCase = keyObjectToChainCase.NextChainCase;
            var typeCase = keyObjectToChainCase.NextTypeCase;
            
            var keyLiteral = _keyUtility.GenerateKeyLiteral(keyObjectToChainCase.KeyType, keyObjectToChainCase.KeyObject);
            code.AppendLine(
                $$"""
                  if ({{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} != {{concreteNode.Number}} && ({{_contextGenerator.ParameterName}}.{{_contextGenerator.KeyPropertyName}}?.Equals({{keyLiteral}}) ?? false))
                  {
                  {{_contextGenerator.GenerateCopyAssignment(
                          key: "null",
                          outwardFacingTypeNumber: chainCase is 0 ? "0" : concreteNode.Number.ToString(CultureInfo.InvariantCulture),
                          caseNumber: chainCase.ToString(CultureInfo.InvariantCulture))}}
                  {{typeCaseReference}} = {{typeCase}};
                  }
                  """);
        }
        foreach (var initialChainCase in concreteNode.InitialChainCase)
        {
            if (first)
                first = false;
            else
                code.Append("else ");
            var chainCase = initialChainCase.NextChainCase;
            var typeCase = initialChainCase.NextTypeCase;
            var scopeNodeName = initialChainCase.Node switch
            {
                ScopeNodeContext.Container => _containerInfo.Name,
                ScopeNodeContext.Scope scope => scope.ScopeName,
                ScopeNodeContext.TransientScope transientScope => transientScope.TransientScopeName,
                _ => throw new ArgumentOutOfRangeException(nameof(initialChainCase.Node))
            };
            
            code.AppendLine(
                $$"""
                  if ({{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} != {{concreteNode.Number}} && {{_contextGenerator.ParameterName}}.{{_contextGenerator.ScopeNodeNamePropertyName}} == "{{scopeNodeName}}")
                  {
                  {{_contextGenerator.GenerateCopyAssignment(
                      key: "null",
                      outwardFacingTypeNumber: chainCase is 0 ? "0" : concreteNode.Number.ToString(CultureInfo.InvariantCulture),
                      caseNumber: chainCase.ToString(CultureInfo.InvariantCulture))}}
                  {{typeCaseReference}} = {{typeCase}};
                  }
                  """);
        }
        foreach (var nextChainCase in concreteNode.NextChainCases)
        {
            if (first)
                first = false;
            else
                code.Append("else ");
            var currentChainCase = nextChainCase.CurrentChainCase;
            var chainCase = nextChainCase.NextChainCase;
            var typeCase = nextChainCase.NextTypeCase;
            
            code.AppendLine(
                $$"""
                  if ({{_contextGenerator.ParameterName}}.{{_contextGenerator.OutwardFacingTypeNumberPropertyName}} == {{concreteNode.Number}} && {{_contextGenerator.ParameterName}}.{{_contextGenerator.CaseNumberPropertyName}} == {{currentChainCase}})
                  {
                  {{_contextGenerator.GenerateCopyAssignment(
                      key: "null",
                      outwardFacingTypeNumber: chainCase is 0 ? "0" : concreteNode.Number.ToString(CultureInfo.InvariantCulture),
                      caseNumber: chainCase.ToString(CultureInfo.InvariantCulture))}}
                  {{typeCaseReference}} = {{typeCase}};
                  }
                  """);
        }
        code.AppendLine(
            $$"""
              else
              {
              throw new {{_wellKnownTypes.Exception.FullName()}}("Bug in DIE generator. This exception should be impossible. Please (re)open an issue ticket with the UUID {{new Guid("0B359FFF-73AD-4A2D-B2AD-D01CCD3EC2F6").ToString()}} in https://github.com/Yeah69/MrMeeseeks.DIE/issues .");
              }
              """);

        first = true;
        code.AppendLine($"{declarationPrefix}{actualReference};");


        foreach (var typeCase in concreteNode.TypeCases)
        {
            if (first)
                first = false;
            else
                code.Append("else ");
            code.AppendLine(
                $$"""
                  if ({{typeCaseReference}} == {{typeCase.TypeCase}})
                  {
                  """);
            var innerReference = _injectionNodeGenerator.Value.CallFunctionOrGenerateForInjectionNode(code, typeCase.Edge, typeCase.Edge.Target);
            code.AppendLine(
                $$"""
                  {{actualReference}} = ({{concreteNode.Data.Interface.FullName()}}) {{innerReference}};
                  }
                  """);
        }
        code.AppendLine(
            $$"""
              else
              {
              throw new {{_wellKnownTypes.Exception.FullName()}}("Bug in DIE generator. This exception should be impossible. Please (re)open an issue ticket with the UUID {{new Guid("1A6D85F0-8B49-4AA6-BD03-3BC8A026A6EC").ToString()}} in https://github.com/Yeah69/MrMeeseeks.DIE/issues .");
              }
              """);

        return actualReference;
    }
}
