using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    private readonly Func<IContainerInfo, IContainerResolutionBuilder> _containerResolutionBuilderFactory;
    private readonly IResolutionTreeCreationErrorHarvester _resolutionTreeCreationErrorHarvester;
    private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;
    private readonly IDiagLogger _diagLogger;

    public ExecuteImpl(
        GeneratorExecutionContext context,
        WellKnownTypes wellKnownTypes,
        IContainerGenerator containerGenerator,
        IContainerErrorGenerator containerErrorGenerator,
        Func<IContainerInfo, IContainerResolutionBuilder> containerResolutionBuilderFactory,
        IResolutionTreeCreationErrorHarvester resolutionTreeCreationErrorHarvester,
        Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory,
        IDiagLogger diagLogger)
    {
        _context = context;
        _wellKnownTypes = wellKnownTypes;
        _containerGenerator = containerGenerator;
        _containerErrorGenerator = containerErrorGenerator;
        _containerResolutionBuilderFactory = containerResolutionBuilderFactory;
        _resolutionTreeCreationErrorHarvester = resolutionTreeCreationErrorHarvester;
        _containerInfoFactory = containerInfoFactory;
        _diagLogger = diagLogger;
    }

    public void Execute()
    {
        _diagLogger.Log("Start Execute");
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
                .Where(x => x.AllInterfaces.Any(x => x.OriginalDefinition.Equals(_wellKnownTypes.Container, SymbolEqualityComparer.Default)));
            foreach (var namedTypeSymbol in containerClasses)
            {
                var containerInfo = _containerInfoFactory(namedTypeSymbol);
                if (containerInfo.IsValid)
                {
                    var containerResolutionBuilder = _containerResolutionBuilderFactory(containerInfo);
                    containerResolutionBuilder.AddCreateResolveFunctions(containerInfo.ResolutionRootTypes);
                    var containerResolution = containerResolutionBuilder.Build();
                    var errorTreeItems = _resolutionTreeCreationErrorHarvester.Harvest(containerResolution);
                    if (errorTreeItems.Any())
                        _containerErrorGenerator.Generate(containerInfo, errorTreeItems);
                    else
                        _containerGenerator.Generate(containerInfo, containerResolution);
                }
                else throw new NotImplementedException("Handle non-valid container information");
            }
        }
            
        _diagLogger.Log("End Execute");
    }
}