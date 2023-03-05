using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal abstract class ReturningFunctionNodeBase : FunctionNodeBase
{
    protected readonly ITypeSymbol TypeSymbol;

    public ReturningFunctionNodeBase(
        // parameters
        Accessibility? accessibility,
        ITypeSymbol typeSymbol,
        IReadOnlyList<ITypeSymbol> parameters,
        ImmutableDictionary<ITypeSymbol, IParameterNode> closureParameters,
        IContainerNode parentContainer,
        IRangeNode parentRange,
        IReferenceGenerator referenceGenerator,
        
        // dependencies
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        IContainerWideContext containerWideContext)
        : base(
            accessibility,
            parameters,
            closureParameters,
            parentContainer,
            parentRange,
            referenceGenerator,
            
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        TypeSymbol = typeSymbol;
        ReturnedTypeFullName = typeSymbol.FullName();
    }

    protected override string GetAsyncTypeFullName() => TypeSymbol.FullName();

    protected override string GetReturnedTypeFullName() => WellKnownTypes.ValueTask1.Construct(TypeSymbol).FullName();

    public override bool CheckIfReturnedType(ITypeSymbol type) => 
        CustomSymbolEqualityComparer.IncludeNullability.Equals(type, TypeSymbol);
}