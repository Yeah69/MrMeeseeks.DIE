using Microsoft.CodeAnalysis.CSharp.Syntax;
using MrMeeseeks.DIE.CodeBuilding;
using MrMeeseeks.DIE.ResolutionBuilding;

namespace MrMeeseeks.DIE;

internal interface IExecute 
{
    void Execute();
}

internal class ExecuteImpl : IExecute
{
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly IContainerGenerator _containerGenerator;
    private readonly IContainerErrorGenerator _containerErrorGenerator;
    private readonly IContainerDieExceptionGenerator _containerDieExceptionGenerator;
    private readonly Func<IContainerInfo, IContainerResolutionBuilder> _containerResolutionBuilderFactory;
    private readonly IResolutionTreeCreationErrorHarvester _resolutionTreeCreationErrorHarvester;
    private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;
    private readonly IDiagLogger _diagLogger;

    internal ExecuteImpl(
        GeneratorExecutionContext context,
        WellKnownTypes wellKnownTypes,
        IContainerGenerator containerGenerator,
        IContainerErrorGenerator containerErrorGenerator,
        IContainerDieExceptionGenerator containerDieExceptionGenerator,
        Func<IContainerInfo, IContainerResolutionBuilder> containerResolutionBuilderFactory,
        IResolutionTreeCreationErrorHarvester resolutionTreeCreationErrorHarvester,
        Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory,
        IDiagLogger diagLogger)
    {
        _context = context;
        _wellKnownTypes = wellKnownTypes;
        _containerGenerator = containerGenerator;
        _containerErrorGenerator = containerErrorGenerator;
        _containerDieExceptionGenerator = containerDieExceptionGenerator;
        _containerResolutionBuilderFactory = containerResolutionBuilderFactory;
        _resolutionTreeCreationErrorHarvester = resolutionTreeCreationErrorHarvester;
        _containerInfoFactory = containerInfoFactory;
        _diagLogger = diagLogger;
    }

    public void Execute()
    {
        _diagLogger.Log("Start Execute");

        var errorDescriptionInsteadOfBuildFailure = _context.Compilation.Assembly.GetAttributes()
            .Any(ad => _wellKnownTypes.ErrorDescriptionInsteadOfBuildFailureAttribute.Equals(ad.AttributeClass, SymbolEqualityComparer.Default));

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
                .Where(x => x.GetAttributes().Any(ad => _wellKnownTypes.CreateFunctionAttribute.Equals(ad.AttributeClass, SymbolEqualityComparer.Default)))
                .ToList();
            foreach (var containerSymbol in containerClasses)
            {
                try
                {
                    var containerInfo = _containerInfoFactory(containerSymbol);
                    if (containerInfo.IsValid)
                    {
                        var containerResolutionBuilder = _containerResolutionBuilderFactory(containerInfo);
                        containerResolutionBuilder.AddCreateResolveFunctions(containerInfo.CreateFunctionData);

                        while (containerResolutionBuilder.HasWorkToDo)
                        {
                            containerResolutionBuilder.DoWork();
                        }

                        var containerResolution = containerResolutionBuilder.Build();
                        var errorTreeItems = _resolutionTreeCreationErrorHarvester.Harvest(containerResolution);
                        if (errorTreeItems.Any())
                            _containerErrorGenerator.Generate(containerInfo, errorTreeItems);
                        else
                            _containerGenerator.Generate(containerInfo, containerResolution);
                    }
                    else throw new NotImplementedException("Handle non-valid container information");
                }
                catch (DieException dieException)
                {
                    if (errorDescriptionInsteadOfBuildFailure)
                        _containerDieExceptionGenerator.Generate(
                            containerSymbol.ContainingNamespace.FullName(), 
                            containerSymbol.Name, 
                            dieException);
                    else
                        _diagLogger.Error(dieException);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
            
        _diagLogger.Log("End Execute");
    }
}