using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Descriptions;
using MrMeeseeks.DIE.Validation.Configuration;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Configuration;

internal interface IInvocationTypeManager
{
    IEnumerable<IInvocationDescriptionNode> InvocationDescriptionNodes { get; }
    IEnumerable<ITypeDescriptionNode> TypeDescriptionNodes { get; }
    IEnumerable<IMethodDescriptionNode> MethodDescriptionNodes { get; }
    
    IInvocationDescriptionNode GetInvocationDescriptionNode(INamedTypeSymbol type);
    ITypeDescriptionNode GetTypeDescriptionNode(INamedTypeSymbol type);
    IMethodDescriptionNode GetMethodDescriptionNode(INamedTypeSymbol type);
}

internal class InvocationTypeManager : IInvocationTypeManager, IContainerInstance
{
    private readonly Dictionary<INamedTypeSymbol, IInvocationDescriptionNode> _invocationDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, ITypeDescriptionNode> _typeDescriptionNodes;
    private readonly Dictionary<INamedTypeSymbol, IMethodDescriptionNode> _methodDescriptionNodes;
    
    internal InvocationTypeManager(
        IContainerWideContext containerWideContext,
        IValidateInvocationDescriptionMappingAttributes validateInvocationDescriptionMappingAttributes,
        IValidateTypeDescriptionMappingAttributes validateTypeDescriptionMappingAttributes,
        IValidateMethodDescriptionMappingAttributes validateMethodDescriptionMappingAttributes,
        Compilation compilation,
        Func<INamedTypeSymbol, IInvocationDescriptionNode> invocationDescriptionNodeFactory,
        Func<INamedTypeSymbol, ITypeDescriptionNode> typeDescriptionNodeFactory,
        Func<INamedTypeSymbol, IMethodDescriptionNode> methodDescriptionNodeFactory)
    {
        var wellKnownTypesMapping = containerWideContext.WellKnownTypesMapping;
        
        var methodDescriptionMappingAttributes = GetAttributes(wellKnownTypesMapping.MethodDescriptionMappingAttribute);
        var typeDescriptionMappingAttributes = GetAttributes(wellKnownTypesMapping.TypeDescriptionMappingAttribute);
        var invocationDescriptionMappingAttributes = GetAttributes(wellKnownTypesMapping.InvocationDescriptionMappingAttribute);
        
        foreach (var invocationDescriptionMappingAttribute in invocationDescriptionMappingAttributes)
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
        }
        
        _invocationDescriptionNodes = invocationDescriptionMappingAttributes
            .Select(attribute => attribute.ConstructorArguments[0].Value)
            .OfType<INamedTypeSymbol>()
            .ToDictionary(nts => nts, invocationDescriptionNodeFactory);
        
        _typeDescriptionNodes = typeDescriptionMappingAttributes
            .Select(attribute => attribute.ConstructorArguments[0].Value)
            .OfType<INamedTypeSymbol>()
            .ToDictionary(nts => nts, typeDescriptionNodeFactory);
        
        _methodDescriptionNodes = methodDescriptionMappingAttributes
            .Select(attribute => attribute.ConstructorArguments[0].Value)
            .OfType<INamedTypeSymbol>()
            .ToDictionary(nts => nts, methodDescriptionNodeFactory);
        
        return;

        ImmutableArray<AttributeData> GetAttributes(INamedTypeSymbol descriptionMappingAttributeType) =>
            compilation.Assembly.GetAttributes()
                .Where(attribute =>
                    attribute.AttributeClass is { } attributeClass
                    && CustomSymbolEqualityComparer.Default.Equals(attributeClass, descriptionMappingAttributeType))
                .ToImmutableArray();
    }

    public IEnumerable<IInvocationDescriptionNode> InvocationDescriptionNodes => _invocationDescriptionNodes.Values;
    public IEnumerable<ITypeDescriptionNode> TypeDescriptionNodes => _typeDescriptionNodes.Values;
    public IEnumerable<IMethodDescriptionNode> MethodDescriptionNodes => _methodDescriptionNodes.Values;
    public IInvocationDescriptionNode GetInvocationDescriptionNode(INamedTypeSymbol type) => _invocationDescriptionNodes[type];
    public ITypeDescriptionNode GetTypeDescriptionNode(INamedTypeSymbol type) => _typeDescriptionNodes[type];
    public IMethodDescriptionNode GetMethodDescriptionNode(INamedTypeSymbol type) => _methodDescriptionNodes[type];
}