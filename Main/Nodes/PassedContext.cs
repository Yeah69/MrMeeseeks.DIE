namespace MrMeeseeks.DIE.Nodes;

internal record PassedContext(
    ImmutableStack<INamedTypeSymbol> ImplementationStack,
    InjectionKey? InjectionKeyModification);
    
internal record InjectionKey(ITypeSymbol Type, object Value);