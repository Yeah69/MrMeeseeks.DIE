using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IReturningFunctionNode : IFunctionNode;

internal abstract class ReturningFunctionNodeBase : FunctionNodeBase, IReturningFunctionNode
{
    protected readonly ITypeSymbol TypeSymbol;

    protected ReturningFunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        
        // dependencies
        ISubDisposalNodeChooser subDisposalNodeChooser,
        ITransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
        Lazy<IFunctionNodeGenerator> functionNodeGenerator,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<PlainFunctionCallNode.Params, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<WrappedAsyncFunctionCallNode.Params, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ScopeCallNode.Params, IScopeCallNode> scopeCallNodeFactory,
        Func<TransientScopeCallNode.Params, ITransientScopeCallNode> transientScopeCallNodeFactory,
        ITypeParameterUtility typeParameterUtility,
        WellKnownTypes wellKnownTypes)
        : base(
            accessibility,
            parameters,
            closureParameters,
            parentContainer,
            parentRange,
            
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            functionNodeGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            wellKnownTypes)
    {
        TypeSymbol = typeSymbol;
        ReturnedType = TypeSymbol;
        TypeParameters = typeParameterUtility.ExtractTypeParameters(typeSymbol);

        ReturnTypeStatus = ReturnTypeStatus.Ordinary;
        AsyncAwaitStatus = AsyncAwaitStatus.No;
        
        if (wellKnownTypes.ValueTask1 is not null && CustomSymbolEqualityComparer.IncludeNullability.Equals(typeSymbol.OriginalDefinition, wellKnownTypes.ValueTask1))
        {
            ReturnTypeStatus = ReturnTypeStatus.ValueTask;
            AsyncAwaitStatus = AsyncAwaitStatus.Yes;
        }
        else if (CustomSymbolEqualityComparer.IncludeNullability.Equals(typeSymbol.OriginalDefinition, wellKnownTypes.Task1))
        {
            ReturnTypeStatus = ReturnTypeStatus.Task;
            AsyncAwaitStatus = AsyncAwaitStatus.Yes;
        }
    }

    public override bool CheckIfReturnedType(ITypeSymbol type) => 
        CustomSymbolEqualityComparer.IncludeNullability.Equals(type, TypeSymbol);

    public override IReadOnlyList<ITypeParameterSymbol> TypeParameters { get; }

    public override ReturnTypeStatus ReturnTypeStatus { get; protected set; }
    public override AsyncAwaitStatus AsyncAwaitStatus { get; protected set; }
}