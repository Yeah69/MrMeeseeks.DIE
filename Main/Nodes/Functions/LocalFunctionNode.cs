using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ILocalFunctionNode : ISingleFunctionNode;

internal sealed partial class LocalFunctionNode : SingleFunctionNodeBase, ILocalFunctionNode, IScopeInstance
{
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> _nonWrapToCreateElementNodeMapperFactory;

    public LocalFunctionNode(
        // parameters
        ITypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters, 
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer, 
        IReferenceGenerator referenceGenerator, 
        IOuterFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IEntryTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory,
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes) 
        : base(
            null,
            typeSymbol, 
            parameters, 
            closureParameters,
            parentRange, 
            parentContainer, 
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeParameterUtility,
            wellKnownTypes)
    {
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _nonWrapToCreateElementNodeMapperFactory = nonWrapToCreateElementNodeMapperFactory;
        Name = referenceGenerator.Generate("Local", typeSymbol);
    }

    protected override IElementNodeMapperBase GetMapper()
    {
        var baseMapper = _typeToElementNodeMapperFactory();
        return _nonWrapToCreateElementNodeMapperFactory(baseMapper);
    }

    public override string Name { get; protected set; }
}