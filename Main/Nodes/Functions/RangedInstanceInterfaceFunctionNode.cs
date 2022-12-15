using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceInterfaceFunctionNode : IFunctionNode
{
    void AddConsideredRange(IRangeNode range);
}

internal class RangedInstanceInterfaceFunctionNode : FunctionNodeBase, IRangedInstanceInterfaceFunctionNode
{
    private readonly INamedTypeSymbol _type;
    private readonly IContainerNode _parentContainer;
    private readonly List<IRangedInstanceFunctionNode> _implementations = new(); 
    
    public RangedInstanceInterfaceFunctionNode(
        INamedTypeSymbol type, 
        IReadOnlyList<ITypeSymbol> parameters,
        IContainerNode parentContainer, 
        IReferenceGenerator referenceGenerator, 
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<string, string, IScopeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, IScopeCallNode> scopeCallNodeFactory, 
        Func<string, ITransientScopeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IReferenceGenerator, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IReferenceGenerator, IParameterNode> parameterNodeFactory,
        WellKnownTypes wellKnownTypes) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            type, 
            parameters,
            ImmutableSortedDictionary<TypeKey, (ITypeSymbol, IParameterNode)>.Empty, 
            parentContainer, 
            referenceGenerator, 
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            wellKnownTypes)
    {
        _type = type;
        _parentContainer = parentContainer;
        Name = referenceGenerator.Generate("GetTransientScopeInstance", _type);
    }

    public override void Build()
    {
        
    }

    public override void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitRangedInstanceInterfaceFunctionNode(this);
    public override string Name { get; protected set; }
    
    public void AddConsideredRange(IRangeNode range)
    {
        var implementation = range.BuildTransientScopeFunction(_type, this);
        (implementation as IRangedInstanceFunctionNodeInitializer)?.Initialize(Name, _parentContainer.TransientScopeInterface.FullName);
        _implementations.Add(implementation);
    }

    protected override void OnBecameAsync()
    {
        base.OnBecameAsync();
        foreach (var implementation in _implementations)
            implementation.ForceToAsync();
    }
}