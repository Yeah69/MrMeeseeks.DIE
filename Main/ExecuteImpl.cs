﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.ResolutionBuilding;
using MrMeeseeks.DIE.ResolutionBuilding.Function;
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
                ResolutionTreeItem.ResolutionTreeItemCount = 0;
                Resolvable.ResolvableCount = 0;
                Resolvable.TypeToCountMap.Clear();
                FunctionResolutionBuilder.FunctionLog.Clear();
                try
                {
                    var containerInfo = _containerInfoFactory(containerSymbol);
                    var validationDiagnostics = _validateContainer.Validate(containerInfo.ContainerType, containerInfo.ContainerType)
                        .ToImmutableArray();
                    if (!validationDiagnostics.Any())
                    {
                        var containerResolutionBuilder = _containerResolutionBuilderFactory(containerInfo);
                        containerResolutionBuilder.AddCreateResolveFunctions(containerInfo.CreateFunctionData);

                        while (containerResolutionBuilder.HasWorkToDo)
                        {
                            containerResolutionBuilder.DoWork();
                        }
                        containerResolutionBuilder.FunctionCycleTracker.DetectCycle();
                        
                        _context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_00_00", 
                                "Debug",
                                $"Resolution tree item count: {ResolutionTreeItem.ResolutionTreeItemCount} ({containerSymbol.FullName()})", 
                                "Warning", DiagnosticSeverity.Warning, 
                                true),
                            Location.None));
                        
                        _context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_00_00", 
                                "Debug",
                                $"Resolution tree (Resolvable) item count: {Resolvable.ResolvableCount} ({containerSymbol.FullName()})", 
                                "Warning", DiagnosticSeverity.Warning, 
                                true),
                            Location.None));

                        foreach (var keyValuePair in Resolvable.TypeToCountMap.OrderByDescending(kvp => kvp.Value))
                        {
                            _context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_00_00", 
                                    "Debug",
                                    $"Count/Type: {keyValuePair.Value}/{keyValuePair.Key}", 
                                    "Warning", DiagnosticSeverity.Warning, 
                                    true),
                                Location.None));
                        }

                        foreach (var s in FunctionResolutionBuilder.FunctionLog.OrderBy(s => s))
                        {
                            _context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_00_00", 
                                    "Debug",
                                    $"Function: {s}", 
                                    "Warning", DiagnosticSeverity.Warning, 
                                    true),
                                Location.None));
                        }
                        
                        var containerResolution = containerResolutionBuilder.Build();
                        
                        _context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor($"{Constants.DieAbbreviation}_00_01", 
                                "Debug",
                                $"Starting code building", 
                                "Warning", DiagnosticSeverity.Warning, 
                                true),
                            Location.None));
                        
                        _containerGenerator.Generate(containerInfo, containerResolution);
                    }
                    else
                    {

                        throw new ValidationDieException(validationDiagnostics);
                    }
                }
                catch (ValidationDieException validationDieException)
                {
                    if (_errorDescriptionInsteadOfBuildFailure)
                        _containerDieExceptionGenerator.Generate(
                            containerSymbol.ContainingNamespace.FullName(), 
                            containerSymbol.Name, 
                            validationDieException);
                    else
                        foreach (var validationDiagnostic in validationDieException.Diagnostics)
                            _diagLogger.Log(validationDiagnostic);
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
                    {
                        // ignore
                    }
                    else
                        _diagLogger.Error(Diagnostics.UnexpectedException(exception));
                }
            }
        }
    }
}