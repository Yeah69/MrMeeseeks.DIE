using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceFunctionNode : ISingleFunctionNode
{
}

internal interface IRangedInstanceFunctionNodeInitializer
{
    /// <summary>
    /// Only intended for transient scope instance function, cause they need to synchronize identifier with their interface function
    /// </summary>
    void Initialize(string name, string explicitInterfaceFullName);
}

internal class RangedInstanceFunctionNode : SingleFunctionNodeBase, IRangedInstanceFunctionNode, IRangedInstanceFunctionNodeInitializer, IScopeInstance
{
    private readonly INamedTypeSymbol _type;
    private readonly Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapper> _typeToElementNodeMapperFactory;

    public RangedInstanceFunctionNode(
        ScopeLevel level,
        INamedTypeSymbol type, 
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentNode, 
        IContainerNode parentContainer, 
        IUserDefinedElementsBase userDefinedElements, 
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator, 
        Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory, 
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        IContainerWideContext containerWideContext) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            type, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentNode, 
            parentContainer, 
            userDefinedElements, 
            checkTypeProperties,
            referenceGenerator, 
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _type = type;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        Name = referenceGenerator.Generate($"Get{level.ToString()}Instance", _type);
    }

    protected override IElementNodeMapperBase GetMapper(ISingleFunctionNode parentFunction, IRangeNode parentNode, IContainerNode parentContainer,
        IUserDefinedElementsBase userDefinedElements, ICheckTypeProperties checkTypeProperties) =>
        _typeToElementNodeMapperFactory(parentFunction, parentNode, parentContainer, userDefinedElements, checkTypeProperties, ReferenceGenerator);

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) => 
        // "MapToImplementation" instead of "Map", because latter would cause an infinite recursion ever trying to create a new ranged instance function
        mapper.MapToImplementation(new(true, false, false), _type, ImmutableStack<INamedTypeSymbol>.Empty); 

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitRangedInstanceFunctionNode(this);
    public override string Name { get; protected set; }

    void IRangedInstanceFunctionNodeInitializer.Initialize(
        string name, 
        string explicitInterfaceFullName)
    {
        Name = name;
        ExplicitInterfaceFullName = explicitInterfaceFullName;
    }
}