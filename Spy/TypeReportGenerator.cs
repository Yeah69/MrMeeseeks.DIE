using System;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MrMeeseeks.DIE.Spy;

internal interface ITypeReportGenerator
{
    void Generate(string namespaceName);
}

internal class TypeReportGenerator : ITypeReportGenerator
{
    private readonly GeneratorExecutionContext _context;
    private readonly IGetAllImplementations _getAllImplementations;

    public TypeReportGenerator(
        GeneratorExecutionContext context,
        IGetAllImplementations getAllImplementations)
    {
        _context = context;
        _getAllImplementations = getAllImplementations;
    }

    public void Generate(string namespaceName)
    {
        var allNonStaticImplementations = _getAllImplementations
            .AllNonStaticImplementations
            .ToList();
        var generatedContainer = new StringBuilder()
            .AppendLine($"namespace {namespaceName}")
            .AppendLine("{");

        generatedContainer = GenerateBody(Accessibility.Public, allNonStaticImplementations, generatedContainer);

        generatedContainer = GenerateBody(Accessibility.Internal, allNonStaticImplementations, generatedContainer);

        generatedContainer = generatedContainer
            .AppendLine("}");

        var containerSource = CSharpSyntaxTree
            .ParseText(SourceText.From(generatedContainer.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
        _context.AddSource("TypeReport.g.cs", containerSource);

        static StringBuilder GenerateBody(Accessibility accessModifier, IList<INamedTypeSymbol> allNonStaticImplementations, StringBuilder generatedContainer)
        {
            var lowerCaseAccessModifier = accessModifier.ToString().ToLower();
            var pascalCaseAccessModifier = accessModifier.ToString();
                
            var types = allNonStaticImplementations
                .Where(t => t.DeclaredAccessibility == accessModifier)
                .ToList();
                
            generatedContainer = generatedContainer
                .AppendLine($"{lowerCaseAccessModifier} interface I{pascalCaseAccessModifier}TypeReport")
                .AppendLine("{");

            var i = -1;
            generatedContainer = types.Aggregate(
                generatedContainer, 
                (current, type) => current.AppendLine(
                    $"{FullName(type)} Type{++i}{(type.TypeArguments.Length > 0 ? $"<{string.Join(", ", type.TypeArguments.Select(t => t.Name))}>" : "")}(){TypeArgumentsConstraintsString(type)};"));

            generatedContainer = generatedContainer
                .AppendLine("}")
                .AppendLine($"{lowerCaseAccessModifier} enum {pascalCaseAccessModifier}ConstructorReport")
                .AppendLine("{");

             generatedContainer = allNonStaticImplementations
                .SelectMany(i => i.Constructors)
                .Where(c => c.DeclaredAccessibility == accessModifier)
                .Select(c => (c, $"{c.ContainingType.Name}{(c.Parameters.Any() ? $"_{string.Join("_", c.Parameters.Select(p => p.Type.Name))}" : "")}"))
                .GroupBy(t => t.Item2)
                .SelectMany(g => !g.Any()
                    ? Array.Empty<(IMethodSymbol, string)>()
                    : g.Count() == 1
                        ? new (IMethodSymbol, string)[] { (g.First().c, g.Key) }
                        : g.Select((t, i) => (t.c, $"{t.Item2}_{i}")))
                .Select(t => @$"
[global::{typeof(EnumMemberAttribute).FullName}({nameof(EnumMemberAttribute.Value)} = ""{t.c.ToDisplayString()}"")]
[global::{typeof(ConstructorChoiceAttribute).FullName}(typeof({t.c.ContainingType.FullName()}){(t.c.Parameters.Any() ? $", {string.Join(", ", t.c.Parameters.Select(p => $"typeof({p.Type.FullName()})"))}" : "")})]
{t.Item2},")
                .Aggregate(
                    generatedContainer,
                    (current, line) => current.AppendLine(line));
             
             generatedContainer = generatedContainer
                 .AppendLine("}");
                
            return generatedContainer;
        }
            
        static string FullName(ISymbol type) =>
            type.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeRef));

        static string TypeArgumentsConstraintsString(INamedTypeSymbol namedTypeSymbol)
        {
            var displayStringWithoutConstraints = namedTypeSymbol.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions: SymbolDisplayMemberOptions.IncludeRef));
            var displayStringWithConstraints = namedTypeSymbol.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
                memberOptions: SymbolDisplayMemberOptions.IncludeRef));
                
            var ret = displayStringWithConstraints.Remove(0, displayStringWithoutConstraints.Length);
            return ret;
        }
    }
}