using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE.Spy
{
    internal static class RoslynExtensions
    {
        // Picked from https://github.com/YairHalberstadt/stronginject Thank you!
        public static string FullName(this ITypeSymbol type) =>
            type.ToDisplayString(new SymbolDisplayFormat(
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
                memberOptions: SymbolDisplayMemberOptions.IncludeRef));
    }
}