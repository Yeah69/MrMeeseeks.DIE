using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateTransientScopeFunctionNode : ICreateFunctionNode
{
}

internal class CreateTransientScopeFunctionNode : SingleFunctionNodeBase, ICreateTransientScopeFunctionNode
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapperBase> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, ITransientScopeDisposalElementNodeMapper> _transientScopeDisposalElementNodeMapperFactory;

    public CreateTransientScopeFunctionNode(
        INamedTypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentNode, 
        IContainerNode parentContainer, 
        IUserDefinedElements userDefinedElements, 
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator, 
        Func<ISingleFunctionNode, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IElementNodeMapperBase> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, ElementNodeMapperBase.PassedDependencies, ITransientScopeDisposalElementNodeMapper> transientScopeDisposalElementNodeMapperFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory, 
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        WellKnownTypes wellKnownTypes) 
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
            wellKnownTypes)
    {
        _typeSymbol = typeSymbol;
        _referenceGenerator = referenceGenerator;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _transientScopeDisposalElementNodeMapperFactory = transientScopeDisposalElementNodeMapperFactory;
        Name = referenceGenerator.Generate("Create", typeSymbol);
    }

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) => 
        mapper.MapToImplementation(new(false, false), _typeSymbol, ImmutableStack<INamedTypeSymbol>.Empty);

    protected override IElementNodeMapperBase GetMapper(ISingleFunctionNode parentFunction, IRangeNode parentNode, IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements, ICheckTypeProperties checkTypeProperties)
    {
        var parentMapper = _typeToElementNodeMapperFactory(parentFunction, parentNode, parentContainer, userDefinedElements,
            checkTypeProperties, _referenceGenerator);
        return _transientScopeDisposalElementNodeMapperFactory(parentMapper, parentMapper.MapperDependencies);
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitCreateFunctionNode(this);
    public override string Name { get; protected set; }
}