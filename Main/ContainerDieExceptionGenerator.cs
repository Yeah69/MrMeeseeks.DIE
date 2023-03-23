using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE;

internal interface IContainerDieExceptionGenerator 
{
    void Generate(Exception exception);
}

internal class ContainerDieExceptionGenerator : IContainerDieExceptionGenerator
{
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private readonly INamedTypeSymbol _containerType;

    internal ContainerDieExceptionGenerator(
        GeneratorExecutionContext context,
        IContainerInfoContext containerInfoContext,
        IContainerWideContext containerWideContext)
    {
        _context = context;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
        _wellKnownTypesMiscellaneous = containerWideContext.WellKnownTypesMiscellaneous;
        _containerType = containerInfoContext.ContainerInfo.ContainerType;
    }

    public void Generate(Exception exception)
    {
        var generatedContainer = new StringBuilder()
            .AppendLine($$"""
#nullable enable
namespace {{_containerType.ContainingNamespace.FullName()}}
{
partial class {{_containerType.Name}} : {{_wellKnownTypes.IAsyncDisposable.FullName()}}, {{_wellKnownTypes.IDisposable.FullName()}}
{
""");
        
        foreach (var constructor in _containerType.InstanceConstructors)
            generatedContainer.AppendLine($$"""
public static {{_containerType.FullName()}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}})
{
return new {{_containerType.FullName()}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Name}: {p.Name}"))}});
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
        _context.AddSource($"{_containerType.ContainingNamespace.FullName()}.{_containerType.Name}.g.cs", containerSource);
    }
}