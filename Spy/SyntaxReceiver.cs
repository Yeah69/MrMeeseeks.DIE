using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MrMeeseeks.DIE.Spy
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Candidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                Candidates.Add(classDeclarationSyntax);
            }
        }
    }
}
