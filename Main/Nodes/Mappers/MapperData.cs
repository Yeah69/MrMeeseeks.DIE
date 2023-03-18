using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Mappers;

internal abstract record MapperData;

internal record VanillaMapperData : MapperData;
internal record OverridingMapperData(ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> Override) : MapperData;
internal record OverridingWithDecorationMapperData((INamedTypeSymbol, INamedTypeSymbol) Override) : MapperData;

internal interface IMapperDataToFunctionKeyTypeConverter
{
    ITypeSymbol Convert(MapperData data, ITypeSymbol initialKey);
}

internal class MapperDataToFunctionKeyTypeConverter : IMapperDataToFunctionKeyTypeConverter
{
    public ITypeSymbol Convert(MapperData data, ITypeSymbol initialKey)
    {
        return data switch
        {
            OverridingMapperData overridingMapperData when
                overridingMapperData.Override.Peek() is var (abstraction, implementation) => 
                CreateSubstitutedKey(initialKey, abstraction, implementation),
            OverridingWithDecorationMapperData { Override: var (abstraction, implementation) } => 
                CreateSubstitutedKey(initialKey, abstraction, implementation),
            _ => initialKey
        };

        ITypeSymbol CreateSubstitutedKey(ITypeSymbol currentKey, ITypeSymbol abstraction, ITypeSymbol implementation)
        {
            if (CustomSymbolEqualityComparer.Default.Equals(currentKey, abstraction))
                return implementation.WithNullableAnnotation(currentKey.NullableAnnotation);
            if (currentKey is INamedTypeSymbol namedCurrentKey
                && namedCurrentKey.TypeArguments.Any())
            {
                var nullableAnnotation = currentKey.NullableAnnotation;
                var substitutedTypeArguments = namedCurrentKey
                    .TypeArguments
                    .Select(t => CreateSubstitutedKey(t, abstraction, implementation))
                    .ToArray();
                return namedCurrentKey
                    .ConstructUnboundGenericType()
                    .Construct(substitutedTypeArguments)
                    .WithNullableAnnotation(nullableAnnotation);
            }
            return currentKey;
        }
    }
}