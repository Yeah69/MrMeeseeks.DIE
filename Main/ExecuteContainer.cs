using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Logging;
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
    private readonly ICurrentExecutionPhaseSetter _currentExecutionPhaseSetter;
    private readonly ILocalDiagLogger _localDiagLogger;
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
        ICurrentExecutionPhaseSetter currentExecutionPhaseSetter,
        ILocalDiagLogger localDiagLogger,
        IDiagLogger diagLogger)
    {
        _errorDescriptionInsteadOfBuildFailure = generatorConfiguration.ErrorDescriptionInsteadOfBuildFailure;
        _context = context;
        _containerNode = containerNode;
        _codeGenerationVisitor = codeGenerationVisitor;
        _validateContainer = validateContainer;
        _containerDieExceptionGenerator = containerDieExceptionGenerator;
        _currentExecutionPhaseSetter = currentExecutionPhaseSetter;
        _localDiagLogger = localDiagLogger;
        _diagLogger = diagLogger;
        _containerInfo = containerInfoContext.ContainerInfo;
    }

    public void Execute()
    {
        try
        {
            _currentExecutionPhaseSetter.Value = ExecutionPhase.ContainerValidation;
            _validateContainer
                .Validate(_containerInfo.ContainerType, _containerInfo.ContainerType);

            if (_diagLogger.ErrorsIssued)
            {
                ErrorExit(null);
                return;
            }
            
            _currentExecutionPhaseSetter.Value = ExecutionPhase.Resolution;
            _containerNode.Build(ImmutableStack.Create<INamedTypeSymbol>());

            if (_diagLogger.ErrorsIssued)
            {
                ErrorExit(null);
                return;
            }

            _currentExecutionPhaseSetter.Value = ExecutionPhase.CodeGeneration;
            _codeGenerationVisitor.VisitIContainerNode(_containerNode);

            if (_diagLogger.ErrorsIssued)
            {
                ErrorExit(null);
                return;
            }

            var containerSource = CSharpSyntaxTree
                .ParseText(SourceText.From(_codeGenerationVisitor.GenerateContainerFile(), Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();

            _context.AddSource($"{_containerInfo.Namespace}.{_containerInfo.Name}.g.cs", containerSource);
        }
        catch (DieException dieException)
        {
            ErrorExit(dieException);
        }
        catch (Exception exception)
        {
            _localDiagLogger.Error(
                ErrorLogData.UnexpectedException(exception), 
                _containerInfo.ContainerType.Locations.FirstOrDefault() ?? Location.None);
            ErrorExit(exception);
        }

        void ErrorExit(Exception? exception)
        {
            if (_errorDescriptionInsteadOfBuildFailure)
                _containerDieExceptionGenerator.Generate(exception);
        }
    }
}