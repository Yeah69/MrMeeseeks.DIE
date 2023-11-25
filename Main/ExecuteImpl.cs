using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE;

internal interface IExecute 
{
    void Execute();
}

internal class ExecuteImpl : IExecute
{
    private readonly GeneratorExecutionContext _context;
    private readonly IRangeUtility _rangeUtility;
    private readonly IDescriptionsGenerator _descriptionsGenerator;
    private readonly Func<INamedTypeSymbol, IContainerInfo> _containerInfoFactory;
    private readonly Func<IContainerInfo, ContainerLevelContainer> _containerLevelContainerFactory;

    internal ExecuteImpl(
        GeneratorExecutionContext context,
        IRangeUtility rangeUtility,
        IDescriptionsGenerator descriptionsGenerator,
        Func<INamedTypeSymbol, IContainerInfo> containerInfoFactory,
        Func<IContainerInfo, ContainerLevelContainer> containerLevelContainerFactory)
    {
        _context = context;
        _rangeUtility = rangeUtility;
        _descriptionsGenerator = descriptionsGenerator;
        _containerInfoFactory = containerInfoFactory;
        _containerLevelContainerFactory = containerLevelContainerFactory;
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
                .Where(x => _rangeUtility.IsAContainer(x))
                .Select(_containerInfoFactory)
                .ToList();
            foreach (var containerInfo in containerInfos)
            {
                using var msContainer = _containerLevelContainerFactory(containerInfo);
                var executeContainer = msContainer.Create();
                executeContainer.Execute();
            }
        }
        
        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(_descriptionsGenerator.Generate(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();

        _context.AddSource($"{Constants.DescriptionsNamespace}.g.cs", containerSource);
    }
}