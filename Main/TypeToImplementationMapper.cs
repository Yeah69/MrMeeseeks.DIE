using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MrMeeseeks.DIE
{
    internal interface ITypeToImplementationsMapper
    {
        IList<INamedTypeSymbol> Map(INamedTypeSymbol typeSymbol); 
    }

    internal class TypeToImplementationsMapper : ITypeToImplementationsMapper
    {
        private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> _map;

        public TypeToImplementationsMapper(
            WellKnownTypes wellKnownTypes,
            IDiagLogger diagLogger,
            IGetAllImplementations getAllImplementations,
            IGetAssemblyAttributes getAssemblyAttributes)
        {
            
            _map = getAllImplementations
                .AllImplementations
                .Concat(GetSpiedImplementations())
                .SelectMany(i => { return i.AllInterfaces.Select(ii => (ii, i)).Prepend((i, i)); })
                .GroupBy(t => t.Item1, t => t.Item2)
                .ToDictionary(g => g.Key, g => g.ToList());

            IEnumerable<INamedTypeSymbol> GetSpiedImplementations() => getAssemblyAttributes
                .AllAssemblyAttributes
                .Where(ad =>
                    ad.AttributeClass?.Equals(wellKnownTypes.SpyAttribute, SymbolEqualityComparer.Default) ?? false)
                .SelectMany(ad =>
                {
                    var countConstructorArguments = ad.ConstructorArguments.Length;
                    if (countConstructorArguments is not 1)
                    {
                        // Invalid code, ignore
                        return ImmutableArray.Create<TypedConstant>();
                    }

                    var typeConstant = ad.ConstructorArguments[0];
                    if (typeConstant.Kind != TypedConstantKind.Array)
                    {
                        // Invalid code, ignore
                        return ImmutableArray.Create<TypedConstant>();
                    }

                    return typeConstant.Values;
                })
                .Select(tc =>
                {
                    if (!CheckValidType(tc, out var type))
                    {
                        return null;
                    }

                    return type;
                })
                .Where(t => t is not null)
                .OfType<INamedTypeSymbol>()
                .SelectMany(t => t.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Select(p => p.Type)
                    .OfType<INamedTypeSymbol>());

            bool CheckValidType(TypedConstant typedConstant, out INamedTypeSymbol type)
            {
                type = (typedConstant.Value as INamedTypeSymbol)!;
                if (typedConstant.Value is null)
                    return false;
                if (type.IsOrReferencesErrorType())
                    // we will report an error for this case anyway.
                    return false;
                if (type.IsUnboundGenericType)
                    return false;
                if (!type.IsAccessibleInternally())
                    return false;

                return true;
            }
        }

        public IList<INamedTypeSymbol> Map(INamedTypeSymbol typeSymbol)
        {
            if(_map.TryGetValue(typeSymbol, out var implementations))
            {
                return implementations;
            }

            return new List<INamedTypeSymbol>();
        }
    }
}
