namespace MrMeeseeks.DIE.Extensions;

internal static class INamedTypeSymbolExtensions
{
    internal static IEnumerable<INamedTypeSymbol> AllDerivedTypes(this INamedTypeSymbol type)
    {
        var concreteTypes = new List<INamedTypeSymbol>();
        var temp = type;
        while (temp is {})
        {
            concreteTypes.Add(temp);
            temp = temp.BaseType;
        }
        return type
            .AllInterfaces
            .Append(type)
            .Concat(concreteTypes);
    }
    
    internal static INamedTypeSymbol UnboundIfGeneric(this INamedTypeSymbol type) =>
        type.IsGenericType && !type.IsUnboundGenericType
            ? type.ConstructUnboundGenericType()
            : type;
}