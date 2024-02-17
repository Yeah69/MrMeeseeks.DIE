namespace MrMeeseeks.DIE.Nodes;

internal sealed record PassedContext(
    ImmutableStack<INamedTypeSymbol> ImplementationStack,
    InjectionKey? InjectionKeyModification);
    
internal sealed record InjectionKey(ITypeSymbol Type, object Value);