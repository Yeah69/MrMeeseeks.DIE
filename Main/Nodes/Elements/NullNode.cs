using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface INullNode : IElementNode
{
    
}

internal sealed partial class NullNode : INullNode
{
    internal NullNode(
        ITypeSymbol nullableType,
        
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = nullableType.FullName();
        Reference = referenceGenerator.Generate(nullableType);
    }
    public void Build(PassedContext passedContext) { }

    public string TypeFullName { get; }
    public string Reference { get; }
}