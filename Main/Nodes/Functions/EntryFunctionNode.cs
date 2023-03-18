using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IEntryFunctionNode : ISingleFunctionNode
{
}

internal class EntryFunctionNode : SingleFunctionNodeBase, IEntryFunctionNode, IScopeInstance
{
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> _nonWrapToCreateElementNodeMapperFactory;

    public EntryFunctionNode(
        ITypeSymbol typeSymbol, 
        string prefix, 
        IReadOnlyList<ITypeSymbol> parameters,
        ITransientScopeWideContext transientScopeWideContext,
        IContainerNode parentContainer, 
        IContainerWideContext containerWideContext,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory, 
        Func<IElementNodeMapperBase, INonWrapToCreateElementNodeMapper> nonWrapToCreateElementNodeMapperFactory,
        Func<string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal,
            typeSymbol, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            transientScopeWideContext.Range, 
            parentContainer, 
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
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

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitEntryFunctionNode(this);
    public override string Name { get; protected set; }
}