using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Interception;
using MrMeeseeks.DIE.Validation.Configuration;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Configuration;

internal interface IInvocationTypeManager
{
    IReadOnlyCollection<IInvocationDescriptionNode> InvocationDescriptionNodes { get; }
    IReadOnlyCollection<ITypeDescriptionNode> TypeDescriptionNodes { get; }
    IReadOnlyCollection<IMethodDescriptionNode> MethodDescriptionNodes { get; }
    
    IInvocationDescriptionNode GetInvocationDescriptionNode(INamedTypeSymbol type);
    ITypeDescriptionNode GetTypeDescriptionNode(INamedTypeSymbol type);
    IMethodDescriptionNode GetMethodDescriptionNode(INamedTypeSymbol type);
    
    IEnumerable<InterceptionDecoratorData> InterceptorBasedDecoratorTypes { get; }
    
    string GetInterceptorBasedDecoratorTypeFullName(INamedTypeSymbol interceptorType, INamedTypeSymbol interfaceType);
}

internal class DelegationImplementationBase
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

internal class InterceptionDecoratorData
{
    private readonly INamedTypeSymbol _interfaceType;
    private readonly INamedTypeSymbol _interceptorType;
    
    internal InterceptionDecoratorData(
        // parameters
        (INamedTypeSymbol InterceptorType, INamedTypeSymbol InterfaceType) types,
        
        // dependencies
        IReferenceGenerator referenceGenerator,
        Func<INamedTypeSymbol, IPropertySymbol, DelegationPropertyImplementation> delegationPropertyImplementationFactory,
        Func<INamedTypeSymbol, IMethodSymbol, DelegationMethodImplementation> delegationMethodImplementationFactory,
        Func<INamedTypeSymbol, IEventSymbol, DelegationEventImplementation> delegationEventImplementationFactory,
        Func<INamedTypeSymbol, IPropertySymbol, DelegationIndexerImplementation> delegationIndexerImplementationFactory)
    {
        _interfaceType = types.InterfaceType;
        _interceptorType = types.InterceptorType;
        Name = referenceGenerator.Generate($"Interceptor_{types.InterceptorType.Name}_{types.InterfaceType.Name}");
        InterfaceFieldReference = referenceGenerator.Generate("_innerInterface");
        InterceptorFieldReference = referenceGenerator.Generate("_interceptor");

        Implementations = _interfaceType
            .AllInterfaces
            .Prepend(_interfaceType)
            .SelectMany(i => i
                .GetMembers()
                .Where(MemberFilter)
                .Select(m => CreateDelegationImplementation(m, i)))
            .ToList();
        return;

        bool MemberFilter(ISymbol member) => member is 
            IPropertySymbol { IsStatic: false } 
            or IMethodSymbol { IsStatic: false, MethodKind: not MethodKind.PropertyGet and not MethodKind.PropertySet and not MethodKind.EventAdd and not MethodKind.EventRemove }
            or IEventSymbol { IsStatic: false };
        
        DelegationImplementationBase CreateDelegationImplementation(ISymbol member, INamedTypeSymbol declaringInterface) => member switch
        {
            IPropertySymbol { IsIndexer: false } property => delegationPropertyImplementationFactory(declaringInterface, property),
            IPropertySymbol { IsIndexer: true } indexer => delegationIndexerImplementationFactory(declaringInterface, indexer),
            IMethodSymbol method => delegationMethodImplementationFactory(declaringInterface, method),
            IEventSymbol @event => delegationEventImplementationFactory(declaringInterface, @event),
            _ => throw new InvalidOperationException()
        };
    }
    internal string Name { get; }
    internal string FullName => $"global::{Constants.NamespaceForGeneratedUtilities}.{Name}";
    internal string InterfaceFullName => _interfaceType.FullName();
    internal string InterceptorFullName => _interceptorType.FullName();
    internal string InterfaceFieldReference { get; }
    internal string InterceptorFieldReference { get; }
    internal IReadOnlyList<DelegationImplementationBase> Implementations { get; }
}

internal sealed class InvocationTypeManager : IInvocationTypeManager, IContainerInstance
{
    private readonly Func<(INamedTypeSymbol InterceptorType, INamedTypeSymbol InterfaceType), InterceptionDecoratorData> _interceptionDecoratorDataFactory;
    private readonly Dictionary<INamedTypeSymbol, IInvocationDescriptionNode> _invocationDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, ITypeDescriptionNode> _typeDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, IMethodDescriptionNode> _methodDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, Dictionary<INamedTypeSymbol, InterceptionDecoratorData>> _interceptorBasedDecoratorTypes = new();
    
    internal InvocationTypeManager(
        IValidateInvocationDescriptionMappingAttributes validateInvocationDescriptionMappingAttributes,
        IValidateTypeDescriptionMappingAttributes validateTypeDescriptionMappingAttributes,
        IValidateMethodDescriptionMappingAttributes validateMethodDescriptionMappingAttributes,
        Compilation compilation,
        WellKnownTypesMapping wellKnownTypesMapping,
        IInterfaceCache interfaceCache,
        Func<INamedTypeSymbol, IInvocationDescriptionNode> invocationDescriptionNodeFactory,
        Func<INamedTypeSymbol, ITypeDescriptionNode> typeDescriptionNodeFactory,
        Func<INamedTypeSymbol, IMethodDescriptionNode> methodDescriptionNodeFactory,
        Func<(INamedTypeSymbol InterceptorType, INamedTypeSymbol InterfaceType), InterceptionDecoratorData> interceptionDecoratorDataFactory)
    {
        _interceptionDecoratorDataFactory = interceptionDecoratorDataFactory;
        var methodDescriptionMappingAttributes = GetMappingAttributeTypes(wellKnownTypesMapping.MethodDescriptionMappingAttribute);
        var typeDescriptionMappingAttributes = GetMappingAttributeTypes(wellKnownTypesMapping.TypeDescriptionMappingAttribute);
        var invocationDescriptionMappingAttributes = GetMappingAttributeTypes(wellKnownTypesMapping.InvocationDescriptionMappingAttribute);
        
        /*foreach (var invocationDescriptionMappingAttribute in invocationDescriptionMappingAttributes)
        {
            validateInvocationDescriptionMappingAttributes.Validate(
                invocationDescriptionMappingAttribute,
                typeDescriptionMappingAttributes,
                methodDescriptionMappingAttributes);
        }
        
        foreach (var typeDescriptionMappingAttribute in typeDescriptionMappingAttributes)
        {
            validateTypeDescriptionMappingAttributes.Validate(
                typeDescriptionMappingAttribute);
        }
        
        foreach (var methodDescriptionMappingAttribute in methodDescriptionMappingAttributes)
        {
            validateMethodDescriptionMappingAttributes.Validate(
                methodDescriptionMappingAttribute,
                typeDescriptionMappingAttributes);
        }*/
        
        _invocationDescriptionNodes = interfaceCache
            .All
            .Where(i => HasMappingAttribute(i, invocationDescriptionMappingAttributes))
            .ToDictionary(nts => nts, invocationDescriptionNodeFactory);
        
        _typeDescriptionNodes = interfaceCache
            .All
            .Where(i => HasMappingAttribute(i, typeDescriptionMappingAttributes))
            .ToDictionary(nts => nts, typeDescriptionNodeFactory);
        
        _methodDescriptionNodes = interfaceCache
            .All
            .Where(i => HasMappingAttribute(i, methodDescriptionMappingAttributes))
            .ToDictionary(nts => nts, methodDescriptionNodeFactory);
        
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
    public IReadOnlyCollection<ITypeDescriptionNode> TypeDescriptionNodes => _typeDescriptionNodes.Values;
    public IReadOnlyCollection<IMethodDescriptionNode> MethodDescriptionNodes => _methodDescriptionNodes.Values;
    public IInvocationDescriptionNode GetInvocationDescriptionNode(INamedTypeSymbol type) => _invocationDescriptionNodes[type];
    public ITypeDescriptionNode GetTypeDescriptionNode(INamedTypeSymbol type) => _typeDescriptionNodes[type];
    public IMethodDescriptionNode GetMethodDescriptionNode(INamedTypeSymbol type) => _methodDescriptionNodes[type];
    public IEnumerable<InterceptionDecoratorData> InterceptorBasedDecoratorTypes => 
        _interceptorBasedDecoratorTypes.Values.SelectMany(d => d.Values);

    public string GetInterceptorBasedDecoratorTypeFullName(INamedTypeSymbol interceptorType, INamedTypeSymbol interfaceType)
    {
        if (_interceptorBasedDecoratorTypes.TryGetValue(interceptorType, out var interfaceTypeToDecorator))
        {
            if (interfaceTypeToDecorator.TryGetValue(interfaceType, out var decoratorData))
                return decoratorData.FullName;
            decoratorData = _interceptionDecoratorDataFactory((interceptorType, interfaceType));
            interfaceTypeToDecorator.Add(interfaceType, decoratorData);
            return decoratorData.FullName;
        }
        
        var newDecoratorData = _interceptionDecoratorDataFactory((interceptorType, interfaceType));
        _interceptorBasedDecoratorTypes.Add(interceptorType, new Dictionary<INamedTypeSymbol, InterceptionDecoratorData> {{interfaceType, newDecoratorData}});
        return newDecoratorData.FullName;
    }
}