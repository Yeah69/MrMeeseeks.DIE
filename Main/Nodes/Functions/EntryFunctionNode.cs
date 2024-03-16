using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IEntryFunctionNode : ISingleFunctionNode;

internal sealed partial class EntryFunctionNode : SingleFunctionNodeBase, IEntryFunctionNode, IScopeInstance
{
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> _nonWrapToCreateElementNodeMapperFactory;

    internal EntryFunctionNode(
        // parameters
        ITypeSymbol typeSymbol, 
        string prefix, 
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer, 
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory,
        Func<ITypeSymbol, string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ITypeSymbol, (string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IFunctionCallNode?, ScopeCallNodeOuterMapperParam, IScopeCallNode> scopeCallNodeFactory,
        Func<ITypeSymbol, string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IFunctionCallNode?, ScopeCallNodeOuterMapperParam, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal,
            typeSymbol, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentRange, 
            parentContainer, 
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
        Name = prefix;
    }

    protected override IElementNodeMapperBase GetMapper()
    {
        var dummyMapper = _typeToElementNodeMapperFactory();

        return _nonWrapToCreateElementNodeMapperFactory(dummyMapper);
    }

    public override string Name { get; protected set; }
}