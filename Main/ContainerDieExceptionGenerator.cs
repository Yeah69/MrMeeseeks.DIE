using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IContainerDieExceptionGenerator 
{
    void Generate(INamedTypeSymbol containerType, Exception exception);
}

internal class ContainerDieExceptionGenerator : IContainerDieExceptionGenerator
{
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;

    internal ContainerDieExceptionGenerator(
        GeneratorExecutionContext context,
        IContainerWideContext containerWideContext)
    {
        _context = context;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
    }

    public void Generate(INamedTypeSymbol containerType, Exception exception)
    {
        var generatedContainer = new StringBuilder()
            .AppendLine($$"""
#nullable enable
namespace {{containerType.ContainingNamespace.FullName()}}
{
partial class {{containerType.Name}} : {{_wellKnownTypes.IAsyncDisposable.FullName()}}, {{_wellKnownTypes.IDisposable.FullName()}}
{
""");
        
        foreach (var constructor in containerType.InstanceConstructors)
            generatedContainer.AppendLine($$"""
public static {{containerType.FullName()}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}})
{
return new {{containerType.FullName()}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Name}: {p.Name}"))}});
}
""");

        generatedContainer.AppendLine($$"""
public {{_wellKnownTypesMiscellaneous.DieExceptionKind.FullName()}} ExceptionKind_0_0 => {{_wellKnownTypesMiscellaneous.DieExceptionKind.FullName()}}.{{((exception as DieException)?.Kind ?? DieExceptionKind.NoneDIE).ToString()}};
public string ExceptionToString_0_1 => @"{{exception}}";
public void Dispose(){}
public {{_wellKnownTypes.ValueTask.FullName()}} DisposeAsync() => new {{_wellKnownTypes.ValueTask.FullName()}}();
}
}
#nullable disable
""");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        _context.AddSource($"{containerType.ContainingNamespace.FullName()}.{containerType.Name}.g.cs", containerSource);
    }
}