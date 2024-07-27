using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Descriptions;

internal interface IMethodDescriptionNode
{
    string Name { get; }
    string InterfaceFullName { get; }
    bool NameProperty { get; }
    INamedTypeSymbol? ReturnTypeProperty { get; }
}

internal sealed class MethodDescriptionNode : IMethodDescriptionNode
{
    internal MethodDescriptionNode(
        // parameters
        INamedTypeSymbol interfaceType,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        Name = referenceGenerator.Generate("TypeDescription");
        InterfaceFullName = interfaceType.FullName();
        NameProperty = interfaceType.MemberNames.Contains("Name");
        ReturnTypeProperty = interfaceType
            .GetMembers("ReturnType")
            .OfType<IPropertySymbol>()
            .SingleOrDefault()
            ?.Type as INamedTypeSymbol;
    }

    public string Name { get; }
    public string InterfaceFullName { get; }
    public bool NameProperty { get; }
    public INamedTypeSymbol? ReturnTypeProperty { get; }
}