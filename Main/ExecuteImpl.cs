using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.ResolutionBuilding;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Validation.Range;
using MrMeeseeks.DIE.Visitors;

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
    private readonly IContainerGenerator _containerGenerator;
    private readonly IContainerDieExceptionGenerator _containerDieExceptionGenerator;
    private readonly IValidateContainer _validateContainer;
    private readonly Func<IContainerInfo, IContainerResolutionBuilder> _containerResolutionBuilderFactory;
    private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;
    private readonly Func<IContainerInfo, IContainerNode> _containerNodeFactory;
    private readonly Func<ICodeGenerationVisitor> _codeGeneratorVisitorFactory;
    private readonly IReferenceGeneratorFactory _referenceGeneratorFactory;
    private readonly IDiagLogger _diagLogger;

    internal ExecuteImpl(
        bool errorDescriptionInsteadOfBuildFailure,
        GeneratorExecutionContext context,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        IContainerGenerator containerGenerator,
        IContainerDieExceptionGenerator containerDieExceptionGenerator,
        IValidateContainer validateContainer,
        Func<IContainerInfo, IContainerResolutionBuilder> containerResolutionBuilderFactory,
        Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory,
        Func<IContainerInfo, IContainerNode> containerNodeFactory,
        Func<ICodeGenerationVisitor> codeGeneratorVisitorFactory,
        IReferenceGeneratorFactory referenceGeneratorFactory,
        IDiagLogger diagLogger)
    {
        _errorDescriptionInsteadOfBuildFailure = errorDescriptionInsteadOfBuildFailure;
        _context = context;
        _wellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
        _containerGenerator = containerGenerator;
        _containerDieExceptionGenerator = containerDieExceptionGenerator;
        _validateContainer = validateContainer;
        _containerResolutionBuilderFactory = containerResolutionBuilderFactory;
        _containerInfoFactory = containerInfoFactory;
        _containerNodeFactory = containerNodeFactory;
        _codeGeneratorVisitorFactory = codeGeneratorVisitorFactory;
        _referenceGeneratorFactory = referenceGeneratorFactory;
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
                .Where(x => x.GetAttributes().Any(ad => SymbolEqualityComparer.Default.Equals(_wellKnownTypesMiscellaneous.CreateFunctionAttribute, ad.AttributeClass)))
                .ToList();
            foreach (var containerSymbol in containerClasses)
            {
                var currentPhase = ExecutionPhase.Validation;
                try
                {
                    var containerInfo = _containerInfoFactory(containerSymbol);
                    var validationDiagnostics = _validateContainer.Validate(containerInfo.ContainerType, containerInfo.ContainerType)
                        .ToImmutableArray();
                    if (!validationDiagnostics.Any())
                    {
                        currentPhase = ExecutionPhase.Resolution;
                        var containerResolutionBuilder = _containerResolutionBuilderFactory(containerInfo);
                        containerResolutionBuilder.AddCreateResolveFunctions(containerInfo.CreateFunctionData);

                        /* todo replace while (containerResolutionBuilder.HasWorkToDo)
                        {
                            containerResolutionBuilder.DoWork();
                        }*/
                        currentPhase = ExecutionPhase.CycleDetection;
                        // todo replace containerResolutionBuilder.FunctionCycleTracker.DetectCycle();
                        
                        currentPhase = ExecutionPhase.ResolutionBuilding;
                        // todo replace var containerResolution = containerResolutionBuilder.Build();
                        
                        currentPhase = ExecutionPhase.CodeGeneration;
                        // todo replace_containerGenerator.Generate(containerInfo, containerResolution);
                        
                        // todo complete visitor way
                        var containerNode = _containerNodeFactory(containerInfo);
                        var buildStack = new Stack<INode>();
                        buildStack.Push(containerNode);
                        while (buildStack.Any() && buildStack.Pop() is {} node)
                        {
                            node.Build();
                        }
                        var visitor = _codeGeneratorVisitorFactory();
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
                        _containerDieExceptionGenerator.Generate(
                            containerSymbol.ContainingNamespace.FullName(), 
                            containerSymbol.Name, 
                            dieException);
                    else
                        _diagLogger.Error(dieException, currentPhase);
                }
                catch (Exception exception)
                {
                    if (_errorDescriptionInsteadOfBuildFailure)
                        _containerDieExceptionGenerator.Generate(
                            containerSymbol.ContainingNamespace.FullName(), 
                            containerSymbol.Name, 
                            exception);
                    else
                        _diagLogger.Log(Diagnostics.UnexpectedException(exception, currentPhase));
                }
            }
        }
    }
}