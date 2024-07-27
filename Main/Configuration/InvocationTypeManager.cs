using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Descriptions;
using MrMeeseeks.DIE.Validation.Configuration;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Configuration;

internal interface IInvocationTypeManager
{
    IReadOnlyCollection<IInvocationDescriptionNode> InvocationDescriptionNodes { get; }
    IReadOnlyCollection<ITypeDescriptionNode> TypeDescriptionNodes { get; }
    IReadOnlyCollection<IMethodDescriptionNode> MethodDescriptionNodes { get; }
    
    IInvocationDescriptionNode GetInvocationDescriptionNode(INamedTypeSymbol type);
    ITypeDescriptionNode GetTypeDescriptionNode(INamedTypeSymbol type);
    IMethodDescriptionNode GetMethodDescriptionNode(INamedTypeSymbol type);
}

internal sealed class InvocationTypeManager : IInvocationTypeManager, IContainerInstance
{
    private readonly Dictionary<INamedTypeSymbol, IInvocationDescriptionNode> _invocationDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, ITypeDescriptionNode> _typeDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, IMethodDescriptionNode> _methodDescriptionNodes;
    
    internal InvocationTypeManager(
        IValidateInvocationDescriptionMappingAttributes validateInvocationDescriptionMappingAttributes,
        IValidateTypeDescriptionMappingAttributes validateTypeDescriptionMappingAttributes,
        IValidateMethodDescriptionMappingAttributes validateMethodDescriptionMappingAttributes,
        Compilation compilation,
        WellKnownTypesMapping wellKnownTypesMapping,
        IInterfaceCache interfaceCache,
        Func<INamedTypeSymbol, IInvocationDescriptionNode> invocationDescriptionNodeFactory,
        Func<INamedTypeSymbol, ITypeDescriptionNode> typeDescriptionNodeFactory,
        Func<INamedTypeSymbol, IMethodDescriptionNode> methodDescriptionNodeFactory)
    {
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
}