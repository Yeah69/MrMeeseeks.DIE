using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateTransientScopeFunctionNode : ICreateFunctionNodeBase;

internal sealed partial class CreateTransientScopeFunctionNode : SingleFunctionNodeBase, ICreateTransientScopeFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _typeSymbol;
    private readonly Func<IElementNodeMapper> _typeToElementNodeMapperFactory;
    private readonly Func<IElementNodeMapperBase, ITransientScopeDisposalElementNodeMapper> _transientScopeDisposalElementNodeMapperFactory;

    public CreateTransientScopeFunctionNode(
        // parameters
        INamedTypeSymbol typeSymbol, 
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer, 
        IReferenceGenerator referenceGenerator, 
        IOuterFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IInnerTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        AsynchronicityHandlingFactory asynchronicityHandlingFactory,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<IElementNodeMapper> typeToElementNodeMapperFactory,
        Func<IElementNodeMapperBase, ITransientScopeDisposalElementNodeMapper> transientScopeDisposalElementNodeMapperFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        ITypeParameterUtility typeParameterUtility) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal,
            typeSymbol, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
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
        _typeSymbol = typeSymbol;
        _typeToElementNodeMapperFactory = typeToElementNodeMapperFactory;
        _transientScopeDisposalElementNodeMapperFactory = transientScopeDisposalElementNodeMapperFactory;
        NamePrefix = $"CreateTransientScope{typeSymbol.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
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


    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }
}