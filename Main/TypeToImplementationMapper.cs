using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MrMeeseeks.DIE
{
    internal interface ITypeToImplementationsMapper
    {
        IList<ITypeSymbol> Map(ITypeSymbol typeSymbol); 
    }

    internal class TypeToImplementationsMapper : ITypeToImplementationsMapper
    {
        private readonly Dictionary<ITypeSymbol, List<ITypeSymbol>> _map;

        public TypeToImplementationsMapper(
            WellKnownTypes wellKnownTypes,
            IGetAllImplementations getAllImplementations,
            IGetAssemblyAttributes getAssemblyAttributes)
        {
            
            _map = getAllImplementations
                .AllImplementations
                .Concat(GetSpiedImplementations())
                .SelectMany(i => { return i.AllInterfaces.OfType<ITypeSymbol>().Select(ii => (ii, i)).Prepend((i, i)); })
                .GroupBy(t => t.Item1, t => t.Item2)
                .ToDictionary(g => g.Key, g => g.Distinct().ToList());

            IEnumerable<ITypeSymbol> GetSpiedImplementations() => getAssemblyAttributes
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
                .SelectMany(t => t?.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(ms => !ms.ReturnsVoid)
                    .Select(ms => ms.ReturnType)
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

        public IList<ITypeSymbol> Map(ITypeSymbol typeSymbol)
        {
            if(_map.TryGetValue(typeSymbol, out var implementations))
            {
                return implementations;
            }

            return new List<ITypeSymbol>();
        }
    }
}
