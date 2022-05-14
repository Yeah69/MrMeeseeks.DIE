namespace MrMeeseeks.DIE.Extensions;

internal static class INamedTypeSymbolExtensions
{
    internal static IEnumerable<INamedTypeSymbol> AllDerivedTypesAndSelf(this INamedTypeSymbol type)
    {
        var baseTypesAndSelf = new List<INamedTypeSymbol>();
        if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
        {
            var temp = type;
            while (temp is {})
            {
                baseTypesAndSelf.Add(temp);
                temp = temp.BaseType;
            }
        }
        else if (type.TypeKind is TypeKind.Interface)
            baseTypesAndSelf.Add(type);
        
        return type
            .AllInterfaces
            .Concat(baseTypesAndSelf);
    }
    
    internal static INamedTypeSymbol UnboundIfGeneric(this INamedTypeSymbol type) =>
        type.IsGenericType && !type.IsUnboundGenericType
            ? type.ConstructUnboundGenericType()
            : type;
    
    internal static INamedTypeSymbol OriginalDefinitionIfUnbound(this INamedTypeSymbol type) =>
        type.IsUnboundGenericType
            ? type.OriginalDefinition
            : type;
}