namespace MrMeeseeks.DIE.Configuration.Interception;

internal abstract record SyncImplementationBase(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod)
    : IInterceptorDecoratorMemberImplementation;

internal record SyncPropertyImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IPropertySymbol Property)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);

internal record SyncMethodImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IMethodSymbol Method)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);

internal record SyncEventImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IEventSymbol Event)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);

internal record SyncIndexerImplementation(INamedTypeSymbol DeclaringInterface, IMethodSymbol InterceptMethod, IPropertySymbol Indexer)
    : SyncImplementationBase(DeclaringInterface, InterceptMethod);