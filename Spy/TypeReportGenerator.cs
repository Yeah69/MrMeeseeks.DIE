using System;
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
                .AppendLine($"{lowerCaseAccessModifier} static class {pascalCaseAccessModifier}ConstructorReport")
                .AppendLine("{");
            
            foreach (var implementation in allNonStaticImplementations
                         .Where(i => i.Constructors.Any(c => c.DeclaredAccessibility == accessModifier)))
            {
                generatedContainer = generatedContainer
                    .AppendLine($"{lowerCaseAccessModifier} static class {implementation.FullName().Replace(':','_').Replace('.', '_')}")
                    .AppendLine("{");
                
                foreach (var constructor in implementation.Constructors
                             .Where(c => c.DeclaredAccessibility == accessModifier))
                {
                    int j = 0;
                    generatedContainer = generatedContainer
                        .AppendLine(
                            $"{lowerCaseAccessModifier} interface {(constructor.Parameters.Any() ? "" : "_")}{string.Join("_", constructor.Parameters.Select(p => p.Type.Name))}")
                        .AppendLine($"{{")
                        .AppendLine($"{implementation.FullName()} _{(j++).ToString().PadLeft(20, '0')}();");
                    
                    foreach (var constructorParameter in constructor.Parameters)
                    {
                        generatedContainer = generatedContainer
                            .AppendLine($"{constructorParameter.Type.FullName()} _{(j++).ToString().PadLeft(20, '0')}();");
                    }
                    
                    generatedContainer = generatedContainer
                        .AppendLine($"}}");
                }
                
                generatedContainer = generatedContainer
                    .AppendLine("}");
            }
             
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