using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MrMeeseeks.DIE;

internal interface IContainerDieExceptionGenerator 
{
    void Generate(string namespaceName, string containerClassName, DieException exception);
}

internal class ContainerDieExceptionGenerator : IContainerDieExceptionGenerator
{
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;

    internal ContainerDieExceptionGenerator(
        GeneratorExecutionContext context,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous)
    {
        _context = context;
        _wellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
    }

    public void Generate(string namespaceName, string containerClassName, DieException exception)
    {
        var generatedContainer = new StringBuilder()
            .AppendLine($"#nullable enable")
            .AppendLine($"namespace {namespaceName}")
            .AppendLine($"{{")
            .AppendLine($"partial class {containerClassName}")
            .AppendLine($"{{")
            .AppendLine($"public {_wellKnownTypesMiscellaneous.DieExceptionKind.FullName()} ExceptionKind_0_0 => {_wellKnownTypesMiscellaneous.DieExceptionKind.FullName()}.{exception.Kind.ToString()};")
            .AppendLine($"}}")
            .AppendLine($"}}")
            .AppendLine($"#nullable disable");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        _context.AddSource($"{namespaceName}.{containerClassName}.g.cs", containerSource);
    }
}