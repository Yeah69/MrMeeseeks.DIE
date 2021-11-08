using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MrMeeseeks.DIE.Spy;

internal interface IGetAllImplementations
{
    IReadOnlyList<INamedTypeSymbol> AllNonStaticImplementations { get; }
}

internal class GetAllImplementations : IGetAllImplementations
{
    private readonly GeneratorExecutionContext _context;

    public GetAllImplementations(GeneratorExecutionContext context)
    {
        _context = context;
    }

    public IReadOnlyList<INamedTypeSymbol> AllNonStaticImplementations => new ReadOnlyCollection<INamedTypeSymbol>(_context.Compilation.SyntaxTrees
        .Select(st => (st, _context.Compilation.GetSemanticModel(st)))
        .SelectMany(t => t.st
            .GetRoot()
            .DescendantNodesAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => t.Item2.GetDeclaredSymbol(c))
            .Where(c => c is not null)
            .OfType<INamedTypeSymbol>())
        .Where(nts => !nts.IsStatic)
        .ToList());
}