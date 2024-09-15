using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Interception;

internal interface ITypeDescriptionNode
{
    string Name { get; }
    string InterfaceFullName { get; }
    bool FullNameProperty { get; }
    bool NameProperty { get; }
}

internal sealed class TypeDescriptionNode : ITypeDescriptionNode
{
    internal TypeDescriptionNode(
        // parameters
        INamedTypeSymbol interfaceType,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        Name = referenceGenerator.Generate("TypeDescription");
        InterfaceFullName = interfaceType.FullName();
        FullNameProperty = interfaceType.MemberNames.Contains("FullName");
        NameProperty = interfaceType.MemberNames.Contains("Name");
    }

    public string Name { get; }
    public string InterfaceFullName { get; }
    public bool FullNameProperty { get; }
    public bool NameProperty { get; }
}