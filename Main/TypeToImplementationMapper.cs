using Microsoft.CodeAnalysis;
using System.Collections.Generic;
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
            IGetAllImplementations getAllImplementations) => 
            _map = getAllImplementations
                .AllImplementations
                .SelectMany(i =>
                {
                    return i.AllInterfaces.Select(ii => (ii, i)).Prepend((i, i));
                })
                .GroupBy(t => t.Item1, t => t.Item2)
                .ToDictionary(g => g.Key, g => g.ToList());

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
