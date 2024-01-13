using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Mappers;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateTransientScopeFunctionNode : ICreateFunctionNodeBase
{
}

internal partial class CreateTransientScopeFunctionNode : SingleFunctionNodeBase, ICreateTransientScopeFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ITransientScopeDisposalElementNodeMapper> _transientScopeDisposalElementNodeMapperFactory;

    public CreateTransientScopeFunctionNode(
        // parameters
        INamedTypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        ITransientScopeWideContext transientScopeWideContext,
        IContainerNode parentContainer, 
        IReferenceGenerator referenceGenerator, 
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, ITransientScopeDisposalElementNodeMapper> transientScopeDisposalElementNodeMapperFactory,
        Func<ITypeSymbol, string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ITypeSymbol, (string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<ITypeSymbol, string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        ITypeParameterUtility typeParameterUtility,
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
            typeParameterUtility,
            containerWideContext)
    {
        _typeSymbol = typeSymbol;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _transientScopeDisposalElementNodeMapperFactory = transientScopeDisposalElementNodeMapperFactory;
        Name = referenceGenerator.Generate("Create", _typeSymbol);
    }

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) => 
        mapper.MapToImplementation(
            new(false, false, false), 
            null, 
            _typeSymbol, 
            new(ImmutableStack<INamedTypeSymbol>.Empty, null));

    protected override IElementNodeMapperBase GetMapper()
    {
        var parentMapper = _typeToElementNodeMapperFactory();
        return _transientScopeDisposalElementNodeMapperFactory(parentMapper);
    }

    public override string Name { get; protected set; }
}