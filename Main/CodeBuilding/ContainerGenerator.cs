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
    private readonly IDiagLogger _diagLogger;
    private readonly Func<IContainerInfo, ContainerResolution, IScopeCodeBuilder, IContainerCodeBuilder> _containerCodeBuilderFactory;
    private readonly Func<IContainerInfo, ScopeResolution, TransientScopeInterfaceResolution, IScopeCodeBuilder> _scopeCodeBuilderFactory;

    internal ContainerGenerator(
        GeneratorExecutionContext context,
        IDiagLogger diagLogger,
        Func<IContainerInfo, ContainerResolution, IScopeCodeBuilder, IContainerCodeBuilder> containerCodeBuilderFactory,
        Func<IContainerInfo, ScopeResolution, TransientScopeInterfaceResolution, IScopeCodeBuilder> scopeCodeBuilderFactory)
    {
        _context = context;
        _diagLogger = diagLogger;
        _containerCodeBuilderFactory = containerCodeBuilderFactory;
        _scopeCodeBuilderFactory = scopeCodeBuilderFactory;
    }

    public void Generate(IContainerInfo containerInfo, ContainerResolution containerResolution)
    {
        if (!containerInfo.IsValid)
        {
            _diagLogger.Log($"return generation");
            return;
        }

        var containerCodeBuilder = _containerCodeBuilderFactory(containerInfo, containerResolution,
            _scopeCodeBuilderFactory(containerInfo, containerResolution.DefaultScope, containerResolution.TransientScopeInterface));

        var generatedContainer = containerCodeBuilder.Build(new StringBuilder());

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        
        _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);
    }
}