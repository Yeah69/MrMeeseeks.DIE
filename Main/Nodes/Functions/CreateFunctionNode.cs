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
    private readonly IMapperFactory _mapperFactory;

    internal CreateFunctionNode(
        // parameters
        MapperData mapperData,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator, 
        IMapperFactory mapperFactory,
        IInnerFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IInnerTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            typeSymbol, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
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
        _mapperData = mapperData;
        _mapperFactory = mapperFactory;
        Name = referenceGenerator.Generate("Create", typeSymbol);
    }

    protected override IElementNodeMapperBase GetMapper() => _mapperFactory.Create(_mapperData);
    
    public override string Name { get; protected set; }
}