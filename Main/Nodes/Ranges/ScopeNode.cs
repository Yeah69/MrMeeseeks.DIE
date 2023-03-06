using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceFullName { get; }
    string TransientScopeInterfaceReference { get; }
    string TransientScopeInterfaceParameterReference { get; }
    IScopeCallNode BuildScopeCallFunction(string containerParameter, string transientScopeInterfaceParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction);
}

internal class ScopeNode : ScopeNodeBase, IScopeNode, ITransientScopeInstance
{
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateScopeFunctionNodeRoot> _createScopeFunctionNodeFactory;

    internal ScopeNode(
        IScopeInfo scopeInfo,
        IContainerNode parentContainer,
        ITransientScopeInterfaceNode transientScopeInterfaceNode,
        IScopeManager scopeManager,
        IUserDefinedElementsBase userDefinedElements,
        IReferenceGenerator referenceGenerator,
        IContainerWideContext containerWideContext,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateScopeFunctionNodeRoot> createScopeFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IRangeNode, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base(
            scopeInfo, 
            parentContainer,
            scopeManager,
            userDefinedElements, 
            referenceGenerator, 
            containerWideContext,
            createFunctionNodeFactory,  
            multiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            voidFunctionNodeFactory,
            disposalHandlingNodeFactory,
            initializedInstanceNodeFactory)
    {
        _createScopeFunctionNodeFactory = createScopeFunctionNodeFactory;
        TransientScopeInterfaceFullName = $"{parentContainer.Namespace}.{parentContainer.Name}.{transientScopeInterfaceNode.Name}";
        TransientScopeInterfaceReference = referenceGenerator.Generate("_transientScope");
        TransientScopeInterfaceParameterReference = referenceGenerator.Generate("transientScope");
    }
    protected override string ContainerParameterForScope => ContainerReference;
    protected override string TransientScopeInterfaceParameterForScope => TransientScopeInterfaceReference;

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitScopeNode(this);

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ParentContainer.BuildContainerInstanceCall(ContainerReference, type, callingFunction);

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) =>
        ParentContainer.TransientScopeInterface.BuildTransientScopeInstanceCall(
            TransientScopeInterfaceReference, 
            type,
            callingFunction);

    public IScopeCallNode BuildScopeCallFunction(string containerParameter, string transientScopeInterfaceParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _createFunctions,
            () => _createScopeFunctionNodeFactory(
                type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateScopeCall(containerParameter, transientScopeInterfaceParameter, callingRange, callingFunction, this));

    public string TransientScopeInterfaceFullName { get; }
    public string TransientScopeInterfaceReference { get; }
    public string TransientScopeInterfaceParameterReference { get; }
}