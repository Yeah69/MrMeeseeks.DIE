namespace MrMeeseeks.DIE.Nodes;

internal sealed record PassedContext(
    ImmutableStack<INamedTypeSymbol> ImplementationStack,
    InjectionKey? InjectionKeyModification)
{
    internal static PassedContext Empty => new(ImmutableStack<INamedTypeSymbol>.Empty, null);
}
    
internal sealed record InjectionKey(ITypeSymbol Type, object Value);