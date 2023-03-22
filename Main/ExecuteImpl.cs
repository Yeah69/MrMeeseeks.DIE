using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE;

internal interface IExecute 
{
    void Execute();
}

internal class ExecuteImpl : IExecute
{
    private readonly bool _errorDescriptionInsteadOfBuildFailure;
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private readonly IContainerDieExceptionGenerator _containerDieExceptionGenerator;
    private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;
    private readonly IDiagLogger _diagLogger;

    internal ExecuteImpl(
        bool errorDescriptionInsteadOfBuildFailure,
        GeneratorExecutionContext context,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        IContainerDieExceptionGenerator containerDieExceptionGenerator,
        Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory,
        IDiagLogger diagLogger)
    {
        _errorDescriptionInsteadOfBuildFailure = errorDescriptionInsteadOfBuildFailure;
        _context = context;
        _wellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
        _containerDieExceptionGenerator = containerDieExceptionGenerator;
        _containerInfoFactory = containerInfoFactory;
        _diagLogger = diagLogger;
    }

    public void Execute()
    {
        foreach (var syntaxTree in _context.Compilation.SyntaxTrees)
        {
            var semanticModel = _context.Compilation.GetSemanticModel(syntaxTree);
            var containerClasses = syntaxTree
                .GetRoot()
                .DescendantNodesAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .Select(x => ModelExtensions.GetDeclaredSymbol(semanticModel, x))
                .Where(x => x is not null)
                .OfType<INamedTypeSymbol>()
                .Where(x => x.GetAttributes().Any(ad => CustomSymbolEqualityComparer.Default.Equals(_wellKnownTypesMiscellaneous.CreateFunctionAttribute, ad.AttributeClass)))
                .ToList();
            foreach (var containerSymbol in containerClasses)
            {
                var currentPhase = ExecutionPhase.Validation;
                try
                {
                    var containerInfo = _containerInfoFactory(containerSymbol);
                    using var msContainer = new MsContainer.MsContainer(_context, containerInfo);
                    var containerNodeRoot = msContainer.Create();
                    var validationDiagnostics = containerNodeRoot
                        .ValidateContainer
                        .Validate(containerInfo.ContainerType, containerInfo.ContainerType)
                        .ToImmutableArray();
                    if (!validationDiagnostics.Any())
                    {
                        // todo fix phases
                        currentPhase = ExecutionPhase.Resolution;
                        currentPhase = ExecutionPhase.CycleDetection;
                        currentPhase = ExecutionPhase.ResolutionBuilding;
                        currentPhase = ExecutionPhase.CodeGeneration;

                        var containerNode = containerNodeRoot.Container;
                        containerNode.Build(ImmutableStack.Create<INamedTypeSymbol>());

                        if (_diagLogger.ErrorsIssued)
                            continue;

                        var visitor = containerNodeRoot.CodeGenerationVisitor;
                        visitor.VisitContainerNode(containerNode);

                        var containerSource = CSharpSyntaxTree
                            .ParseText(SourceText.From(visitor.GenerateContainerFile(), Encoding.UTF8))
                            .GetRoot()
                            .NormalizeWhitespace()
                            .SyntaxTree
                            .GetText();

                        _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);
                    }
                    else
                    {
                        throw new ValidationDieException(validationDiagnostics);
                    }
                }
                catch (DieException dieException)
                {
                    if (_errorDescriptionInsteadOfBuildFailure)
                        _containerDieExceptionGenerator.Generate(containerSymbol, dieException);
                    else
                        _diagLogger.Error(dieException, currentPhase);
                }
                catch (Exception exception)
                {
                    if (_errorDescriptionInsteadOfBuildFailure)
                        _containerDieExceptionGenerator.Generate(containerSymbol, exception);
                    else
                        _diagLogger.Log(Diagnostics.UnexpectedException(exception, currentPhase));
                }
                finally
                {
                    _diagLogger.Reset();
                }
            }
        }
    }
}