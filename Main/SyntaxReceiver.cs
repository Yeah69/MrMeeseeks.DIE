using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MrMeeseeks.DIE;

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