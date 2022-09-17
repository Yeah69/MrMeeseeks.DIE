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

        var generatedContainer = containerCodeBuilder.Build(new StringBuilder());
        
        RenderSourceFile($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", generatedContainer);

        generatedContainer.Clear();
        
        foreach (var transientScopeCodeBuilder in containerResolution.TransientScopes.Select(ts => _transientScopeCodeBuilderFactory(containerInfo, ts, containerResolution)).ToList())
        {
            var generatedTransientScope = transientScopeCodeBuilder.Build(new StringBuilder());
            
            RenderSourceFile($"{containerInfo.Namespace}.{containerInfo.Name}.{transientScopeCodeBuilder.TransientScopeResolution.Name}.g.cs", generatedTransientScope);

            generatedTransientScope.Clear();
        }
        
        foreach (var scopeCodeBuilder in containerResolution.Scopes.Select(s => _scopeCodeBuilderFactory(containerInfo, s, containerResolution.TransientScopeInterface, containerResolution)).ToList())
        {
            var generatedTransientScope = scopeCodeBuilder.Build(new StringBuilder());
            
            RenderSourceFile($"{containerInfo.Namespace}.{containerInfo.Name}.{scopeCodeBuilder.ScopeResolution.Name}.g.cs", generatedTransientScope);

            generatedTransientScope.Clear();
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