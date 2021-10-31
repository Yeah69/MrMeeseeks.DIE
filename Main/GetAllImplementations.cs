using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MrMeeseeks.DIE
{
    internal interface IGetAllImplementations
    {
        IReadOnlyList<INamedTypeSymbol> AllImplementations { get; }
    }

    internal class GetAllImplementations : IGetAllImplementations
    {
        public GetAllImplementations(
            GeneratorExecutionContext context,
            ITypesFromAttributes typesFromAttributes)
        {
            var implementationsOfThisAssembly = context.Compilation.SyntaxTrees
                .Select(st => (st, context.Compilation.GetSemanticModel(st)))
                .SelectMany(t => t.st
                    .GetRoot()
                    .DescendantNodesAndSelf()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(c => t.Item2.GetDeclaredSymbol(c))
                    .Where(c => c is not null)
                    .OfType<INamedTypeSymbol>());

            var spiedImplementations = typesFromAttributes
                .Spy
                .SelectMany(t => t?.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(ms => !ms.ReturnsVoid)
                    .Select(ms => ms.ReturnType)
                    .OfType<INamedTypeSymbol>());

            AllImplementations = implementationsOfThisAssembly.Concat(spiedImplementations).ToList();
        }

        public IReadOnlyList<INamedTypeSymbol> AllImplementations { get; }
    }
}
