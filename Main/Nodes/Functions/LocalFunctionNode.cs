using MrMeeseeks.DIE.CodeGeneration.Nodes;
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
        AsynchronicityHandlingFactory asynchronicityHandlingFactory,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory,
        ITypeParameterUtility typeParameterUtility) 
        : base(
            null,
            typeSymbol, 
            parameters, 
            closureParameters,
            parentRange, 
            parentContainer, 
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            asynchronicityHandlingFactory,
            functionNodeGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeParameterUtility)
    {
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _nonWrapToCreateElementNodeMapperFactory = nonWrapToCreateElementNodeMapperFactory;
        NamePrefix = $"Local{typeSymbol.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
    }

    protected override IElementNodeMapperBase GetMapper()
    {
        var baseMapper = _typeToElementNodeMapperFactory();
        return _nonWrapToCreateElementNodeMapperFactory(baseMapper);
    }

    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }
}