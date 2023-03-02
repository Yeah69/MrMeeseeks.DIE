using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ITransientScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceName { get; }
    string TransientScopeDisposalReference { get; }
    ITransientScopeCallNode BuildTransientScopeCallFunction(string containerParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction);
}

internal class TransientScopeNode : ScopeNodeBase, ITransientScopeNode, ITransientScopeInstance
{
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateTransientScopeFunctionNodeRoot> _createTransientScopeFunctionNodeFactory;

    internal TransientScopeNode(
        IScopeInfo scopeInfo,
        IContainerNode parentContainer,
        IScopeManager scopeManager,
        IUserDefinedElementsBase userDefinedElements,
        IScopeCheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateFunctionNodeRoot> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IMultiFunctionNodeRoot> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, ICreateTransientScopeFunctionNodeRoot> createTransientScopeFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReadOnlyList<IInitializedInstanceNode>, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IReferenceGenerator, IVoidFunctionNodeRoot> voidFunctionNodeFactory, 
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory,
        Func<INamedTypeSymbol, IReferenceGenerator, IInitializedInstanceNode> initializedInstanceNodeFactory)
        : base (
            scopeInfo, 
            parentContainer,
            scopeManager,
            userDefinedElements, 
            checkTypeProperties, 
            referenceGenerator, 
            wellKnownTypesMiscellaneous,
            createFunctionNodeFactory, 
            multiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            voidFunctionNodeFactory,
            disposalHandlingNodeFactory,
            initializedInstanceNodeFactory)
    {
        _createTransientScopeFunctionNodeFactory = createTransientScopeFunctionNodeFactory;
        TransientScopeInterfaceName = parentContainer.TransientScopeInterface.Name;
        TransientScopeDisposalReference = parentContainer.TransientScopeDisposalReference;
    }
    protected override string ContainerParameterForScope => ContainerReference;

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitTransientScopeNode(this);

    public override IFunctionCallNode BuildContainerInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction)
    {
        return ParentContainer.BuildContainerInstanceCall(ContainerReference, type, callingFunction);
    }

    public override IFunctionCallNode BuildTransientScopeInstanceCall(INamedTypeSymbol type, IFunctionNode callingFunction) => 
        ParentContainer.TransientScopeInterface.BuildTransientScopeInstanceCall($"({Constants.ThisKeyword} as {ParentContainer.TransientScopeInterface.FullName})", type, callingFunction);

    public string TransientScopeInterfaceName { get; }
    public string TransientScopeDisposalReference { get; }

    public ITransientScopeCallNode BuildTransientScopeCallFunction(string containerParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _createFunctions,
            () => _createTransientScopeFunctionNodeFactory(
                type,
                callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                .Function
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateTransientScopeCall(containerParameter, callingRange, callingFunction, this));
}