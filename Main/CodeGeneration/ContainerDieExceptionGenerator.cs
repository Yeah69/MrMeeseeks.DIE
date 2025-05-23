﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MrMeeseeks.DIE.Logging;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.CodeGeneration;

internal interface IContainerDieExceptionGenerator 
{
    void Generate(Exception? exception);
}

internal sealed class ContainerDieExceptionGenerator : IContainerDieExceptionGenerator
{
    private readonly GeneratorExecutionContext _context;
    private readonly IDiagLogger _diagLogger;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly WellKnownTypesMiscellaneous _wellKnownTypesMiscellaneous;
    private readonly INamedTypeSymbol _containerType;
    private readonly WellKnownTypesCollections _wellKnownTypesCollections;

    internal ContainerDieExceptionGenerator(
        GeneratorExecutionContext context,
        IContainerInfo containerInfo,
        WellKnownTypes wellKnownTypes,
        WellKnownTypesCollections wellKnownTypesCollections,
        WellKnownTypesMiscellaneous wellKnownTypesMiscellaneous,
        IDiagLogger diagLogger)
    {
        _context = context;
        _diagLogger = diagLogger;
        _wellKnownTypes = wellKnownTypes;
        _wellKnownTypesMiscellaneous = wellKnownTypesMiscellaneous;
        _wellKnownTypesCollections = wellKnownTypesCollections;
        _containerType = containerInfo.ContainerType;
    }

    public void Generate(Exception? exception)
    {
        var asyncDisposableBit = _wellKnownTypes.IAsyncDisposable is not null
            ? $"{_wellKnownTypes.IAsyncDisposable.FullName()}, "
            : "";
        
        var genericParameters = _containerType.TypeParameters.Length == 0
            ? ""
            : $"<{string.Join(", ", _containerType.TypeParameters.Select(p => p.Name))}>";
        
        var generatedContainer = new StringBuilder()
            .AppendLine($$"""
                #nullable enable
                namespace {{_containerType.ContainingNamespace.FullName()}}
                {
                partial class {{_containerType.Name}}{{genericParameters}} : {{asyncDisposableBit}}{{_wellKnownTypes.IDisposable.FullName()}}
                {
                """);
        
        foreach (var constructor in _containerType.InstanceConstructors)
            generatedContainer.AppendLine($$"""
                public static {{_containerType.FullName()}} {{Constants.CreateContainerFunctionName}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Type.FullName()} {p.Name}"))}})
                {
                return new {{_containerType.FullName()}}({{string.Join(", ", constructor.Parameters.Select(p => $"{p.Name}: {p.Name}"))}});
                }
                """);
        var listOfKinds = _wellKnownTypesCollections.List1.Construct(_wellKnownTypesMiscellaneous.DieExceptionKind);

        generatedContainer.AppendLine($$"""
            public {{listOfKinds.FullName()}} ExceptionKinds_0_0 { get; } = new {{listOfKinds.FullName()}}() { {{string.Join(", ", _diagLogger.ErrorKinds.Select(k => $"{_wellKnownTypesMiscellaneous.DieExceptionKind.FullName()}.{k.ToString()}"))}} };
            public {{_wellKnownTypes.String.FullName()}}[] DieBuildErrorCodes { get; } = new {{_wellKnownTypes.String.FullName()}}[] { {{string.Join(", ", _diagLogger.DieBuildErrorCodes.Select(c => $"\"{c}\""))}} };
            public string ExceptionToString_0_1 => @"{{exception?.ToString() ?? "no exception"}}";
            public void Dispose(){}
            {{(_wellKnownTypes.ValueTask is not null ? $"public {_wellKnownTypes.ValueTask.FullName()} DisposeAsync() => new {_wellKnownTypes.ValueTask.FullName()}();" : "")}}
            }
            }
            #nullable disable
            """);

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText(); _context.AddSource($"{_containerType.ContainingNamespace.FullName()}.{_containerType.Name}.g.cs", containerSource);
    }
}