namespace MrMeeseeks.DIE.Extensions;

internal static class CompilationExtension
{
    internal static INamedTypeSymbol GetTypeByMetadataNameOrThrow(this Compilation compilation, string metadataName) =>
        compilation.GetTypeByMetadataName(metadataName) 
        ?? throw new ArgumentException("Type not found by metadata name.", nameof(metadataName));
}