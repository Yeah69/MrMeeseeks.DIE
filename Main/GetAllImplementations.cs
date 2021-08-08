using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MrMeeseeks.DIE
{
    internal interface IGetAllImplementations
    {
        IReadOnlyList<INamedTypeSymbol> AllImplementations { get; }
    }

    internal class GetAllImplementations : IGetAllImplementations
    {
        private GeneratorExecutionContext context;

        public GetAllImplementations(GeneratorExecutionContext context)
        {
            this.context = context;
        }

        public IReadOnlyList<INamedTypeSymbol> AllImplementations => new ReadOnlyCollection<INamedTypeSymbol>(context.Compilation.SyntaxTrees
                .Select(st => (st, context.Compilation.GetSemanticModel(st)))
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
