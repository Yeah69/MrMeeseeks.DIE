using MrMeeseeks.DIE.Configuration;
using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ITransientScopeNode : IScopeNodeBase
{
    string TransientScopeInterfaceName { get; }
    ITransientScopeCallNode BuildTransientScopeCallFunction(string containerParameter, INamedTypeSymbol type, IFunctionNode callingFunction);
}

internal class TransientScopeNode : RangeNode, ITransientScopeNode
{
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangeNode, IContainerNode, IUserDefinedElements, ICheckTypeProperties, IReferenceGenerator, ICreateScopeFunctionNode> _createScopeFunctionNodeFactory;

    internal TransientScopeNode(
        string name,
        IContainerNode parentContainer,
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
        ContainerReference = referenceGenerator.Generate("_container");
        ContainerParameterReference = referenceGenerator.Generate("container");
        TransientScopeInterfaceName = parentContainer.TransientScopeInterface.Name;
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

    public ITransientScopeCallNode BuildTransientScopeCallFunction(string containerParameter, INamedTypeSymbol type, IFunctionNode callingFunction)
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
        
        return createFunction.CreateTransientScopeCall(containerParameter, callingFunction, this);
    }

    public override string FullName { get; }
    public override DisposalType DisposalType => ParentContainer.DisposalType;
    public string ContainerFullName { get; }
    public string ContainerReference { get; }
    public string ContainerParameterReference { get; }
}