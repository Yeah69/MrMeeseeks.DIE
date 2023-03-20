using MrMeeseeks.DIE.Contexts;
using MrMeeseeks.DIE.Nodes.Functions;
using MrMeeseeks.DIE.Visitors;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.DIE.Nodes.Ranges;

internal interface ICreateContainerFunctionNode : INode
{
    string Name { get; }
    IReadOnlyList<(string TypeFullName, string Reference)> Parameters { get; }
    string ContainerTypeFullName { get; }
    string ContainerReference { get; }
    string? InitializationFunctionName { get; }
    bool InitializationAwaited { get; }
    string ReturnTypeFullName { get; }
}

internal class CreateContainerFunctionNode : ICreateContainerFunctionNode
{
    private readonly IVoidFunctionNode? _initializationFunction;
    private readonly INamedTypeSymbol _containerType;
    private readonly WellKnownTypes _wellKnownTypes;

    internal CreateContainerFunctionNode(
        // parameters
        IMethodSymbol constructor,
        IVoidFunctionNode? initializationFunction,
        
        // dependencies
        IContainerWideContext containerWideContext,
        IReferenceGenerator referenceGenerator,
        IContainerInfoContext containerInfoContext)
    {
        _initializationFunction = initializationFunction;
        Name = containerInfoContext.ContainerInfo.Name;
        ContainerTypeFullName = containerInfoContext.ContainerInfo.FullName;
        ContainerReference = referenceGenerator.Generate(containerInfoContext.ContainerInfo.ContainerType);
        Parameters = constructor.Parameters.Select(ps => (ps.Type.FullName(), ps.Name)).ToList();
        InitializationFunctionName = initializationFunction?.Name;
        _containerType = containerInfoContext.ContainerInfo.ContainerType;
        _wellKnownTypes = containerWideContext.WellKnownTypes;
    }
    
    public void Build(ImmutableStack<INamedTypeSymbol> implementationStack)
    {
    }

    public void Accept(INodeVisitor nodeVisitor) => 
        nodeVisitor.VisitCreateContainerFunctionNode(this);

    public string Name { get; }
    public IReadOnlyList<(string TypeFullName, string Reference)> Parameters { get; }
    public string ContainerTypeFullName { get; }
    public string ContainerReference { get; }
    public string? InitializationFunctionName { get; }

    public bool InitializationAwaited =>
        _initializationFunction?.SynchronicityDecision is {} synchronicityDecision
        && synchronicityDecision != SynchronicityDecision.Sync;

    public string ReturnTypeFullName => InitializationAwaited
        ? _wellKnownTypes.ValueTask1.Construct(_containerType).FullName()
        : ContainerTypeFullName;
}