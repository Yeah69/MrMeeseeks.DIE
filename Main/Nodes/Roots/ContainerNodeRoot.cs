using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Roots;

internal interface IContainerNodeRoot
{
    IContainerNode Container { get; }
    ICodeGenerationVisitor CodeGenerationVisitor { get; }
    IValidateContainer ValidateContainer { get; }
    IDiagLogger DiagLogger { get; }
    IContainerDieExceptionGenerator ContainerDieExceptionGenerator { get; }
}

internal class ContainerNodeRoot : IContainerNodeRoot
{
    public ContainerNodeRoot(
        IContainerNode container,
        ICodeGenerationVisitor codeGenerationVisitor,
        IValidateContainer validateContainer,
        IDiagLogger diagLogger,
        IContainerDieExceptionGenerator containerDieExceptionGenerator)
    {
        Container = container;
        CodeGenerationVisitor = codeGenerationVisitor;
        ValidateContainer = validateContainer;
        DiagLogger = diagLogger;
        ContainerDieExceptionGenerator = containerDieExceptionGenerator;
    }

    public IContainerNode Container { get; }
    public ICodeGenerationVisitor CodeGenerationVisitor { get; }
    public IValidateContainer ValidateContainer { get; }
    public IDiagLogger DiagLogger { get; }
    public IContainerDieExceptionGenerator ContainerDieExceptionGenerator { get; }
}