using MrMeeseeks.DIE.Extensions;
using MrMeeseeks.DIE.MsContainer;
using MrMeeseeks.DIE.Nodes.Elements.FunctionCalls;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Nodes.Roots;
using MrMeeseeks.DIE.Utility;
using MrMeeseeks.SourceGeneratorUtility;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ITransientScopeInterfaceNode : INode
{
    string FullName { get; }
    string Name { get; }
    IFunctionCallNode BuildTransientScopeInstanceCall(string? ownerReference, INamedTypeSymbol type, IFunctionNode callingFunction);
    IEnumerable<IRangedInstanceInterfaceFunctionNode> Functions { get; }
    void RegisterRange(IRangeNode range);
}

internal partial class TransientScopeInterfaceNode : ITransientScopeInterfaceNode, IContainerInstance
{
    private readonly IContainerNode _container;
    private readonly Dictionary<ITypeSymbol, List<IRangedInstanceInterfaceFunctionNode>> _interfaceFunctions = new(CustomSymbolEqualityComparer.IncludeNullability);
    private readonly Collection<IRangeNode> _ranges = new();
    private readonly Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangedInstanceInterfaceFunctionNodeRoot> _rangedInstanceInterfaceFunctionNodeFactory;

    internal TransientScopeInterfaceNode(
        IContainerNode container,
        
        IReferenceGenerator referenceGenerator,
        Func<INamedTypeSymbol, IReadOnlyList<ITypeSymbol>, IRangedInstanceInterfaceFunctionNodeRoot> rangedInstanceInterfaceFunctionNodeFactory)
    {
        _container = container;
        
        _rangedInstanceInterfaceFunctionNodeFactory = rangedInstanceInterfaceFunctionNodeFactory;
        Name = referenceGenerator.Generate("ITransientScope");
        FullName = $"{container.FullName}.{Name}";
    }
    
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

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
                        callingFunction.Overrides.Select(kvp => kvp.Key).ToList())
                    .Function
                    .EnqueueBuildJobTo(_container.BuildQueue, ImmutableStack<INamedTypeSymbol>.Empty);
                foreach (var range in _ranges)
                    interfaceFunction.AddConsideredRange(range);
                return interfaceFunction;
            },
            f => f.CreateCall(ownerReference, callingFunction));

    public IEnumerable<IRangedInstanceInterfaceFunctionNode> Functions => _interfaceFunctions.Values.SelectMany(x => x);

    public void RegisterRange(IRangeNode range)
    {
        foreach (var interfaceFunction in Functions)
            interfaceFunction.AddConsideredRange(range);
        _ranges.Add(range);
    }
}