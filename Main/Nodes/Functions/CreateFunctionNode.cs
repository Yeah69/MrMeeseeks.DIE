using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateFunctionNodeBase : ISingleFunctionNode
{
}

internal interface ICreateFunctionNode : ICreateFunctionNodeBase
{
}

internal class CreateFunctionNode : SingleFunctionNodeBase, ICreateFunctionNode, IScopeInstance
{
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;

    public CreateFunctionNode(
        ITypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        ITransientScopeWideContext transientScopeWideContext,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator, 
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        IContainerWideContext containerWideContext) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            typeSymbol, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            transientScopeWideContext.Range, 
            parentContainer, 
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        Name = referenceGenerator.Generate("Create", typeSymbol);
    }

    protected override IElementNodeMapperBase GetMapper() =>
        _typeToElementNodeMapperFactory();

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitCreateFunctionNode(this);
    public override string Name { get; protected set; }
}