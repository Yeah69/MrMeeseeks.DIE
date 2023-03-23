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
                .Select(x => ModelExtensions.GetDeclaredSymbol(semanticModel, x))
                .Where(x => x is not null)
                .OfType<INamedTypeSymbol>()
                .Where(x => x
                    .GetAttributes()
                    .Any(ad => CustomSymbolEqualityComparer.Default.Equals(
                        _wellKnownTypesMiscellaneous.CreateFunctionAttribute, 
                        ad.AttributeClass)))
                .Select(_containerInfoFactory)
                .ToList();
            foreach (var containerInfo in containerInfos)
            {
                using var msContainer = new MsContainer.MsContainer(_context, containerInfo);
                var executeContainer = msContainer.Create();
                executeContainer.Execute();
            }
        }
    }
}