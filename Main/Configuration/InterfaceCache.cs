using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Configuration;

internal interface IInterfaceCache
{
    IImmutableSet<INamedTypeSymbol> All { get; }
}

internal sealed class InterfaceCache : IInterfaceCache, IContainerInstance
{
    private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _all;

    internal InterfaceCache(
        INamedTypeCache namedTypeCache)
    {
        _all = new Lazy<IImmutableSet<INamedTypeSymbol>>(
            () => namedTypeCache
                .All
                .Where(nts => nts is
                {
                    IsStatic: false,
                    IsImplicitClass: false,
                    IsScriptClass: false,
                    TypeKind: TypeKind.Interface
                })
                .Where(nts => !nts.Name.StartsWith("<", StringComparison.Ordinal)) // Filter framework-generated types
                .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default));
    }

    public IImmutableSet<INamedTypeSymbol> All => _all.Value;
}