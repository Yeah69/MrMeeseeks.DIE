using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IContainerNodeRoot
{
    IContainerNode Container { get; }
    ICodeGenerationVisitor CodeGenerationVisitor { get; }
    IValidateContainer ValidateContainer { get; }
}

internal class ContainerNodeRoot : IContainerNodeRoot
{
    public ContainerNodeRoot(
        IContainerNode container,
        ICodeGenerationVisitor codeGenerationVisitor,
        IValidateContainer validateContainer)
    {
        Container = container;
        CodeGenerationVisitor = codeGenerationVisitor;
        ValidateContainer = validateContainer;
    }

    public IContainerNode Container { get; }
    public ICodeGenerationVisitor CodeGenerationVisitor { get; }
    public IValidateContainer ValidateContainer { get; }
}