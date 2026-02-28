using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.CodeGeneration;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE;

internal sealed class ExecuteImpl
{
    private readonly GeneratorExecutionContext _context;
    private readonly RangeUtility _rangeUtility;
    private readonly RequiredKeywordUtility _requiredKeywordUtility;
    private readonly DisposeUtility _disposeUtility;
    private readonly DescriptionsGenerator _descriptionsGenerator;
    private readonly InterceptorDecoratorGenerator _interceptorDecoratorGenerator;
    private readonly Func<INamedTypeSymbol, ContainerInfo> _containerInfoFactory;
    private readonly Func<ContainerInfo, IExecuteContainerContext> _executeContainerContextFactory;

    internal ExecuteImpl(
        GeneratorExecutionContext context,
        RangeUtility rangeUtility,
        RequiredKeywordUtility requiredKeywordUtility,
        DisposeUtility disposeUtility,
        DescriptionsGenerator descriptionsGenerator,
        InterceptorDecoratorGenerator interceptorDecoratorGenerator,
        Func<INamedTypeSymbol, ContainerInfo> containerInfoFactory,
        Func<ContainerInfo, IExecuteContainerContext> executeContainerContextFactory)
    {
        _context = context;
        _rangeUtility = rangeUtility;
        _requiredKeywordUtility = requiredKeywordUtility;
        _disposeUtility = disposeUtility;
        _descriptionsGenerator = descriptionsGenerator;
        _interceptorDecoratorGenerator = interceptorDecoratorGenerator;
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
                // Container types can be nested in other types
                //.SelectMany(SelfAndNestedTypes)
                .Where(x => _rangeUtility.IsAContainer(x))
                .Select(_containerInfoFactory)
                .ToList();
            foreach (var containerInfo in containerInfos)
            { 
                using var executeContainer = _executeContainerContextFactory(containerInfo);
                executeContainer.Execute();
                containersGenerated = true;
            }

            continue;

            IEnumerable<INamedTypeSymbol> SelfAndNestedTypes(INamedTypeSymbol symbol)
            {
                yield return symbol;
                foreach (INamedTypeSymbol nested in symbol.GetTypeMembers().SelectMany(SelfAndNestedTypes))
                    yield return nested;
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
        
        var interceptionCode = _interceptorDecoratorGenerator.Generate();
        if (interceptionCode is not null)
        {
            var descriptionsSource = CSharpSyntaxTree
                .ParseText(SourceText.From(interceptionCode, Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
            
            _context.AddSource($"{Constants.NamespaceForGeneratedUtilities}.Interception.cs", descriptionsSource);
        }
    }
}