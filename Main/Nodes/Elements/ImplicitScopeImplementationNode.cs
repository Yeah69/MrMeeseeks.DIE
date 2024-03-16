namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IImplicitScopeImplementationNode : IElementNode
{
    IReadOnlyList<(string Name, IElementNode Element)> Properties { get; }
}

internal partial class ImplicitScopeImplementationNode : IImplicitScopeImplementationNode
{
    internal ImplicitScopeImplementationNode(
        string typeFullName,
        (string Name, IElementNode Element)[] properties,
        
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = typeFullName;
        Reference = referenceGenerator.Generate("scope");
        Properties = properties;
    }

    public void Build(PassedContext passedContext)
    {
    }

    public string TypeFullName { get; }
    public string Reference { get; }
    public IReadOnlyList<(string Name, IElementNode Element)> Properties { get; }
}