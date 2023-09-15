namespace MrMeeseeks.DIE.Nodes;

public record PassedContext(
    ImmutableStack<INamedTypeSymbol> ImplementationStack);