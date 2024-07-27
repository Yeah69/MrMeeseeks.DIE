using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.CodeGeneration;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE;

internal interface IExecute 
{
    void Execute();
}

internal sealed class ExecuteImpl : IExecute
{
    private readonly GeneratorExecutionContext _context;
    private readonly IRangeUtility _rangeUtility;
    private readonly IRequiredKeywordUtility _requiredKeywordUtility;
    private readonly IDisposeUtility _disposeUtility;
    private readonly IDescriptionsGenerator _descriptionsGenerator;
    private readonly Func<INamedTypeSymbol, ContainerInfo> _containerInfoFactory;
    private readonly Func<ContainerInfo, IExecuteContainerContext> _executeContainerContextFactory;

    internal ExecuteImpl(
        GeneratorExecutionContext context,
        IRangeUtility rangeUtility,
        IRequiredKeywordUtility requiredKeywordUtility,
        IDisposeUtility disposeUtility,
        IDescriptionsGenerator descriptionsGenerator,
        Func<INamedTypeSymbol, ContainerInfo> containerInfoFactory,
        Func<ContainerInfo, IExecuteContainerContext> executeContainerContextFactory)
    {
        _context = context;
        _rangeUtility = rangeUtility;
        _requiredKeywordUtility = requiredKeywordUtility;
        _disposeUtility = disposeUtility;
        _descriptionsGenerator = descriptionsGenerator;
        _containerInfoFactory = containerInfoFactory;
        _executeContainerContextFactory = executeContainerContextFactory;
    }

    public void Execute()
    {
        var containersGenerated = false;
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
                using var executeContainer = _executeContainerContextFactory(containerInfo);
                executeContainer.Execute();
                containersGenerated = true;
            }
        }
        
        // Generate the remaining code only if there actually are containers that were generated
        // It can happen that this generator is executed in project where it is not needed (e.g. a test project)
        if (!containersGenerated)
            return;
        
        var requiredKeywordTypesFile = _requiredKeywordUtility.GenerateRequiredKeywordTypesFile();
        if (requiredKeywordTypesFile is not null)
        {
            var requiredSource = CSharpSyntaxTree
                .ParseText(SourceText.From(requiredKeywordTypesFile, Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
            
            _context.AddSource($"{Constants.NamespaceForGeneratedUtilities}.RequiredKeywordTypes.cs", requiredSource);
        }
        
        var disposeUtilityCode = CSharpSyntaxTree
            .ParseText(SourceText.From(_disposeUtility.GenerateSingularDisposeFunctionsFile(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        
        _context.AddSource($"{Constants.NamespaceForGeneratedUtilities}.{_disposeUtility.ClassName}.cs", disposeUtilityCode);
        
        var descriptionsCode = _descriptionsGenerator.Generate();
        if (descriptionsCode is not null)
        {
            var descriptionsSource = CSharpSyntaxTree
                .ParseText(SourceText.From(descriptionsCode, Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
            
            _context.AddSource($"{Constants.NamespaceForGeneratedUtilities}.Descriptions.cs", descriptionsSource);
        }
    }
}