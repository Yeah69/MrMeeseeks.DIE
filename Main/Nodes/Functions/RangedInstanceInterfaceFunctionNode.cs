using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Ranges;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Functions;

internal interface IRangedInstanceInterfaceFunctionNode : IFunctionNode
{
    void AddConsideredRange(IRangeNode range);
}

internal class RangedInstanceInterfaceFunctionNode : ReturningFunctionNodeBase, IRangedInstanceInterfaceFunctionNode, IScopeInstance
{
    private readonly INamedTypeSymbol _type;
    private readonly IContainerNode _parentContainer;
    private readonly List<IRangedInstanceFunctionNode> _implementations = new(); 
    
    public RangedInstanceInterfaceFunctionNode(
        INamedTypeSymbol type, 
        IReadOnlyList<ITypeSymbol> parameters,
        IContainerNode parentContainer,
        ITransientScopeWideContext transientScopeWideContext,
        IReferenceGenerator referenceGenerator, 
        Func<string?, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IPlainFunctionCallNode> plainFunctionCallNodeFactory,
        Func<(string, string), IScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, IScopeCallNode> scopeCallNodeFactory,
        Func<string, ITransientScopeNode, IRangeNode, IFunctionNode, IReadOnlyList<(IParameterNode, IParameterNode)>, IFunctionCallNode?, ITransientScopeCallNode> transientScopeCallNodeFactory,
        Func<ITypeSymbol, IParameterNode> parameterNodeFactory,
        IContainerWideContext containerWideContext) 
        : base(
            Microsoft.CodeAnalysis.Accessibility.Private,
            type, 
            parameters,
            ImmutableDictionary.Create<ITypeSymbol, IParameterNode>(CustomSymbolEqualityComparer.IncludeNullability), 
            parentContainer, 
            transientScopeWideContext.Range,
            parameterNodeFactory,
            plainFunctionCallNodeFactory,
            scopeCallNodeFactory,
            transientScopeCallNodeFactory,
            containerWideContext)
    {
        _type = type;
        _parentContainer = parentContainer;
        Name = referenceGenerator.Generate("GetTransientScopeInstance", _type);
    }

    public override void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
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