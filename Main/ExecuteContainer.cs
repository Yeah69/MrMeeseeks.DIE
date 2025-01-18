using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Analytics;
using MrMeeseeks.DIE.CodeGeneration;
using MrMeeseeks.DIE.InjectionGraph;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE;

internal interface IExecuteContainer
{
    void Execute();
}

internal sealed class ExecuteContainer : IExecuteContainer
{
    private readonly bool _errorDescriptionInsteadOfBuildFailure;
    private readonly GeneratorExecutionContext _context;
    private readonly IContainerNode _containerNode;
    private readonly ICodeGenerationVisitor _codeGenerationVisitor;
    private readonly IValidateContainer _validateContainer;
    private readonly IContainerDieExceptionGenerator _containerDieExceptionGenerator;
    private readonly ICurrentExecutionPhaseSetter _currentExecutionPhaseSetter;
    private readonly ILocalDiagLogger _localDiagLogger;
    private readonly IAnalyticsFlags _analyticsFlags;
    private readonly Func<IImmutableSet<INode>?, IResolutionGraphAnalyticsNodeVisitor> _resolutionGraphAnalyticsNodeVisitorFactory;
    private readonly Lazy<IFilterForErrorRelevancyNodeVisitor> _filterForErrorRelevancyNodeVisitor;
    private readonly IInjectionGraphBuilder _injectionGraphBuilder;
    private readonly IInjectionGraphCodeGenerator _injectionGraphCodeGenerator;
    private readonly IDiagLogger _diagLogger;
    private readonly IContainerInfo _containerInfo;

    internal ExecuteContainer(
        IGeneratorConfiguration generatorConfiguration,
        GeneratorExecutionContext context,
        IContainerNode containerNode,
        ICodeGenerationVisitor codeGenerationVisitor,
        IValidateContainer validateContainer,
        IContainerDieExceptionGenerator containerDieExceptionGenerator,
        IContainerInfo containerInfo,
        ICurrentExecutionPhaseSetter currentExecutionPhaseSetter,
        ILocalDiagLogger localDiagLogger,
        IAnalyticsFlags analyticsFlags,
        Func<IImmutableSet<INode>?, IResolutionGraphAnalyticsNodeVisitor> resolutionGraphAnalyticsNodeVisitorFactory,
        Lazy<IFilterForErrorRelevancyNodeVisitor> filterForErrorRelevancyNodeVisitor,
        IInjectionGraphBuilder injectionGraphBuilder,
        IInjectionGraphCodeGenerator injectionGraphCodeGenerator,
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
        _analyticsFlags = analyticsFlags;
        _resolutionGraphAnalyticsNodeVisitorFactory = resolutionGraphAnalyticsNodeVisitorFactory;
        _filterForErrorRelevancyNodeVisitor = filterForErrorRelevancyNodeVisitor;
        _injectionGraphBuilder = injectionGraphBuilder;
        _injectionGraphCodeGenerator = injectionGraphCodeGenerator;
        _diagLogger = diagLogger;
        _containerInfo = containerInfo;
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
                ErrorExit(null, null);
                return;
            }
            
            _currentExecutionPhaseSetter.Value = ExecutionPhase.Resolution;
            //_containerNode.Build(new(ImmutableStack<INamedTypeSymbol>.Empty, null));

            if (_diagLogger.ErrorsIssued)
            {
                ErrorExit(null, _containerNode);
                return;
            }

            _currentExecutionPhaseSetter.Value = ExecutionPhase.CodeGeneration;
            //_codeGenerationVisitor.VisitIContainerNode(_containerNode);

            if (_diagLogger.ErrorsIssued)
            {
                ErrorExit(null, _containerNode);
                return;
            }

            /*var containerSource = CSharpSyntaxTree
                .ParseText(SourceText.From(_codeGenerationVisitor.GenerateContainerFile(), Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();

            //_context.AddSource($"{_containerInfo.Namespace}.{_containerInfo.Name}.g.cs", containerSource);//*/
            
            foreach (var (rootType, name, overrideTypes, attributesLocation) in _containerInfo.CreateFunctionData)
            {
                _injectionGraphBuilder.BuildForRootType(rootType, name, overrideTypes, attributesLocation);
            }
            
            _injectionGraphBuilder.AssignFunctions();
            
            var injectionGraphSource = CSharpSyntaxTree
                .ParseText(SourceText.From(_injectionGraphCodeGenerator.Generate(), Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
            
            _context.AddSource($"{_containerInfo.Namespace}.{_containerInfo.Name}.g.cs", injectionGraphSource);
                
            _currentExecutionPhaseSetter.Value = ExecutionPhase.Analytics;
            if (_analyticsFlags.ResolutionGraph)
                _resolutionGraphAnalyticsNodeVisitorFactory(null).VisitIContainerNode(_containerNode);
        }
        catch (DieException dieException)
        {
            ErrorExit(dieException, _containerNode);
        }
#pragma warning disable CA1031 // In this case we want to catch all exceptions to have a chance to gracefully exit the code generation process
        catch (Exception exception)
#pragma warning restore CA1031
        {
            _localDiagLogger.Error(
                ErrorLogData.UnexpectedException(exception), 
                _containerInfo.ContainerType.Locations.FirstOrDefault() ?? Location.None);
            ErrorExit(exception, _containerNode);
        }

        return;

        void ErrorExit(
            Exception? exception,
            IContainerNode? containerNode)
        {
            if (_errorDescriptionInsteadOfBuildFailure)
                _containerDieExceptionGenerator.Generate(exception);

            if (_analyticsFlags.ErrorFilteredResolutionGraph && containerNode is not null)
            {
                _filterForErrorRelevancyNodeVisitor.Value.VisitIContainerNode(containerNode);
                _resolutionGraphAnalyticsNodeVisitorFactory(_filterForErrorRelevancyNodeVisitor.Value.ErrorRelevantNodes)
                    .VisitIContainerNode(containerNode);
            }
        }
    }
}

internal interface IExecuteContainerContext :  IDisposable
{
    void Execute();
}

internal sealed class ExecuteContainerContext : IExecuteContainerContext, ITransientScopeRoot
{
    private readonly IExecuteContainer _executeContainer;
    private readonly IDisposable _eagerDisposalTrigger;
    private int _disposed; // 0 = false, > 0 = true

    public ExecuteContainerContext(
        IExecuteContainer executeContainer,
        IDisposable eagerDisposalTrigger)
    {
        _executeContainer = executeContainer;
        _eagerDisposalTrigger = eagerDisposalTrigger;
    }

    public void Execute() => _executeContainer.Execute();

    public void Dispose()
    {
        if (Interlocked.Increment(ref _disposed) != 1) 
            return;
        
        _eagerDisposalTrigger.Dispose();
    }
}
