using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Interception;
using MrMeeseeks.DIE.Validation.Configuration;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration.Interception;

internal interface IInvocationTypeManager
{
    IReadOnlyCollection<IInvocationDescriptionNode> InvocationDescriptionNodes { get; }
    
    IInvocationDescriptionNode? GetInvocationDescriptionNode(INamedTypeSymbol type);
    
    IEnumerable<InterceptionDecoratorData> InterceptorBasedDecoratorTypes { get; }
    
    string GetInterceptorBasedDecoratorTypeFullName(INamedTypeSymbol interceptorType, INamedTypeSymbol interfaceType);
}

internal interface IInterceptorDecoratorMemberImplementation;

internal class InterceptionDecoratorData
{
    private readonly INamedTypeSymbol _interfaceType;
    private readonly INamedTypeSymbol _interceptorType;
    
    internal InterceptionDecoratorData(
        // parameters
        (INamedTypeSymbol InterceptorType, INamedTypeSymbol InterfaceType) types,
        IReadOnlyList<IInterceptorDecoratorMemberImplementation> implementations,
        
        // dependencies
        IReferenceGenerator referenceGenerator)
    {
        _interfaceType = types.InterfaceType;
        _interceptorType = types.InterceptorType;
        Name = referenceGenerator.Generate($"Interceptor_{types.InterceptorType.Name}_{types.InterfaceType.Name}");
        InterfaceFieldReference = referenceGenerator.Generate("_innerInterface");
        InterceptorFieldReference = referenceGenerator.Generate("_interceptor");
        Implementations = implementations;
    }
    internal string Name { get; }
    internal string FullName => $"global::{Constants.NamespaceForGeneratedUtilities}.{Name}";
    internal string InterfaceFullName => _interfaceType.FullName();
    internal string InterceptorFullName => _interceptorType.FullName();
    internal string InterfaceFieldReference { get; }
    internal string InterceptorFieldReference { get; }
    internal IReadOnlyList<IInterceptorDecoratorMemberImplementation> Implementations { get; }
}

internal class InterceptionDecoratorDataBuilder
{
    private record MethodAndInvocation(IMethodSymbol Method, IInvocationDescriptionNode Invocation);
    
    private readonly INamedTypeSymbol _interceptorType;
    private readonly Func<(INamedTypeSymbol InterceptorType, INamedTypeSymbol InterfaceType), IReadOnlyList<IInterceptorDecoratorMemberImplementation>, InterceptionDecoratorData> _interceptionDecoratorDataFactory;
    private readonly Func<INamedTypeSymbol, IPropertySymbol, DelegationPropertyImplementation> _delegationPropertyImplementationFactory;
    private readonly Func<INamedTypeSymbol, IMethodSymbol, DelegationMethodImplementation> _delegationMethodImplementationFactory;
    private readonly Func<INamedTypeSymbol, IEventSymbol, DelegationEventImplementation> _delegationEventImplementationFactory;
    private readonly Func<INamedTypeSymbol, IPropertySymbol, DelegationIndexerImplementation> _delegationIndexerImplementationFactory;
    private readonly MethodAndInvocation? _interceptMethodAndInvocation;

    internal InterceptionDecoratorDataBuilder(
        // parameters
        INamedTypeSymbol interceptorType,
        
        // dependencies
        InvocationTypeManager invocationTypeManager,
        Func<(INamedTypeSymbol InterceptorType, INamedTypeSymbol InterfaceType), IReadOnlyList<IInterceptorDecoratorMemberImplementation>, InterceptionDecoratorData> interceptionDecoratorDataFactory,
        Func<INamedTypeSymbol, IPropertySymbol, DelegationPropertyImplementation> delegationPropertyImplementationFactory,
        Func<INamedTypeSymbol, IMethodSymbol, DelegationMethodImplementation> delegationMethodImplementationFactory,
        Func<INamedTypeSymbol, IEventSymbol, DelegationEventImplementation> delegationEventImplementationFactory,
        Func<INamedTypeSymbol, IPropertySymbol, DelegationIndexerImplementation> delegationIndexerImplementationFactory)
    {
        _interceptorType = interceptorType;
        _interceptionDecoratorDataFactory = interceptionDecoratorDataFactory;
        _delegationPropertyImplementationFactory = delegationPropertyImplementationFactory;
        _delegationMethodImplementationFactory = delegationMethodImplementationFactory;
        _delegationEventImplementationFactory = delegationEventImplementationFactory;
        _delegationIndexerImplementationFactory = delegationIndexerImplementationFactory;

        _interceptMethodAndInvocation = interceptorType
            .GetMembers("Intercept")
            .Where(m => m is IMethodSymbol
            {
                Parameters: [{ Type: INamedTypeSymbol { TypeKind: TypeKind.Interface } }]
            })
            .OfType<IMethodSymbol>()
            .Select(m =>
            {
                if (m.Parameters[0].Type is INamedTypeSymbol invocationType
                    && invocationTypeManager.GetInvocationDescriptionNode(invocationType) is
                        { } invocationDescriptionNode)
                    return new MethodAndInvocation(m, invocationDescriptionNode);
                return null;
            })
            .FirstOrDefault();
    }

    internal InterceptionDecoratorData Build(INamedTypeSymbol interfaceType)
    {
        var implementations = interfaceType
            .AllInterfaces
            .Prepend(interfaceType)
            .SelectMany(i => i
                .GetMembers()
                .Where(MemberFilter)
                .Select(m => _interceptMethodAndInvocation is not null 
                    ? (IInterceptorDecoratorMemberImplementation) CreateSyncImplementation(m, i) 
                    : CreateDelegationImplementation(m, i)))
            .ToList();
        
        return _interceptionDecoratorDataFactory((_interceptorType, interfaceType), implementations);

        bool MemberFilter(ISymbol member) => member is 
            IPropertySymbol { IsStatic: false } 
            or IMethodSymbol { IsStatic: false, MethodKind: not MethodKind.PropertyGet and not MethodKind.PropertySet and not MethodKind.EventAdd and not MethodKind.EventRemove }
            or IEventSymbol { IsStatic: false };
        
        DelegationImplementationBase CreateDelegationImplementation(ISymbol member, INamedTypeSymbol declaringInterface) => member switch
        {
            IPropertySymbol { IsIndexer: false } property => _delegationPropertyImplementationFactory(declaringInterface, property),
            IPropertySymbol { IsIndexer: true } indexer => _delegationIndexerImplementationFactory(declaringInterface, indexer),
            IMethodSymbol method => _delegationMethodImplementationFactory(declaringInterface, method),
            IEventSymbol @event => _delegationEventImplementationFactory(declaringInterface, @event),
            _ => throw new InvalidOperationException()
        };
        
        SyncImplementationBase CreateSyncImplementation(ISymbol member, INamedTypeSymbol declaringInterface) => member switch
        {
            IPropertySymbol { IsIndexer: false } property => new SyncPropertyImplementation(declaringInterface, _interceptMethodAndInvocation.Method, property),
            IPropertySymbol { IsIndexer: true } indexer => new SyncIndexerImplementation(declaringInterface, _interceptMethodAndInvocation.Method, indexer),
            IMethodSymbol method => new SyncMethodImplementation(declaringInterface, _interceptMethodAndInvocation.Method, method),
            IEventSymbol @event => new SyncEventImplementation(declaringInterface, _interceptMethodAndInvocation.Method, @event),
            _ => throw new InvalidOperationException()
        };
    }
}

internal sealed class InvocationTypeManager : IInvocationTypeManager, IContainerInstance
{
    private readonly Func<INamedTypeSymbol, InterceptionDecoratorDataBuilder> _interceptionDecoratorDataBuilderFactory;
    private readonly Dictionary<INamedTypeSymbol, IInvocationDescriptionNode> _invocationDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, InterceptionDecoratorData>> _interceptorBasedDecoratorTypes = new();
    private readonly Dictionary<INamedTypeSymbol, InterceptionDecoratorDataBuilder> _interceptorBasedDecoratorDataBuilders = new();
    
    internal InvocationTypeManager(
        IValidateInvocationDescriptionMappingAttributes validateInvocationDescriptionMappingAttributes,
        Compilation compilation,
        WellKnownTypesMapping wellKnownTypesMapping,
        IInterfaceCache interfaceCache,
        Func<INamedTypeSymbol, IInvocationDescriptionNode> invocationDescriptionNodeFactory,
        Func<INamedTypeSymbol, InterceptionDecoratorDataBuilder> interceptionDecoratorDataBuilderFactory)
    {
        _interceptionDecoratorDataBuilderFactory = interceptionDecoratorDataBuilderFactory;
        var invocationDescriptionMappingAttributes = GetMappingAttributeTypes(wellKnownTypesMapping.InvocationDescriptionMappingAttribute);
        
        /*foreach (var invocationDescriptionMappingAttribute in invocationDescriptionMappingAttributes)
        {
            validateInvocationDescriptionMappingAttributes.Validate(invocationDescriptionMappingAttribute);
        }*/
        
        _invocationDescriptionNodes = interfaceCache
            .All
            .Where(i => HasMappingAttribute(i, invocationDescriptionMappingAttributes))
            .ToDictionary(nts => nts, invocationDescriptionNodeFactory);
        
        return;
        
        bool HasMappingAttribute(INamedTypeSymbol type, ImmutableArray<INamedTypeSymbol> mappingAttributeTypes) =>
            type.GetAttributes()
                .Any(attribute =>
                    attribute.AttributeClass is { } attributeClass 
                    && mappingAttributeTypes.Contains(attributeClass, CustomSymbolEqualityComparer.Default));

        ImmutableArray<INamedTypeSymbol> GetMappingAttributeTypes(INamedTypeSymbol descriptionMappingAttributeType) =>
            compilation.Assembly.GetAttributes()
                .Where(attribute =>
                    attribute.AttributeClass is { } attributeClass
                    && CustomSymbolEqualityComparer.Default.Equals(attributeClass, descriptionMappingAttributeType))
                .Select(attribute =>
                {
                    if (attribute.ConstructorArguments.Length != 1
                        || attribute.ConstructorArguments[0].Kind != TypedConstantKind.Type
                        || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
                        return null;
                    return type;
                })
                .OfType<INamedTypeSymbol>()
                .ToImmutableArray();
    }

    public IReadOnlyCollection<IInvocationDescriptionNode> InvocationDescriptionNodes => _invocationDescriptionNodes.Values;
    public IInvocationDescriptionNode? GetInvocationDescriptionNode(INamedTypeSymbol type) => 
        _invocationDescriptionNodes.TryGetValue(type, out var node) ? node : null;
    public IEnumerable<InterceptionDecoratorData> InterceptorBasedDecoratorTypes => 
        _interceptorBasedDecoratorTypes.Values.SelectMany(d => d.Values);

    public string GetInterceptorBasedDecoratorTypeFullName(INamedTypeSymbol interceptorType, INamedTypeSymbol interfaceType)
    {
        if (_interceptorBasedDecoratorTypes.TryGetValue(interceptorType, out var interfaceTypeToDecorator))
        {
            if (interfaceTypeToDecorator.TryGetValue(interfaceType, out var decoratorData))
                return decoratorData.FullName;
            
            if (!_interceptorBasedDecoratorDataBuilders.TryGetValue(interceptorType, out var builder))
            {
                builder = _interceptionDecoratorDataBuilderFactory(interceptorType);
                _interceptorBasedDecoratorDataBuilders.Add(interceptorType, builder);
            }
            
            decoratorData = builder.Build(interfaceType);
            interfaceTypeToDecorator.Add(interfaceType, decoratorData);
            return decoratorData.FullName;
        }
        
        var newBuilder = _interceptionDecoratorDataBuilderFactory(interceptorType);
        _interceptorBasedDecoratorDataBuilders.Add(interceptorType, newBuilder);
        var newDecoratorData = newBuilder.Build(interfaceType);
        _interceptorBasedDecoratorTypes.Add(interceptorType, new Dictionary<INamedTypeSymbol, InterceptionDecoratorData> {{interfaceType, newDecoratorData}});
        return newDecoratorData.FullName;
    }
}