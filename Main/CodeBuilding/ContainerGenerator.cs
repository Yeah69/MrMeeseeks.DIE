using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MrMeeseeks.DIE.CodeBuilding;

internal interface IContainerGenerator
{
    void Generate(IContainerInfo containerInfo, ContainerResolution resolvable);
}

internal class ContainerGenerator : IContainerGenerator
{
    private readonly GeneratorExecutionContext _context;
    private readonly Func<IContainerInfo, ContainerResolution, IContainerCodeBuilder> _containerCodeBuilderFactory;
    private readonly Func<IContainerInfo, TransientScopeResolution, ContainerResolution, ITransientScopeCodeBuilder> _transientScopeCodeBuilderFactory;
    private readonly Func<IContainerInfo, ScopeResolution, TransientScopeInterfaceResolution, ContainerResolution, IScopeCodeBuilder> _scopeCodeBuilderFactory;

    internal ContainerGenerator(
        GeneratorExecutionContext context,
        Func<IContainerInfo, ContainerResolution, IContainerCodeBuilder> containerCodeBuilderFactory,
        Func<IContainerInfo, TransientScopeResolution, ContainerResolution, ITransientScopeCodeBuilder> transientScopeCodeBuilderFactory,
        Func<IContainerInfo, ScopeResolution, TransientScopeInterfaceResolution, ContainerResolution, IScopeCodeBuilder> scopeCodeBuilderFactory)
    {
        _context = context;
        _containerCodeBuilderFactory = containerCodeBuilderFactory;
        _transientScopeCodeBuilderFactory = transientScopeCodeBuilderFactory;
        _scopeCodeBuilderFactory = scopeCodeBuilderFactory;
    }

    public void Generate(IContainerInfo containerInfo, ContainerResolution containerResolution)
    {
        var containerCodeBuilder = _containerCodeBuilderFactory(containerInfo, containerResolution);
        
        GenerateRange(containerCodeBuilder, containerResolution);
        
        foreach (var transientScopeResolution in containerResolution.TransientScopes)
        {
            var transientScopeCodeBuilder = _transientScopeCodeBuilderFactory(containerInfo, transientScopeResolution, containerResolution);
            GenerateRange(transientScopeCodeBuilder, transientScopeResolution);
        }
        
        foreach (var scopeResolution in containerResolution.Scopes)
        {
            var scopeCodeBuilder = _scopeCodeBuilderFactory(containerInfo, scopeResolution, containerResolution.TransientScopeInterface, containerResolution);
            GenerateRange(scopeCodeBuilder, scopeResolution);
        }

        void GenerateRange(IRangeCodeBaseBuilder rangeCodeBaseBuilder, IRangeResolution rangeResolution)
        {
            var generatedGeneral = rangeCodeBaseBuilder.BuildGeneral(new StringBuilder());
            RenderSourceFile($"{containerInfo.Namespace}.{containerInfo.Name}.{rangeResolution.Name}.g.cs", generatedGeneral);
            generatedGeneral.Clear();
        
            foreach (var createFunction in rangeResolution.CreateFunctions)
            {
                var generatedCreateFunction = rangeCodeBaseBuilder.BuildCreateFunction(new StringBuilder(), createFunction);
                RenderSourceFile($"{containerInfo.Namespace}.{containerInfo.Name}.{rangeResolution.Name}.{createFunction.Reference}.g.cs", generatedCreateFunction);
                generatedCreateFunction.Clear();
            }
        
            foreach (var rangedInstanceFunctionGroup in rangeResolution.RangedInstanceFunctionGroups)
            {
                var generatedInstanceFunctionGroupFunction = rangeCodeBaseBuilder.BuildRangedFunction(new StringBuilder(), rangedInstanceFunctionGroup);
                RenderSourceFile($"{containerInfo.Namespace}.{containerInfo.Name}.{rangeResolution.Name}.{rangedInstanceFunctionGroup.Overloads[0].Reference}.g.cs", generatedInstanceFunctionGroupFunction);
                generatedInstanceFunctionGroupFunction.Clear();
            }
        }
        
        void RenderSourceFile(string fileName, StringBuilder compiledCode)
        {
            var containerSource = CSharpSyntaxTree
                .ParseText(SourceText.From(compiledCode.ToString(), Encoding.UTF8))
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
        
            _context.AddSource(fileName, containerSource);
        }
    }
}