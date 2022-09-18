using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.ResolutionBuilding;
using MrMeeseeks.DIE.Validation.Range;

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
                .Select(x => semanticModel.GetDeclaredSymbol(x))
                .Where(x => x is not null)
                .OfType<INamedTypeSymbol>()
                .Where(x => x.GetAttributes().Any(ad => _wellKnownTypesMiscellaneous.CreateFunctionAttribute.Equals(ad.AttributeClass, SymbolEqualityComparer.Default)))
                .ToList();
            foreach (var containerSymbol in containerClasses)
            {
                try
                {
                    DateTime start = DateTime.Now;
                    _context.ReportDiagnostic(Diagnostics.Logging($"Start {start}"));
                    var containerInfo = _containerInfoFactory(containerSymbol);
                    var validationDiagnostics = _validateContainer.Validate(containerInfo.ContainerType, containerInfo.ContainerType)
                        .ToImmutableArray();
                    if (!validationDiagnostics.Any())
                    {
                        _context.ReportDiagnostic(Diagnostics.Logging($"Validation {DateTime.Now - start}"));
                        start = DateTime.Now;
                        var containerResolutionBuilder = _containerResolutionBuilderFactory(containerInfo);
                        _context.ReportDiagnostic(Diagnostics.Logging($"Building Container Builder {DateTime.Now - start}"));
                        start = DateTime.Now;
                        containerResolutionBuilder.AddCreateResolveFunctions(containerInfo.CreateFunctionData);
                        _context.ReportDiagnostic(Diagnostics.Logging($"Create Resolve Functions {DateTime.Now - start}"));
                        start = DateTime.Now;

                        while (containerResolutionBuilder.HasWorkToDo)
                        {
                            containerResolutionBuilder.DoWork();
                        }
                        _context.ReportDiagnostic(Diagnostics.Logging($"Do work {DateTime.Now - start}"));
                        start = DateTime.Now;
                        containerResolutionBuilder.FunctionCycleTracker.DetectCycle();
                        _context.ReportDiagnostic(Diagnostics.Logging($"Detect cycles {DateTime.Now - start}"));
                        start = DateTime.Now;
                        
                        var containerResolution = containerResolutionBuilder.Build();
                        
                        _containerGenerator.Generate(containerInfo, containerResolution);
                        _context.ReportDiagnostic(Diagnostics.Logging($"Generate Code {DateTime.Now - start}"));
                        start = DateTime.Now;
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
                        _diagLogger.Error(dieException);
                }
                catch (Exception exception)
                {
                    if (_errorDescriptionInsteadOfBuildFailure)
                        _containerDieExceptionGenerator.Generate(
                            containerSymbol.ContainingNamespace.FullName(), 
                            containerSymbol.Name, 
                            exception);
                    else
                        _diagLogger.Log(Diagnostics.UnexpectedException(exception));
                }
            }
        }
    }
}