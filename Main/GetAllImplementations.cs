using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MrMeeseeks.DIE;

internal interface IGetAllImplementations
{
    IReadOnlyList<INamedTypeSymbol> AllImplementations { get; }
}

internal class GetAllImplementations : IGetAllImplementations
{
    internal GetAllImplementations(
        GeneratorExecutionContext context,
        ITypesFromTypeAggregatingAttributes typesFromTypeAggregatingAttributes)
    {
        var implementationsOfThisAssembly = context.Compilation.SyntaxTrees
            .Select(st => (st, context.Compilation.GetSemanticModel(st)))
            .SelectMany(t => t.st
                .GetRoot()
                .DescendantNodesAndSelf()
                .Where(e => e is ClassDeclarationSyntax or StructDeclarationSyntax or RecordDeclarationSyntax)
                .Select(c => t.Item2.GetDeclaredSymbol(c))
                .Where(c => c is not null)
                .OfType<INamedTypeSymbol>());

        var spiedImplementations = typesFromTypeAggregatingAttributes
            .Spy
            .SelectMany(t => t?.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(ms => !ms.ReturnsVoid)
                .Select(ms => ms.ReturnType)
                .OfType<INamedTypeSymbol>());

        AllImplementations = implementationsOfThisAssembly
            .Concat(typesFromTypeAggregatingAttributes.Implementation)
            .Concat(spiedImplementations)
            .ToList();
    }

    public IReadOnlyList<INamedTypeSymbol> AllImplementations { get; }
}