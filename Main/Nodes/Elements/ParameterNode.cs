using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Elements;

internal interface IParameterNode : IElementNode
{
}

internal sealed partial class ParameterNode : IParameterNode
{
    internal ParameterNode(
        ITypeSymbol type, 
        
        IReferenceGenerator referenceGenerator)
    {
        TypeFullName = type.FullName();
        Reference = referenceGenerator.Generate(type);
    }

    public void Build(PassedContext passedContext) { }

    public string TypeFullName { get; }
    public string Reference { get; }
}