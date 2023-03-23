using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE;

internal interface IExecuteContainer
{
    void Execute();
}

internal class ExecuteContainer : IExecuteContainer
{
    private readonly bool _errorDescriptionInsteadOfBuildFailure;
    private readonly GeneratorExecutionContext _context;
    private readonly IContainerNode _containerNode;
    private readonly ICodeGenerationVisitor _codeGenerationVisitor;
    private readonly IValidateContainer _validateContainer;
    private readonly IContainerDieExceptionGenerator _containerDieExceptionGenerator;
    private readonly IDiagLogger _diagLogger;
    private readonly IContainerInfo _containerInfo;

    internal ExecuteContainer(
        IGeneratorConfiguration generatorConfiguration,
        GeneratorExecutionContext context,
        IContainerNode containerNode,
        ICodeGenerationVisitor codeGenerationVisitor,
        IValidateContainer validateContainer,
        IContainerDieExceptionGenerator containerDieExceptionGenerator,
        IContainerInfoContext containerInfoContext,
        IDiagLogger diagLogger)
    {
        _errorDescriptionInsteadOfBuildFailure = generatorConfiguration.ErrorDescriptionInsteadOfBuildFailure;
        _context = context;
        _containerNode = containerNode;
        _codeGenerationVisitor = codeGenerationVisitor;
        _validateContainer = validateContainer;
        _containerDieExceptionGenerator = containerDieExceptionGenerator;
        _diagLogger = diagLogger;
        _containerInfo = containerInfoContext.ContainerInfo;
    }

    public void Execute()
    {
        var currentPhase = ExecutionPhase.Validation;
        try
        {
            var validationDiagnostics = _validateContainer
                .Validate(_containerInfo.ContainerType, _containerInfo.ContainerType)
                .ToImmutableArray();
            if (!validationDiagnostics.Any())
            {
                // todo fix phases
                currentPhase = ExecutionPhase.Resolution;
                currentPhase = ExecutionPhase.CycleDetection;
                currentPhase = ExecutionPhase.ResolutionBuilding;
                currentPhase = ExecutionPhase.CodeGeneration;

                _containerNode.Build(ImmutableStack.Create<INamedTypeSymbol>());

                if (_diagLogger.ErrorsIssued)
                    return;

                _codeGenerationVisitor.VisitContainerNode(_containerNode);

                var containerSource = CSharpSyntaxTree
                    .ParseText(SourceText.From(_codeGenerationVisitor.GenerateContainerFile(), Encoding.UTF8))
                    .GetRoot()
                    .NormalizeWhitespace()
                    .SyntaxTree
                    .GetText();

                _context.AddSource($"{_containerInfo.Namespace}.{_containerInfo.Name}.g.cs", containerSource);
            }
            else
                throw new ValidationDieException(validationDiagnostics);
        }
        catch (DieException dieException)
        {
            if (_errorDescriptionInsteadOfBuildFailure)
                _containerDieExceptionGenerator.Generate(dieException);
            else
                _diagLogger.Error(dieException, currentPhase);
        }
        catch (Exception exception)
        {
            if (_errorDescriptionInsteadOfBuildFailure)
                _containerDieExceptionGenerator.Generate(exception);
            else
                _diagLogger.Log(Diagnostics.UnexpectedException(exception, currentPhase));
        }
        finally
        {
            _diagLogger.Reset();
        }
    }
}