using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MrMeeseeks.DIE.Spy
{
    internal interface IGetAllImplementations
    {
        IReadOnlyList<INamedTypeSymbol> AllImplementations { get; }
    }

    internal class GetAllImplementations : IGetAllImplementations
    {
        private GeneratorExecutionContext _context;

        public GetAllImplementations(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public IReadOnlyList<INamedTypeSymbol> AllImplementations => new ReadOnlyCollection<INamedTypeSymbol>(_context.Compilation.SyntaxTrees
                .Select(st => (st, _context.Compilation.GetSemanticModel(st)))
                .SelectMany(t => t.st
                    .GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(c => t.Item2.GetDeclaredSymbol(c))
                    .Where(c => c is not null)
                    .OfType<INamedTypeSymbol>())
                .ToList());
    }
}
