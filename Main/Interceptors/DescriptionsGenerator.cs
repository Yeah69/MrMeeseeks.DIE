using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Descriptions;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Interceptors;

internal interface IDescriptionsGenerator
{
    string Generate();
}

internal class DescriptionsGenerator(
        IInvocationTypeManager invocationTypeManager, 
        IContainerWideContext containerWideContext) 
    : IDescriptionsGenerator
{
    private readonly WellKnownTypes _wellKnownTypes = containerWideContext.WellKnownTypes;
    
    public string Generate()
    {
        var code = new StringBuilder();
        
        code.AppendLine(
            $$"""
              namespace {{Constants.DescriptionsNamespace}}
              {
              """);

        foreach (var invocationDescriptionNode in invocationTypeManager.InvocationDescriptionNodes)
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
        
        foreach (var typeDescriptionNode in invocationTypeManager.TypeDescriptionNodes)
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
        
        foreach (var methodDescriptionNode in invocationTypeManager.MethodDescriptionNodes)
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