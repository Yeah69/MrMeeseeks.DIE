using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Descriptions;

internal interface ITypeDescriptionNode
{
    string Name { get; }
    string InterfaceFullName { get; }
    bool FullNameProperty { get; }
    string? FullNameConstrParam { get; }
    bool NameProperty { get; }
    string? NameConstrParam { get; }
}

internal class TypeDescriptionNode : ITypeDescriptionNode
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
        FullNameConstrParam = FullNameProperty
            ? referenceGenerator.Generate("fullName")
            : null;
        NameProperty = interfaceType.MemberNames.Contains("Name");
        NameConstrParam = NameProperty
            ? referenceGenerator.Generate("name")
            : null;
    }

    public string Name { get; }
    public string InterfaceFullName { get; }
    public bool FullNameProperty { get; }
    public string? FullNameConstrParam { get; }
    public bool NameProperty { get; }
    public string? NameConstrParam { get; }
}