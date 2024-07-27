using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Configuration;

internal interface IImplementationCache
{
    IImmutableSet<INamedTypeSymbol> All { get; }

    IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly);
}

internal sealed class ImplementationCache : IImplementationCache, IContainerInstance
{
    private readonly INamedTypeCache _namedTypeCache;
    private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _all;

    internal ImplementationCache(
        INamedTypeCache namedTypeCache)
    {
        _namedTypeCache = namedTypeCache;
        _all = new Lazy<IImmutableSet<INamedTypeSymbol>>(
            () => namedTypeCache
                .All
                .Where(IsImplementation)
                .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default));
    }

    public IImmutableSet<INamedTypeSymbol> All => _all.Value;
    public IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly) => 
        _namedTypeCache.ForAssembly(assembly)
            .Where(IsImplementation)
            .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
    
    private static bool IsImplementation(INamedTypeSymbol nts) => 
        nts is
        {
            IsAbstract: false,
            IsStatic: false,
            IsImplicitClass: false,
            IsScriptClass: false,
            TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Structure
        } && !nts.Name.StartsWith("<", StringComparison.Ordinal); // Filter framework-generated types
}