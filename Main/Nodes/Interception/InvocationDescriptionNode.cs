using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Interception;

internal interface IInvocationDescriptionNode
{
    string Name { get; }
    string InterfaceFullName { get; }
    string ImplementationFullName { get; }
    bool ArgumentsProperty { get; }
    bool ArgumentsItemTypeNullable { get; }
    bool GenericArgumentsProperty { get; }
    bool InvocationTargetProperty { get; }
    bool MethodProperty { get; }
    bool MethodInvocationTargetProperty { get; }
    bool ProxyProperty { get; }
    bool ReturnValueProperty { get; }
    bool ReturnValueTypeNullable { get; }
    bool TargetTypeProperty { get; }
    bool ProceedMethod { get; }
}

internal sealed class InvocationDescriptionNode : IInvocationDescriptionNode
{
    public string ImplementationFullName { get; }

    internal InvocationDescriptionNode(
        // parameters
        INamedTypeSymbol interfaceType,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        Name = referenceGenerator.Generate("InvocationDescription");
        InterfaceFullName = interfaceType.FullName();
        ImplementationFullName = $"global::{Constants.DescriptionsNamespace}.{Name}";
        ArgumentsProperty = HasProperty("Arguments");
        ArgumentsItemTypeNullable = interfaceType
            .GetMembers("Arguments")
            .OfType<IPropertySymbol>()
            .SingleOrDefault()?
            .Type is IArrayTypeSymbol { ElementType.NullableAnnotation: NullableAnnotation.Annotated };
        GenericArgumentsProperty = HasProperty("GenericArguments");
        InvocationTargetProperty = HasProperty("InvocationTarget");
        MethodProperty = HasProperty("Method");
        MethodInvocationTargetProperty = HasProperty("MethodInvocationTarget");
        ProxyProperty = HasProperty("Proxy");
        ReturnValueProperty = HasProperty("ReturnValue");
        ReturnValueTypeNullable = interfaceType
            .GetMembers("ReturnValue")
            .OfType<IPropertySymbol>()
            .SingleOrDefault()?
            .Type
            .NullableAnnotation is NullableAnnotation.Annotated;
        TargetTypeProperty = HasProperty("TargetType");
        
        ProceedMethod = interfaceType
            .GetMembers("Proceed")
            .OfType<IMethodSymbol>()
            .SingleOrDefault(method => method.Parameters.IsEmpty) is not null;
        return;

        bool HasProperty(string propertyName) => interfaceType
            .GetMembers(propertyName)
            .OfType<IPropertySymbol>()
            .SingleOrDefault() is not null;
    }

    public string Name { get; }
    public string InterfaceFullName { get; }
    public bool ArgumentsProperty { get; }
    public bool ArgumentsItemTypeNullable { get; }
    public bool GenericArgumentsProperty { get; }
    public bool InvocationTargetProperty { get; }
    public bool MethodProperty { get; }
    public bool MethodInvocationTargetProperty { get; }
    public bool ProxyProperty { get; }
    public bool ReturnValueProperty { get; }
    public bool ReturnValueTypeNullable { get; }
    public bool TargetTypeProperty { get; }
    public bool ProceedMethod { get; }
}