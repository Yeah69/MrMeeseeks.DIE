using MrMeeseeks.DIE.Contexts;
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
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        Func<ITypeSymbol, string?, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<ITypeSymbol, string?, SynchronicityDecision, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IWrappedAsyncFunctionCallNode> asyncFunctionCallNodeFactory,
        Func<ITypeSymbol, (string, string), IScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<ITypeSymbol, string, ITransientScopeNode, IRangeNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReadOnlyList<ITypeSymbol>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        ITypeParameterUtility typeParameterUtility,
        IContainerWideContext containerWideContext)
        : base(
            accessibility,
            parameters,
            closureParameters,
            parentContainer,
            parentRange,
            
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        TypeSymbol = typeSymbol;
        ReturnedTypeFullName = TypeSymbol.FullName();
        TypeParameters = typeParameterUtility.ExtractTypeParameters(typeSymbol);
    }

    protected override void AdjustToAsync()
    {
        var symbol = TypeSymbol is INamedTypeSymbol namedTypeSymbol
            ? namedTypeSymbol.OriginalDefinitionIfUnbound()
            : TypeSymbol;
        if (WellKnownTypes.ValueTask1 is not null)
        {
            SynchronicityDecision = SynchronicityDecision.AsyncValueTask;
            ReturnedTypeFullName =  WellKnownTypes.ValueTask1.Construct(symbol).FullName();
        }
        else
        {
            SynchronicityDecision = SynchronicityDecision.AsyncTask;
            ReturnedTypeFullName =  WellKnownTypes.Task1.Construct(symbol).FullName();
        }
    }

    public override bool CheckIfReturnedType(ITypeSymbol type) => 
        CustomSymbolEqualityComparer.IncludeNullability.Equals(type, TypeSymbol);

    public override IReadOnlyList<ITypeParameterSymbol> TypeParameters { get; }
}