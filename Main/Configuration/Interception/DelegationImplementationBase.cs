using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration.Interception;

internal class DelegationImplementationBase : IInterceptorDecoratorMemberImplementation
{
    internal DelegationImplementationBase(INamedTypeSymbol declaringInterfaceType)
    {
        DeclaringInterfaceFullName = declaringInterfaceType.FullName();
    }
    
    internal string DeclaringInterfaceFullName { get; }
}

internal class DelegationPropertyImplementation : DelegationImplementationBase
{
    private readonly IPropertySymbol _property;

    internal DelegationPropertyImplementation(
        INamedTypeSymbol declaringInterfaceType,
        IPropertySymbol property)
        : base(declaringInterfaceType)
    {
        _property = property;
    }
    
    internal string Name => _property.Name;
    internal string TypeFullName => _property.Type.FullName();
    internal bool HasGetter => _property.GetMethod is not null;
    internal bool HasSetter => _property.SetMethod is not null && !_property.SetMethod.IsInitOnly;
}

internal class DelegationMethodImplementation : DelegationImplementationBase
{
    private readonly IMethodSymbol _method;

    internal DelegationMethodImplementation(
        INamedTypeSymbol declaringInterfaceType,
        IMethodSymbol method)
        : base(declaringInterfaceType)
    {
        _method = method;
    }
    
    internal string Name => _method.Name;
    internal string TypeFullName => _method.ReturnsVoid ? "void" : _method.ReturnType.FullName();
    internal ImmutableArray<ITypeParameterSymbol> TypeParameters => _method.TypeParameters;
    internal bool ReturnsVoid => _method.ReturnsVoid;
    internal IReadOnlyList<string> GenericTypeParameters => _method.TypeParameters.Select(p => p.FullName()).ToList();
    internal IReadOnlyList<(string TypeFullName, string Name)> Parameters => _method.Parameters.Select(p => (p.Type.FullName(), p.Name)).ToList();
}

internal class DelegationEventImplementation : DelegationImplementationBase
{
    private readonly IEventSymbol _event;

    internal DelegationEventImplementation(
        INamedTypeSymbol declaringInterfaceType,
        IEventSymbol @event)
        : base(declaringInterfaceType)
    {
        _event = @event;
    }
    
    internal string Name => _event.Name;
    internal string TypeFullName => _event.Type.FullName();
}

internal class DelegationIndexerImplementation : DelegationImplementationBase
{
    private readonly IPropertySymbol _indexer;

    internal DelegationIndexerImplementation(
        INamedTypeSymbol declaringInterfaceType,
        IPropertySymbol indexer)
        : base(declaringInterfaceType)
    {
        _indexer = indexer;
    }
    
    internal string TypeFullName => _indexer.Type.FullName();
    internal bool HasGetter => _indexer.GetMethod is not null;
    internal bool HasSetter => _indexer.SetMethod is not null;
    internal IReadOnlyList<(string TypeFullName, string Name)> Parameters => _indexer.Parameters.Select(p => (p.Type.FullName(), p.Name)).ToList();
}