using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.RangeRoots;

internal interface IContainerNodeRoot
{
    //IContainerNode Container { get; }
    ICodeGenerationVisitor CodeGenerationVisitor { get; }
}

internal class ContainerNodeRoot : IContainerNodeRoot
{
    public ContainerNodeRoot(
        //IContainerNode container,
        ICodeGenerationVisitor codeGenerationVisitor
        )
    {
        //Container = container;
        CodeGenerationVisitor = codeGenerationVisitor;
    }

    //public IContainerNode Container { get; }
    public ICodeGenerationVisitor CodeGenerationVisitor { get; }
}