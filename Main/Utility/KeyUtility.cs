using Microsoft.CodeAnalysis.CSharp;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Utility;

internal sealed class KeyUtility(WellKnownTypes wellKnownTypes)
{
    internal string GenerateKeyLiteral(ITypeSymbol type, object value)
    {
        return type.TypeKind == TypeKind.Enum 
            ? $"({type.FullName()}) {SymbolDisplay.FormatPrimitive(value, true, false)}" 
            : CustomSymbolEqualityComparer.Default.Equals(type, wellKnownTypes.Type) 
                ? $"typeof({(value as ITypeSymbol)?.FullName() ?? ""})" 
                : SymbolDisplay.FormatPrimitive(value, true, false);
    }
}