using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MrMeeseeks.DIE
{
    public interface ICheckDisposalManagement
    {
        bool ShouldBeManaged(INamedTypeSymbol type);
    }

    internal class CheckDisposalManagement : ICheckDisposalManagement
    {
        private readonly ImmutableHashSet<ISymbol?> _transientTypes;

        public CheckDisposalManagement(
            IGetAllImplementations getAllImplementations,
            ITypesFromAttributes typesFromAttributes)
        {
            _transientTypes = getAllImplementations
                .AllImplementations
                .Where(i =>
                {
                    var derivedTypes = AllDerivedTypes(i);
                    return typesFromAttributes.Transient.Any(t => derivedTypes.Contains(t, SymbolEqualityComparer.Default));
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
    }
}