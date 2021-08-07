using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MrMeeseeks.DIE
{
    public interface ITypeToImplementationsMapper
    {
        IList<INamedTypeSymbol> Map(INamedTypeSymbol typeSymbol); 
    }

    class TypeToImplementationsMapper : ITypeToImplementationsMapper
    {
        private readonly Dictionary<INamedTypeSymbol, List<INamedTypeSymbol>> map;

        public TypeToImplementationsMapper(
            IGetAllImplementations getAllImplementations) => 
            map = getAllImplementations
                .AllImplementations
                .SelectMany(i =>
                {
                    return i.AllInterfaces.Select(ii => (ii, i)).Prepend((i, i));
                })
                .GroupBy(t => t.Item1, t => t.Item2)
                .ToDictionary(g => g.Key, g => g.ToList());

        public IList<INamedTypeSymbol> Map(INamedTypeSymbol typeSymbol)
        {
            if(map.TryGetValue(typeSymbol, out var implementations))
            {
                return implementations;
            }

            return new List<INamedTypeSymbol>();
        }
    }
}
