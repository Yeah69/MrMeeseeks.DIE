using MrMeeseeks.DIE.Configuration.Interception;

namespace MrMeeseeks.DIE.CodeGeneration;

internal interface IDescriptionsGenerator
{
    string? Generate();
}

internal sealed class DescriptionsGenerator : IDescriptionsGenerator
{
    private readonly IInvocationTypeManager _invocationTypeManager;
    private readonly WellKnownTypes _wellKnownTypes;

    internal DescriptionsGenerator(
        IInvocationTypeManager invocationTypeManager, 
        WellKnownTypes wellKnownTypes)
    {
        _invocationTypeManager = invocationTypeManager;
        _wellKnownTypes = wellKnownTypes;
    }

    public string? Generate()
    {
        if (_invocationTypeManager.InvocationDescriptionNodes.Count == 0)
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
                $$"""
                  internal class {{invocationDescriptionNode.Name}} : {{invocationDescriptionNode.InterfaceFullName}}
                  {
                  """);
            if (invocationDescriptionNode.ArgumentsProperty)
            {
                var nullable = invocationDescriptionNode.ArgumentsItemTypeNullable ? "?" : "";
                code.AppendLine($"public required {_wellKnownTypes.Object}{nullable}[] Arguments {{ get; init; }}");
            }
            if (invocationDescriptionNode.GenericArgumentsProperty)
            {
                code.AppendLine($"public required {_wellKnownTypes.Type}[] GenericArguments {{ get; init; }}");
            }
            if (invocationDescriptionNode.InvocationTargetProperty)
            {
                code.AppendLine($"public required {_wellKnownTypes.Object} InvocationTarget {{ get; init; }}");
            }
            if (invocationDescriptionNode.MethodProperty)
            {
                code.AppendLine($"public required {_wellKnownTypes.MethodInfo} Method {{ get; init; }}");
            }
            if (invocationDescriptionNode.MethodInvocationTargetProperty)
            {
                code.AppendLine($"public required {_wellKnownTypes.MethodInfo} MethodInvocationTarget {{ get; init; }}");
            }
            if (invocationDescriptionNode.ProxyProperty)
            {
                code.AppendLine($"public required {_wellKnownTypes.Object} Proxy {{ get; init; }}");
            }
            if (invocationDescriptionNode.ReturnValueProperty)
            {
                var nullable = invocationDescriptionNode.ReturnValueTypeNullable ? "?" : "";
                code.AppendLine($"public required {_wellKnownTypes.Object}{nullable} ReturnValue {{ get; internal set; }}");
            }
            if (invocationDescriptionNode.TargetTypeProperty)
            {
                code.AppendLine($"public required {_wellKnownTypes.Type} TargetType {{ get; init; }}");
            }
            if (invocationDescriptionNode.ProceedMethod)
            {
                code.AppendLine(
                    $$"""
                      internal {{_wellKnownTypes.Action}}? ProceedAction { private get; set; } = null!;
                      public void Proceed() => ProceedAction?.Invoke();
                      """);
            }
            code.AppendLine("}");
        }
        
        code.AppendLine("}");
        
        return code.ToString();
    }
}