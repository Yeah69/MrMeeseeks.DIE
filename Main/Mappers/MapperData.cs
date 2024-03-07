using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Mappers;

internal abstract record MapperData;

internal sealed record VanillaMapperData : MapperData;
internal sealed record OverridingMapperData(ImmutableQueue<(INamedTypeSymbol, INamedTypeSymbol)> Override) : MapperData;
internal sealed record OverridingWithDecorationMapperData((INamedTypeSymbol, INamedTypeSymbol) Override) : MapperData;

internal interface IMapperDataToFunctionKeyTypeConverter
{
    ITypeSymbol Convert(MapperData data, ITypeSymbol initialKey);
}

internal sealed class MapperDataToFunctionKeyTypeConverter : IMapperDataToFunctionKeyTypeConverter
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