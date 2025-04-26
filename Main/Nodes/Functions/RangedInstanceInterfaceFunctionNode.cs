using MrMeeseeks.DIE.CodeGeneration.Nodes;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceInterfaceFunctionNode : IFunctionNode
{
    void AddConsideredRange(IRangeNode range);
}

internal sealed partial class RangedInstanceInterfaceFunctionNode : ReturningFunctionNodeBase, IRangedInstanceInterfaceFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _type;
    private readonly IContainerNode _parentContainer;
    private readonly List<IRangedInstanceFunctionNode> _implementations = []; 
    
    public RangedInstanceInterfaceFunctionNode(
        // parameters
        INamedTypeSymbol type, 
        IReadOnlyList<ITypeSymbol> parameters,
        
        // dependencies
        IContainerNode parentContainer,
        IRangeNode parentRange,
        IReferenceGenerator referenceGenerator, 
        IOuterFunctionSubDisposalNodeChooser subDisposalNodeChooser,
        IEntryTransientScopeDisposalNodeChooser transientScopeDisposalNodeChooser,
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
            type, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentContainer, 
            parentRange, 
            asynchronicityHandlingFactory.Typed(type, false),
            subDisposalNodeChooser,
            transientScopeDisposalNodeChooser,
            functionNodeGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            asyncFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            typeParameterUtility)
    {
        _type = type;
        _parentContainer = parentContainer;
        ReturnedTypeNameNotWrapped = type.Name;
        NamePrefix = $"GetTransientScopeInstance{_type.Name}";
        NameNumberSuffix = referenceGenerator.Generate("");
    }


    protected override string NamePrefix { get; set; }
    protected override string NameNumberSuffix { get; set; }
    public override string ReturnedTypeNameNotWrapped { get; }

    public void AddConsideredRange(IRangeNode range)
    {
        var implementation = range.BuildTransientScopeFunction(_type, this);
        (implementation as IRangedInstanceFunctionNodeInitializer)?.Initialize(NamePrefix, NameNumberSuffix, _parentContainer.TransientScopeInterface.FullName);
        implementation.RegisterCallingFunction(this);
        _implementations.Add(implementation);
    }
}