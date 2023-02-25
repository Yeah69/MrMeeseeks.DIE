using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.DIE.Visitors;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ITransientScopeInterfaceNode : INode
{
    string FullName { get; }
    string Name { get; }
    IFunctionCallNode BuildTransientScopeInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction);
    IEnumerable<IRangedInstanceInterfaceFunctionNode> Functions { get; }
    void RegisterRange(IRangeNode range);
}

internal class TransientScopeInterfaceNode : ITransientScopeInterfaceNode
{
    private readonly IContainerNode _container;
    private readonly IReferenceGenerator _referenceGenerator;
    private readonly Dictionary<TypeKey, List<IRangedInstanceInterfaceFunctionNode>> _interfaceFunctions = new();
    private readonly Collection<IRangeNode> _ranges = new();
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IContainerNode, IRangeNode, IReferenceGenerator, IRangedInstanceInterfaceFunctionNode> _rangedInstanceInterfaceFunctionNodeFactory;

    internal TransientScopeInterfaceNode(
        IContainerNode container,
        IReferenceGenerator referenceGenerator,
        
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IContainerNode, IRangeNode, IReferenceGenerator, IRangedInstanceInterfaceFunctionNode> rangedInstanceInterfaceFunctionNodeFactory)
    {
        _container = container;
        
        _referenceGenerator = referenceGenerator;
        _rangedInstanceInterfaceFunctionNodeFactory = rangedInstanceInterfaceFunctionNodeFactory;
        Name = referenceGenerator.Generate("ITransientScope");
        FullName = $"{container.FullName}.{Name}";
    }
    
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => nodeVisitor.VisitTransientScopeInterfaceNode(this);

    public string FullName { get; }
    public string Name { get; }
    public IFunctionCallNode BuildTransientScopeInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction) =>
        FunctionResolutionUtility.GetOrCreateFunctionCall(
            type,
            callingFunction,
            _interfaceFunctions,
            () =>
            {
                var interfaceFunction = _rangedInstanceInterfaceFunctionNodeFactory(
                        type,
                        callingFunction.Overrides.Select(kvp => kvp.Key).ToList(),
                        _container,
                        _container,
                        _referenceGenerator)
                    .EnqueueBuildJobTo(_container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
                foreach (var range in _ranges)
                    interfaceFunction.AddConsideredRange(range);
                return interfaceFunction;
            },
            f => f.CreateCall(ownerReference, callingFunction, callingFunction));

    public IEnumerable<IRangedInstanceInterfaceFunctionNode> Functions => _interfaceFunctions.Values.SelectMany(x => x);

    public void RegisterRange(IRangeNode range)
    {
        foreach (var interfaceFunction in Functions)
            interfaceFunction.AddConsideredRange(range);
        _ranges.Add(range);
    }
}