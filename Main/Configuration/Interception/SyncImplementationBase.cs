namespace MrMeeseeks.DIE.Configuration.Interception;

internal abstract record SyncImplementationBase(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod)
    : IInterceptorDecoratorMemberImplementation;

internal sealed record SyncPropertyImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IPropertySymbol Property)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);

internal sealed record SyncMethodImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IMethodSymbol Method)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);

internal sealed record SyncEventImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IEventSymbol Event)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);

internal sealed record SyncIndexerImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IPropertySymbol Indexer)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);