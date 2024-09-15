using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Interception;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IDescriptionsGenerator
{
    string? Generate();
}

internal sealed class DescriptionsGenerator : IDescriptionsGenerator
{
    private readonly IInvocationTypeManager _invocationTypeManager;
    private readonly WellKnownTypes _wellKnownTypes;

    internal DescriptionsGenerator(IInvocationTypeManager invocationTypeManager, 
        WellKnownTypes wellKnownTypes)
    {
        _invocationTypeManager = invocationTypeManager;
        _wellKnownTypes = wellKnownTypes;
    }

    public string? Generate()
    {
        if (_invocationTypeManager.InvocationDescriptionNodes.Count == 0
            && _invocationTypeManager.TypeDescriptionNodes.Count == 0
            && _invocationTypeManager.MethodDescriptionNodes.Count == 0)
            return null;
        
        var code = new StringBuilder();
        
        code.AppendLine(
            $$"""
              namespace {{Constants.DescriptionsNamespace}}
              {
              """);

        foreach (var invocationDescriptionNode in _invocationTypeManager.InvocationDescriptionNodes)
        {
            code.AppendLine(
                $"internal record {invocationDescriptionNode.Name}({string.Join(", ", GetParameters(invocationDescriptionNode))}) : {invocationDescriptionNode.InterfaceFullName};");
            continue;

            static IEnumerable<string> GetParameters(IInvocationDescriptionNode invocationDescriptionNode)
            {
                if (invocationDescriptionNode.TargetTypeProperty is not null)
                    yield return $"{invocationDescriptionNode.TargetTypeProperty.FullName()} TargetType";
                if (invocationDescriptionNode.TargetMethodProperty is not null)
                    yield return $"{invocationDescriptionNode.TargetMethodProperty.FullName()} TargetMethod";
            }
        }
        
        foreach (var typeDescriptionNode in _invocationTypeManager.TypeDescriptionNodes)
        {
            code.AppendLine(
                $"internal record {typeDescriptionNode.Name}({string.Join(", ", GetParameters(typeDescriptionNode, _wellKnownTypes))}) : {typeDescriptionNode.InterfaceFullName};");
            continue;

            static IEnumerable<string> GetParameters(ITypeDescriptionNode typeDescriptionNode, WellKnownTypes wellKnownTypes)
            {
                if (typeDescriptionNode.FullNameProperty)
                    yield return $"{wellKnownTypes.String.FullName()} FullName";
                if (typeDescriptionNode.NameProperty)
                    yield return $"{wellKnownTypes.String.FullName()} Name";
            }
        }
        
        foreach (var methodDescriptionNode in _invocationTypeManager.MethodDescriptionNodes)
        {
            code.AppendLine(
                $"internal record {methodDescriptionNode.Name}({string.Join(", ", GetParameters(methodDescriptionNode, _wellKnownTypes))}) : {methodDescriptionNode.InterfaceFullName};");
            continue;

            static IEnumerable<string> GetParameters(IMethodDescriptionNode methodDescriptionNode, WellKnownTypes wellKnownTypes)
            {
                if (methodDescriptionNode.NameProperty)
                    yield return $"{wellKnownTypes.String.FullName()} Name";
                if (methodDescriptionNode.ReturnTypeProperty is not null)
                    yield return $"{methodDescriptionNode.ReturnTypeProperty.FullName()} ReturnType";
            }
        }
        
        code.AppendLine("}");
        
        return code.ToString();
    }
}