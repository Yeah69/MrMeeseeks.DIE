using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IVoidFunctionNode : IFunctionNode
{
    IReadOnlyList<(IFunctionCallNode, IInitializedInstanceNode)> Initializations { get; }
}

internal class VoidFunctionNode : FunctionNodeBase, IVoidFunctionNode, IScopeInstance
{
    private readonly IReadOnlyList<IInitializedInstanceNode> _initializedInstanceNodes;
    private readonly IRangeNode _parentRange;

    internal VoidFunctionNode(
        // parameters
        IReadOnlyList<IInitializedInstanceNode> initializedInstanceNodes,
        IReadOnlyList<ITypeSymbol> parameters,
        IRangeNode parentRange,
        IContainerNode parentContainer,
        IReferenceGenerator referenceGenerator,
        
        // dependencies
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IContainerNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        IContainerWideContext containerWideContext)
        : base(
            Microsoft.CodeAnalysis.Accessibility.Internal, 
            parameters, 
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentContainer, 
            parentRange,
            referenceGenerator,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _initializedInstanceNodes = initializedInstanceNodes;
        _parentRange = parentRange;
        ReturnedTypeFullName = "void";
        Name = referenceGenerator.Generate("Initialize");
    }
    
    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
        Initializations = _initializedInstanceNodes
            .Select(i => (i.BuildCall(_parentRange, this), i))
            .ToList();
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitVoidFunctionNode(this);
    protected override string GetAsyncTypeFullName() => "void";

    protected override string GetReturnedTypeFullName() =>
        SynchronicityDecision == SynchronicityDecision.AsyncValueTask 
            ? WellKnownTypes.ValueTask.FullName()
            : WellKnownTypes.Task.FullName();

    public override bool CheckIfReturnedType(ITypeSymbol type) => false;

    public override string Name { get; protected set; }

    public IReadOnlyList<(IFunctionCallNode, IInitializedInstanceNode)> Initializations { get; private set; } =
        Array.Empty<(IFunctionCallNode, IInitializedInstanceNode)>();
}