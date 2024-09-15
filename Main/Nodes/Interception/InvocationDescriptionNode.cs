using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Interception;

internal interface IInvocationDescriptionNode
{
    string Name { get; }
    string InterfaceFullName { get; }
    INamedTypeSymbol? TargetTypeProperty { get; }
    INamedTypeSymbol? TargetMethodProperty { get; }
}

internal sealed class InvocationDescriptionNode : IInvocationDescriptionNode
{
    internal InvocationDescriptionNode(
        // parameters
        INamedTypeSymbol interfaceType,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        Name = referenceGenerator.Generate("InvocationDescription");
        InterfaceFullName = interfaceType.FullName();
        TargetTypeProperty = interfaceType
            .GetMembers("TargetType")
            .OfType<IPropertySymbol>()
            .SingleOrDefault()
            ?.Type as INamedTypeSymbol;
        TargetMethodProperty = interfaceType
            .GetMembers("TargetMethod")
            .OfType<IPropertySymbol>()
            .SingleOrDefault()
            ?.Type as INamedTypeSymbol;
    }

    public string Name { get; }
    public string InterfaceFullName { get; }
    public INamedTypeSymbol? TargetTypeProperty { get; }
    public INamedTypeSymbol? TargetMethodProperty { get; }
}