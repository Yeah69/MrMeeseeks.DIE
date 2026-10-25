using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration;

internal sealed class ContainerDieExceptionGenerator(
    GeneratorExecutionContext context,
    ContainerInfo containerInfo,
    WellKnownTypes wellKnownTypes,
    WellKnownTypesCollections wellKnownTypesCollections,
    WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
    DiagLogger diagLogger)
{
    public void Generate(Exception? exception)
    {
        var containerType = containerInfo.ContainerType;
        
        var asyncDisposableBit = wellKnownTypes.IAsyncDisposable is not null
            ? $"{wellKnownTypes.IAsyncDisposable.FullName()}, "
            : "";
        
        var genericParameters = containerType.TypeParameters.Length == 0
            ? ""
            : $"<{string.Join(", ", containerType.TypeParameters.Select(p => p.Name))}>";
        
        var generatedContainer = new StringBuilder().AppendLine("#nullable enable");
        
        if (containerInfo.Namespace is not null)
            generatedContainer.AppendLine(
                $$"""
                  namespace {{containerInfo.Namespace}}
                  {
                  """);
        
        foreach (var nestingParentName in containerInfo.ContainingTypeNames)
        {
            generatedContainer.AppendLine(
                $$"""
                  partial class {{nestingParentName}}
                  {
                  """);
        }

        generatedContainer.AppendLine(
            $$"""
              partial class {{containerType.Name}}{{genericParameters}} : {{asyncDisposableBit}}{{wellKnownTypes.IDisposable.FullName()}}
              {
              """);
        
        foreach (var constructor in containerType.InstanceConstructors)
            generatedContainer.AppendLine($$"""
                public static {{containerType.FullName()}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}})
                {
                return new {{containerType.FullName()}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Name}: {p.Name}"))}});
                }
                """);
        var listOfKinds = wellKnownTypesCollections.List1.Construct(wellKnownTypesMiscellaneous.DieExceptionKind);

        generatedContainer.AppendLine($$"""
            public {{listOfKinds.FullName()}} ExceptionKinds_0_0 { get; } = new {{listOfKinds.FullName()}}() { {{string.Join(", ", diagLogger.ErrorKinds.Select(k => $"{wellKnownTypesMiscellaneous.DieExceptionKind.FullName()}.{k.ToString()}"))}} };
            public {{wellKnownTypes.String.FullName()}}[] DieBuildErrorCodes { get; } = new {{wellKnownTypes.String.FullName()}}[] { {{string.Join(", ", diagLogger.DieBuildErrorCodes.Select(c => $"\"{c}\""))}} };
            public string ExceptionToString_0_1 => @"{{exception?.ToString() ?? "no exception"}}";
            public void Dispose(){}
            {{(wellKnownTypes.ValueTask is not null ? $"public {wellKnownTypes.ValueTask.FullName()} DisposeAsync() => new {wellKnownTypes.ValueTask.FullName()}();" : "")}}
            }
            """);
        
        if (containerInfo.Namespace is not null)
            generatedContainer.AppendLine("}");
        
        generatedContainer.AppendLine(string.Join(Environment.NewLine, Enumerable.Repeat("}", containerInfo.ContainingTypeNames.Length)));
        
        generatedContainer.AppendLine("#nullable disable");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        context.AddSource(containerInfo.GenerateHintPath(suffix: ".Exception"), containerSource);
    }
}