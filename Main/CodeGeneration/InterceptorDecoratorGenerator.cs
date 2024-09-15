using MrMeeseeks.DIE.Configuration;

namespace MrMeeseeks.DIE.CodeGeneration;

internal interface IInterceptorDecoratorGenerator
{
    string? Generate();
}

internal class InterceptorDecoratorGenerator : IInterceptorDecoratorGenerator
{
    private readonly IInvocationTypeManager _invocationTypeManager;
    private readonly IReferenceGenerator _referenceGenerator;

    internal InterceptorDecoratorGenerator(
        // parameters
        
        // dependencies
        IInvocationTypeManager invocationTypeManager,
        IReferenceGenerator referenceGenerator)
    {
        _invocationTypeManager = invocationTypeManager;
        _referenceGenerator = referenceGenerator;
    }
    
    public string? Generate()
    {
        var interceptionDecoratorDatas = _invocationTypeManager.InterceptorBasedDecoratorTypes.ToList();
        if (interceptionDecoratorDatas.Count == 0)
            return null;
        
        var code = new StringBuilder();
        code.AppendLine("#nullable enable");
        code.AppendLine($"namespace {Constants.NamespaceForGeneratedUtilities}");
        code.AppendLine("{");
        
        foreach (var interceptionDecoratorData in interceptionDecoratorDatas)
        {
            var interfaceConstructorParameterReference = _referenceGenerator.Generate("interface");
            var interceptorConstructorParameterReference = _referenceGenerator.Generate("interceptor");
            code.AppendLine($"internal class {interceptionDecoratorData.Name} : {interceptionDecoratorData.InterfaceFullName}");
            code.AppendLine("{");
            code.AppendLine($"private readonly {interceptionDecoratorData.InterfaceFullName} {interceptionDecoratorData.InterfaceFieldReference};");
            code.AppendLine($"private readonly {interceptionDecoratorData.InterceptorFullName} {interceptionDecoratorData.InterceptorFieldReference};");
            code.AppendLine($"internal {interceptionDecoratorData.Name}({interceptionDecoratorData.InterceptorFullName} {interceptorConstructorParameterReference}, {interceptionDecoratorData.InterfaceFullName} {interfaceConstructorParameterReference})");
            code.AppendLine("{");
            code.AppendLine($"{interceptionDecoratorData.InterfaceFieldReference} = {interfaceConstructorParameterReference};");
            code.AppendLine($"{interceptionDecoratorData.InterceptorFieldReference} = {interceptorConstructorParameterReference};");
            code.AppendLine("}");
            
            foreach (var implementation in interceptionDecoratorData.Implementations)
            {
                switch (implementation)
                {
                    case DelegationPropertyImplementation delegationPropertyImplementation:
                        code.AppendLine($"{delegationPropertyImplementation.TypeFullName} {delegationPropertyImplementation.DeclaringInterfaceFullName}.{delegationPropertyImplementation.Name}");
                        code.AppendLine("{");
                        if (delegationPropertyImplementation.HasGetter)
                            code.AppendLine($"get {{ return (({delegationPropertyImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationPropertyImplementation.Name}; }}");
                        if (delegationPropertyImplementation.HasSetter)
                            code.AppendLine($"set {{ (({delegationPropertyImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationPropertyImplementation.Name} = value; }}");
                        code.AppendLine("}");
                        break;
                    case DelegationIndexerImplementation delegationIndexerImplementation:
                        code.AppendLine($"{delegationIndexerImplementation.TypeFullName} {delegationIndexerImplementation.DeclaringInterfaceFullName}.this[{string.Join(", ", delegationIndexerImplementation.Parameters.Select(p => $"{p.TypeFullName} {p.Name}"))}]");
                        code.AppendLine("{");
                        if (delegationIndexerImplementation.HasGetter)
                            code.AppendLine($"get {{ return (({delegationIndexerImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference})[{string.Join(", ", delegationIndexerImplementation.Parameters.Select(p => p.Name))}]; }}");
                        if (delegationIndexerImplementation.HasSetter)
                            code.AppendLine($"set {{ (({delegationIndexerImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference})[{string.Join(", ", delegationIndexerImplementation.Parameters.Select(p => p.Name))}] = value; }}");
                        code.AppendLine("}");
                        break;
                    case DelegationMethodImplementation delegationMethodImplementation:
                        var genericsPart = delegationMethodImplementation.GenericTypeParameters.Count == 0
                            ? string.Empty
                            : $"<{string.Join(", ", delegationMethodImplementation.GenericTypeParameters)}>";
                        code.AppendLine($"{delegationMethodImplementation.TypeFullName} {delegationMethodImplementation.DeclaringInterfaceFullName}.{delegationMethodImplementation.Name}{genericsPart}({string.Join(", ", delegationMethodImplementation.Parameters.Select(p => $"{p.TypeFullName} {p.Name}"))})");
                        code.AppendLine("{");
                        var returnPart = delegationMethodImplementation.ReturnsVoid
                            ? string.Empty
                            : "return ";
                        code.AppendLine($"{returnPart}(({delegationMethodImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationMethodImplementation.Name}{genericsPart}({string.Join(", ", delegationMethodImplementation.Parameters.Select(p => p.Name))});");
                        code.AppendLine("}");
                        break;
                    case DelegationEventImplementation delegationEventImplementation:
                        code.AppendLine($"event {delegationEventImplementation.TypeFullName} {delegationEventImplementation.DeclaringInterfaceFullName}.{delegationEventImplementation.Name}");
                        code.AppendLine("{");
                        code.AppendLine($"add {{ (({delegationEventImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationEventImplementation.Name} += value; }}");
                        code.AppendLine($"remove {{ (({delegationEventImplementation.DeclaringInterfaceFullName}){interceptionDecoratorData.InterfaceFieldReference}).{delegationEventImplementation.Name} += value; }}");
                        code.AppendLine("}");
                        break;
                }
            }
            
            code.AppendLine("}");
        }
        code.AppendLine("}");
        code.AppendLine("#nullable disable");
        return code.ToString();
    }
}