using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ILocalFunctionNode : ISingleFunctionNode
{
}

internal class LocalFunctionNode : SingleFunctionNodeBase, ILocalFunctionNode
{
    private readonly IElementNodeMapperBase _mapper;

    public LocalFunctionNode(
        ITypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)> closureParameters, 
        IRangeNode parentNode, 
        IContainerNode parentContainer, 
        IUserDefinedElements userDefinedElements, 
        ICheckTypeProperties checkTypeProperties, 
        IElementNodeMapperBase mapper,
        IReferenceGenerator referenceGenerator, 
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        WellKnownTypes wellKnownTypes) 
        : base(
            null,
            typeSymbol, 
            parameters, 
            closureParameters,
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
        _mapper = mapper;
        Name = referenceGenerator.Generate("Local", typeSymbol);
    }

    protected override IElementNodeMapperBase GetMapper(ISingleFunctionNode parentFunction, IRangeNode parentNode, IContainerNode parentContainer,
        IUserDefinedElements userDefinedElements, ICheckTypeProperties checkTypeProperties) =>
        _mapper;

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitLocalFunctionNode(this);
    public override string Name { get; protected set; }
}