using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface IScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceFullName { get; }
    string TransientScopeInterfaceReference { get; }
    string TransientScopeInterfaceParameterReference { get; }
    IScopeCallNode BuildScopeCallFunction(string containerParameter, string transientScopeInterfaceParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction);
}

internal class ScopeNode : RangeNode, IScopeNode
{
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateScopeFunctionNode> _createScopeFunctionNodeFactory;

    internal ScopeNode(
        string name,
        IContainerNode parentContainer,
        ITransientScopeInterfaceNode transientScopeInterfaceNode,
        IScopeManager scopeManager,
        IUserDefinedElements userDefinedElements,
        ICheckTypeProperties checkTypeProperties,
        IReferenceGenerator referenceGenerator,
        Func<ITypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateFunctionNode> createFunctionNodeFactory,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateScopeFunctionNode> createScopeFunctionNodeFactory,
        Func<ScopeLevel, INamedTypeSymbol, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, IRangedInstanceFunctionGroupNode> rangedInstanceFunctionGroupNodeFactory,
        Func<IReferenceGenerator, IDisposalHandlingNode> disposalHandlingNodeFactory)
        : base (
            name, 
            userDefinedElements, 
            checkTypeProperties, 
            referenceGenerator, 
            createFunctionNodeFactory, 
            rangedInstanceFunctionGroupNodeFactory,
            disposalHandlingNodeFactory)
    {
        _createScopeFunctionNodeFactory = createScopeFunctionNodeFactory;
        ParentContainer = parentContainer;
        ScopeManager = scopeManager;
        FullName = $"{parentContainer.Namespace}.{parentContainer.Name}.{name}";
        ContainerFullName = parentContainer.FullName;
        TransientScopeInterfaceFullName = $"{parentContainer.Namespace}.{parentContainer.Name}.{transientScopeInterfaceNode.Name}";
        ContainerReference = referenceGenerator.Generate("_container");
        ContainerParameterReference = referenceGenerator.Generate("container");
        TransientScopeInterfaceReference = referenceGenerator.Generate("_transientScope");
        TransientScopeInterfaceParameterReference = referenceGenerator.Generate("transientScope");
    }

    protected override IScopeManager ScopeManager { get; }
    protected override IContainerNode ParentContainer { get; }
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

    public IScopeCallNode BuildScopeCallFunction(string containerParameter, string transientScopeInterfaceParameter, INamedTypeSymbol type, IRangeNode callingRange, IFunctionNode callingFunction)
    {
        // todo smarter overloads handling
        var createFunction = _createScopeFunctionNodeFactory(
            type,
            callingFunction.Overrides.Select(kvp => kvp.Value.Item1).ToList(),
            this,
            ParentContainer,
            UserDefinedElements,
            CheckTypeProperties,
            ReferenceGenerator).EnqueueTo(ParentContainer.BuildQueue);
        _createFunctions.Add(createFunction);
        
        return createFunction.CreateScopeCall(containerParameter, transientScopeInterfaceParameter, callingRange, callingFunction, this);
    }

    public override string FullName { get; }
    public override DisposalType DisposalType => ParentContainer.DisposalType;
    public string ContainerFullName { get; }
    public override string ContainerReference { get; }
    public string ContainerParameterReference { get; }
    public string TransientScopeInterfaceFullName { get; }
    public string TransientScopeInterfaceReference { get; }
    public string TransientScopeInterfaceParameterReference { get; }
}