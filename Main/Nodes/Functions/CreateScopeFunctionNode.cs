using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateScopeFunctionNode : ICreateFunctionNodeBase
{
}

internal class CreateScopeFunctionNode : SingleFunctionNodeBase, ICreateScopeFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;

    public CreateScopeFunctionNode(
        INamedTypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        ITransientScopeWideContext transientScopeWideContext,
        IContainerNode parentContainer, 
        IReferenceGenerator referenceGenerator, 
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        IContainerWideContext containerWideContext) 
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
        _typeSymbol = typeSymbol;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        Name = referenceGenerator.Generate("CreateScope", typeSymbol);
    }

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) => 
        mapper.MapToImplementation(new(false, false, false), _typeSymbol, ImmutableStack<INamedTypeSymbol>.Empty);

    protected override IElementNodeMapperBase GetMapper() =>
        _typeToElementNodeMapperFactory();

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitCreateFunctionNode(this);
    public override string Name { get; protected set; }
}