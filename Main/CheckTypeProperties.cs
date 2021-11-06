using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    public interface ICheckTypeProperties
    {
        bool ShouldBeManaged(INamedTypeSymbol type);
        bool ShouldBeSingleInstance(INamedTypeSymbol type);
    }

    internal class CheckTypeProperties : ICheckTypeProperties
    {
        private readonly IImmutableSet<ISymbol?> _transientTypes;
        private readonly IImmutableSet<ISymbol?> _singleInstanceTypes;

        public CheckTypeProperties(
            IGetAllImplementations getAllImplementations,
            ITypesFromAttributes typesFromAttributes)
        {
            _transientTypes = GetSetOfTypesWithProperties(typesFromAttributes.Transient);
            _singleInstanceTypes = GetSetOfTypesWithProperties(typesFromAttributes.SingleInstance);
            
            IImmutableSet<ISymbol?> GetSetOfTypesWithProperties(IReadOnlyList<INamedTypeSymbol> propertyGivingTypes) => getAllImplementations
                .AllImplementations
                .Where(i =>
                {
                    var derivedTypes = AllDerivedTypes(i);
                    return propertyGivingTypes.Any(t => derivedTypes.Contains(t, SymbolEqualityComparer.Default));
                })
                .ToImmutableHashSet(SymbolEqualityComparer.Default);

            IEnumerable<INamedTypeSymbol> AllDerivedTypes(INamedTypeSymbol type)
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
        }

        public bool ShouldBeManaged(INamedTypeSymbol type) => !_transientTypes.Contains(type);
        public bool ShouldBeSingleInstance(INamedTypeSymbol type) => _singleInstanceTypes.Contains(type);
    }
}