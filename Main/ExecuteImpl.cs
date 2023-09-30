using Microsoft.CodeAnalysis.CSharp.Syntax;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IExecute 
{
    void Execute();
}

internal class ExecuteImpl : IExecute
{
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;

    internal ExecuteImpl(
        GeneratorExecutionContext context,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory)
    {
        _context = context;
        _wellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
        _containerInfoFactory = containerInfoFactory;
    }

    public void Execute()
    {
        foreach (var syntaxTree in _context.Compilation.SyntaxTrees)
        {
            var semanticModel = _context.Compilation.GetSemanticModel(syntaxTree);
            var containerInfos = syntaxTree
                .GetRoot()
                .DescendantNodesAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .Select(x => semanticModel.GetDeclaredSymbol(x))
                .Where(x => x is not null)
                .OfType<INamedTypeSymbol>()
                .Where(x => x is { IsAbstract: false } && x
                    .AllBaseTypesAndSelf()
                    .SelectMany(t => t.GetAttributes())
                    .Any(ad => CustomSymbolEqualityComparer.Default.Equals(
                        _wellKnownTypesMiscellaneous.CreateFunctionAttribute, 
                        ad.AttributeClass)))
                .Select(_containerInfoFactory)
                .ToList();
            foreach (var containerInfo in containerInfos)
            {
                using var msContainer = MsContainer.MsContainer.DIE_CreateContainer(_context, containerInfo);
                var executeContainer = msContainer.Create();
                executeContainer.Execute();
            }
        }
    }
}