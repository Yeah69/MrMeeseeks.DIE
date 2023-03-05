using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateTransientScopeFunctionNode : ICreateFunctionNodeBase
{
}

internal class CreateTransientScopeFunctionNode : SingleFunctionNodeBase, ICreateTransientScopeFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, ITransientScopeDisposalElementNodeMapper> _transientScopeDisposalElementNodeMapperFactory;

    public CreateTransientScopeFunctionNode(
        INamedTypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentNode, 
        IContainerNode parentContainer, 
        IUserDefinedElementsBase userDefinedElements, 
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator, 
        Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElementsBase, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, ITransientScopeDisposalElementNodeMapper> transientScopeDisposalElementNodeMapperFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory, 
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        IContainerWideContext containerWideContext) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal,
            typeSymbol, 
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
        _typeSymbol = typeSymbol;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _transientScopeDisposalElementNodeMapperFactory = transientScopeDisposalElementNodeMapperFactory;
        Name = referenceGenerator.Generate("Create", typeSymbol);
    }

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) => 
        mapper.MapToImplementation(new(false, false, false), _typeSymbol, ImmutableStack<INamedTypeSymbol>.Empty);

    protected override IElementNodeMapperBase GetMapper(ISingleFunctionNode parentFunction, IRangeNode parentNode, IContainerNode parentContainer,
        IUserDefinedElementsBase userDefinedElements, ICheckTypeProperties checkTypeProperties)
    {
        var parentMapper = _typeToElementNodeMapperFactory(parentFunction, parentNode, parentContainer, userDefinedElements,
            checkTypeProperties, ReferenceGenerator);
        return _transientScopeDisposalElementNodeMapperFactory(parentMapper, parentMapper.MapperDependencies);
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitCreateFunctionNode(this);
    public override string Name { get; protected set; }
}