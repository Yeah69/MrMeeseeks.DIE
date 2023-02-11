using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ITransientScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceName { get; }
    string TransientScopeDisposalReference { get; }
    ITransientScopeCallNode BuildTransientScopeCallFunction(string containerParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction);
}

internal class TransientScopeNode : RangeNode, ITransientScopeNode
{
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateTransientScopeFunctionNode> _createTransientScopeFunctionNodeFactory;

    internal TransientScopeNode(
        string name,
        IContainerNode parentContainer,
        IScopeManager scopeManager,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IMultiFunctionNode> multiFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateTransientScopeFunctionNode> createTransientScopeFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory)
        : base (
            name, 
            userDefinedElements, 
            checkTypeProperties, 
            referenceGenerator, 
            createFunctionNodeFactory, 
            multiFunctionNodeFactory,
            rangedInstanceFunctionGroupNodeFactory,
            disposalHandlingNodeFactory)
    {
        _createTransientScopeFunctionNodeFactory = createTransientScopeFunctionNodeFactory;
        ParentContainer = parentContainer;
        ScopeManager = scopeManager;
        FullName = $"{parentContainer.Namespace}.{parentContainer.Name}.{name}";
        ContainerFullName = parentContainer.FullName;
        ContainerReference = referenceGenerator.Generate("_container");
        ContainerParameterReference = referenceGenerator.Generate("container");
        TransientScopeInterfaceName = parentContainer.TransientScopeInterface.Name;
        TransientScopeDisposalReference = parentContainer.TransientScopeDisposalReference;
    }

    protected override IScopeManager ScopeManager { get; }
    protected override IContainerNode ParentContainer { get; }
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
                callingFunction.Overrides.Select(kvp => kvp.Value.Item1).ToList(),
                this,
                ParentContainer,
                UserDefinedElements,
                CheckTypeProperties,
                ReferenceGenerator)
                .EnqueueBuildJobTo(ParentContainer.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty),
            f => f.CreateTransientScopeCall(containerParameter, callingRange, callingFunction, this));

    public override string FullName { get; }
    public override DisposalType DisposalType => ParentContainer.DisposalType;
    public string ContainerFullName { get; }
    public override string ContainerReference { get; }
    public string ContainerParameterReference { get; }
}