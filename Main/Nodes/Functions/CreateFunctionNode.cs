using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Mappers;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface ICreateFunctionNodeBase : ISingleFunctionNode;

internal interface ICreateFunctionNode : ICreateFunctionNodeBase;

internal sealed partial class CreateFunctionNode : SingleFunctionNodeBase, ICreateFunctionNode, IScopeInstance
{
    private readonly MapperData _mapperData;
    private readonly ImplementationMappingConfiguration? _implementationMappingConfiguration;
    private readonly IMapperFactory _mapperFactory;

    internal CreateFunctionNode(
        // parameters
        MapperData mapperData,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImplementationMappingConfiguration? implementationMappingConfiguration,
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator, 
        IMapperFactory mapperFactory,
        IInnerFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IInnerTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        AsynchronicityHandlingFactory asynchronicityHandlingFactory,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        ITypeParameterUtility typeParameterUtility) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
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
        _mapperData = mapperData;
        _implementationMappingConfiguration = implementationMappingConfiguration;
        _mapperFactory = mapperFactory;
        NamePrefix = $"Create{typeSymbol.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
    }

    protected override IElementNode MapToReturnedElement(IElementNodeMapperBase mapper) =>
        _implementationMappingConfiguration is not null && TypeSymbol is INamedTypeSymbol namedTypeSymbol
            ? mapper.MapToImplementation(
                _implementationMappingConfiguration,
                null,
                namedTypeSymbol,
                new PassedContext(ImmutableStack<INamedTypeSymbol>.Empty, null))
            : base.MapToReturnedElement(mapper);

    protected override IElementNodeMapperBase GetMapper() => _mapperFactory.Create(_mapperData);

    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }
}