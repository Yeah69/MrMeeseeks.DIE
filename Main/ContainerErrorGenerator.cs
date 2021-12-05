using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MrMeeseeks.DIE;

internal interface IContainerErrorGenerator 
{
    void Generate(IContainerInfo containerInfo, IReadOnlyList<ErrorTreeItem> errorTreeItems);
}

internal class ContainerErrorGenerator : IContainerErrorGenerator
{
    private readonly GeneratorExecutionContext _context;

    internal ContainerErrorGenerator(
        GeneratorExecutionContext context) =>
        _context = context;

    public void Generate(IContainerInfo containerInfo, IReadOnlyList<ErrorTreeItem> errorTreeItems)
    {
        var generatedContainer = new StringBuilder()
            .AppendLine($"namespace {containerInfo.Namespace}")
            .AppendLine($"{{")
            .AppendLine($"partial class {containerInfo.Name}")
            .AppendLine($"{{")
            .AppendLine($"public object Resolve()")
            .AppendLine($"{{")
            .AppendLine($"throw new Exception(@\"{string.Join(Environment.NewLine, errorTreeItems.Select(eri => eri.Message))}\");")
            .AppendLine($"}}")
            .AppendLine($"}}")
            .AppendLine($"}}");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        _context.AddSource($"{containerInfo.Namespace}.{containerInfo.Name}.g.cs", containerSource);
    }
}