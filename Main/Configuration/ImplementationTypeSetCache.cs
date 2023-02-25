using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal interface IImplementationTypeSetCache
{
    IImmutableSet<INamedTypeSymbol> All { get; }

    IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly);
}

internal class ImplementationTypeSetCache : IImplementationTypeSetCache, IContainerInstance
{
    private readonly GeneratorExecutionContext _context;
    private readonly WellKnownTypes _wellKnownTypes;
    private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _all;
    private IImmutableDictionary<IAssemblySymbol, IImmutableSet<INamedTypeSymbol>> _assemblyCache =
        ImmutableDictionary<IAssemblySymbol, IImmutableSet<INamedTypeSymbol>>.Empty;

    private readonly string _currentAssemblyName;

    internal ImplementationTypeSetCache(
        GeneratorExecutionContext context,
        WellKnownTypes wellKnownTypes)
    {
        _context = context;
        _wellKnownTypes = wellKnownTypes;
        _currentAssemblyName = context.Compilation.AssemblyName ?? "";
        _all = new Lazy<IImmutableSet<INamedTypeSymbol>>(
            () => context
                .Compilation
                .SourceModule
                .ReferencedAssemblySymbols
                .Prepend(_context.Compilation.Assembly)
                .SelectMany(ForAssembly)
                .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default));
    }

    public IImmutableSet<INamedTypeSymbol> All => _all.Value;
    public IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly)
    {
        if (_assemblyCache.TryGetValue(assembly, out var set)) return set;

        var freshSet = GetImplementationsFrom(assembly);
        _assemblyCache = _assemblyCache.Add(assembly, freshSet);
        return freshSet;
    }

    private IImmutableSet<INamedTypeSymbol> GetImplementationsFrom(IAssemblySymbol assemblySymbol)
    {
        var internalsAreVisible = 
            CustomSymbolEqualityComparer.Default.Equals(_context.Compilation.Assembly, assemblySymbol) 
            ||assemblySymbol
                .GetAttributes()
                .Any(ad =>
                    CustomSymbolEqualityComparer.Default.Equals(ad.AttributeClass, _wellKnownTypes.InternalsVisibleToAttribute)
                    && ad.ConstructorArguments.Length == 1
                    && ad.ConstructorArguments[0].Value is string assemblyName
                    && Equals(assemblyName, _currentAssemblyName));
                
        return GetAllNamespaces(assemblySymbol.GlobalNamespace)
            .SelectMany(ns => ns.GetTypeMembers())
            .SelectMany(t => t.AllNestedTypesAndSelf())
            .Where(nts => nts is
            {
                IsAbstract: false,
                IsStatic: false,
                IsImplicitClass: false,
                IsScriptClass: false,
                TypeKind: TypeKind.Class or TypeKind.Struct or TypeKind.Structure,
                DeclaredAccessibility: Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
            })
            .Where(nts => 
                !nts.Name.StartsWith("<") 
                && (nts.IsAccessiblePublicly() 
                    || internalsAreVisible && nts.IsAccessibleInternally()))
            .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
    }

    private static IEnumerable<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol root)
    {
        yield return root;
        foreach(var child in root.GetNamespaceMembers())
        foreach(var next in GetAllNamespaces(child))
            yield return next;
    }
}